using System.Security.Cryptography;
using System.Text;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Audio;

namespace MaskApp.Core.Features.Profiles;

public sealed class MaskProfileSession
{
    private readonly object stateLock = new();
    private readonly SemaphoreSlim mutationGate = new(1, 1);
    private readonly IMaskProfileStore profileStore;
    private readonly IFacePatternStore? legacyFaceStore;
    private readonly Func<DateTimeOffset> getUtcNow;
    private string activeProfileId = string.Empty;

    public MaskProfileSession(
        IMaskProfileStore profileStore,
        IFacePatternStore? legacyFaceStore = null,
        Func<DateTimeOffset>? getUtcNow = null)
    {
        this.profileStore = profileStore;
        this.legacyFaceStore = legacyFaceStore;
        this.getUtcNow = getUtcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public event EventHandler<MaskProfileChangedEventArgs>? ActiveProfileChanged;

    public string ActiveProfileId
    {
        get
        {
            lock (stateLock)
            {
                return activeProfileId;
            }
        }
    }

    public static string DeriveProfileId(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        var normalizedDeviceId = deviceId.Trim().ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedDeviceId));
        return $"mask-{Convert.ToHexString(hash.AsSpan(0, 16)).ToLowerInvariant()}";
    }

    public async Task<MaskProfile> ActivateAsync(
        DiscoveredMaskDevice device,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);
        var timestamp = getUtcNow();
        MaskProfile activeProfile;

        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var profileId = DeriveProfileId(device.Id);
            var state = (await profileStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            EnsureStoreIsWritable(state);
            var profile = state.Profiles.FirstOrDefault(existing =>
                    string.Equals(existing.ProfileId, profileId, StringComparison.Ordinal))
                ?? new MaskProfile
                {
                    ProfileId = profileId,
                    DisplayName = device.Name,
                    FirstSeenAt = timestamp,
                    LastSeenAt = timestamp
                };
            profile = profile.Normalize() with
            {
                DisplayName = string.IsNullOrWhiteSpace(device.Name) ? profile.DisplayName : device.Name.Trim(),
                LastSeenAt = timestamp
            };

            FacePatternStoreState? legacyState = null;
            if (legacyFaceStore is not null)
            {
                legacyState = (await legacyFaceStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            }

            if (!state.LegacyGlobalSlotLedgerMigrated)
            {
                var existingSlots = profile.PreparedSlots.ToDictionary(slot => slot.Slot);
                foreach (var installation in legacyState?.SlotInstallations ?? [])
                {
                    existingSlots.TryAdd(installation.Slot, MaskPreparedSlot.FromLegacy(installation));
                }

                profile = profile with
                {
                    PreparedSlots = existingSlots.Values.OrderBy(slot => slot.Slot).ToArray(),
                    PreparedStateStatus = existingSlots.Count == 0
                        ? "No legacy prepared slots were present."
                        : "Legacy global prepared slots were assigned to this mask and require verification."
                };
                state = state with { LegacyGlobalSlotLedgerMigrated = true };
            }

            state = state.Upsert(profile, makeActive: true);
            await profileStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);

            if (legacyFaceStore is not null && legacyState is { SlotInstallations.Count: > 0 })
            {
                await legacyFaceStore
                    .SaveAsync(legacyState with { SlotInstallations = [] }, cancellationToken)
                    .ConfigureAwait(false);
            }

            activeProfile = state.Profiles.First(item =>
                string.Equals(item.ProfileId, profileId, StringComparison.Ordinal));
            SetActiveProfileId(profileId);
        }
        finally
        {
            mutationGate.Release();
        }

        ActiveProfileChanged?.Invoke(this, new MaskProfileChangedEventArgs(activeProfile));
        return activeProfile;
    }

    public async Task<MaskProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = (await profileStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            EnsureStoreIsWritable(state);
            var profile = state.GetActiveProfile();
            SetActiveProfileId(profile?.ProfileId ?? string.Empty);
            return profile;
        }
        finally
        {
            mutationGate.Release();
        }
    }

    public async Task<MaskProfile?> ObserveCapabilitiesAsync(
        MaskCapabilitySnapshot capabilities,
        CancellationToken cancellationToken = default)
        => await ObserveCapabilitiesForProfileAsync(
                expectedProfileId: null,
                capabilities,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<MaskProfile?> ObserveCapabilitiesForProfileAsync(
        string? expectedProfileId,
        MaskCapabilitySnapshot capabilities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        var timestamp = getUtcNow();
        MaskProfile? updatedProfile = null;

        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = (await profileStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            EnsureStoreIsWritable(state);
            var profile = state.GetActiveProfile();
            if (profile is null
                || expectedProfileId is not null
                && !string.Equals(profile.ProfileId, expectedProfileId, StringComparison.Ordinal))
            {
                return null;
            }

            updatedProfile = profile.ObserveCapabilities(capabilities, timestamp);
            state = state.Upsert(updatedProfile, makeActive: true);
            await profileStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            SetActiveProfileId(updatedProfile.ProfileId);
        }
        finally
        {
            mutationGate.Release();
        }

        ActiveProfileChanged?.Invoke(this, new MaskProfileChangedEventArgs(updatedProfile));
        return updatedProfile;
    }

    public async Task<MaskProfile?> ReplacePreparedSlotsAsync(
        IReadOnlyList<FaceSlotInstallation> installations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(installations);
        MaskProfile? updatedProfile = null;

        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = (await profileStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            EnsureStoreIsWritable(state);
            var profile = state.GetActiveProfile();
            if (profile is null)
            {
                return null;
            }

            var normalizedProfile = profile.Normalize();
            var existing = normalizedProfile.PreparedSlots.ToDictionary(slot => slot.Slot);
            var verificationForNewContent = normalizedProfile.Capabilities.AcknowledgementMode ==
                MaskAcknowledgementMode.Acknowledged
                    ? MaskPreparedSlotVerification.Acknowledged
                    : MaskPreparedSlotVerification.WriteOnlyUnverified;
            var preparedSlots = installations
                .Select(installation => installation.Normalize())
                .Where(installation => !string.IsNullOrWhiteSpace(installation.ContentFingerprint))
                .Select(installation =>
                {
                    var verification = existing.TryGetValue(installation.Slot, out var previous)
                        && string.Equals(
                            previous.ContentFingerprint,
                            installation.ContentFingerprint,
                            StringComparison.OrdinalIgnoreCase)
                            ? previous.Verification
                            : verificationForNewContent;
                    return new MaskPreparedSlot
                    {
                        Slot = installation.Slot,
                        ContentFingerprint = installation.ContentFingerprint,
                        SourceId = installation.SourceId,
                        InstalledAt = installation.InstalledAt,
                        Verification = verification
                    }.Normalize();
                })
                .OrderBy(slot => slot.Slot)
                .ToArray();
            updatedProfile = normalizedProfile with
            {
                PreparedSlots = preparedSlots,
                PreparedStateStatus = preparedSlots.Length == 0
                    ? "No prepared slots recorded."
                    : $"{preparedSlots.Length} prepared slot(s) recorded for this mask."
            };
            state = state.Upsert(updatedProfile, makeActive: true);
            await profileStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            mutationGate.Release();
        }

        ActiveProfileChanged?.Invoke(this, new MaskProfileChangedEventArgs(updatedProfile));
        return updatedProfile;
    }

    public async Task<MaskProfile?> RecordAudioVisualizationEvidenceAsync(
        AudioVisualizationEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        MaskProfile? updatedProfile = null;

        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = (await profileStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            EnsureStoreIsWritable(state);
            var profile = state.GetActiveProfile();
            if (profile is null)
            {
                return null;
            }

            var normalizedEvidence = evidence.Normalize();
            updatedProfile = profile.Normalize() with
            {
                AudioVisualizationEvidence = normalizedEvidence,
                SustainableCadenceHz = normalizedEvidence.EnablesLiveMicrophone
                    ? normalizedEvidence.ObservedWriteCadenceHz
                    : profile.SustainableCadenceHz,
                LastSeenAt = getUtcNow()
            };
            state = state.Upsert(updatedProfile, makeActive: true);
            await profileStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            mutationGate.Release();
        }

        ActiveProfileChanged?.Invoke(this, new MaskProfileChangedEventArgs(updatedProfile));
        return updatedProfile;
    }

    public async Task<MaskProfile?> RecordCommandLatencyForProfileAsync(
        string expectedProfileId,
        TimeSpan latency,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedProfileId);
        if (latency <= TimeSpan.Zero || !double.IsFinite(latency.TotalMilliseconds))
        {
            throw new ArgumentOutOfRangeException(nameof(latency), latency, "Latency must be finite and positive.");
        }

        MaskProfile? updatedProfile = null;
        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = (await profileStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            EnsureStoreIsWritable(state);
            var profile = state.GetActiveProfile();
            if (profile is null
                || !string.Equals(profile.ProfileId, expectedProfileId, StringComparison.Ordinal))
            {
                return null;
            }

            const double latestSampleWeight = 0.2;
            var priorAverage = profile.AverageCommandLatencyMilliseconds;
            var average = priorAverage is > 0
                ? (priorAverage.Value * (1 - latestSampleWeight))
                    + (latency.TotalMilliseconds * latestSampleWeight)
                : latency.TotalMilliseconds;
            updatedProfile = profile.Normalize() with
            {
                AverageCommandLatencyMilliseconds = Math.Round(average, 3),
                LastSeenAt = getUtcNow()
            };
            state = state.Upsert(updatedProfile, makeActive: true);
            await profileStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            mutationGate.Release();
        }

        ActiveProfileChanged?.Invoke(this, new MaskProfileChangedEventArgs(updatedProfile));
        return updatedProfile;
    }

    private void SetActiveProfileId(string profileId)
    {
        lock (stateLock)
        {
            activeProfileId = profileId;
        }
    }

    private static void EnsureStoreIsWritable(MaskProfileStoreState state)
    {
        if (state.UsedFallback)
        {
            throw new InvalidOperationException(
                "Mask profiles require recovery before they can be changed; the unreadable file was not overwritten.");
        }
    }
}
