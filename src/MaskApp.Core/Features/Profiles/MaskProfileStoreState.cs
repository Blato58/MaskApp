namespace MaskApp.Core.Features.Profiles;

public sealed record MaskProfileStoreState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public string ActiveProfileId { get; init; } = string.Empty;

    public bool LegacyGlobalSlotLedgerMigrated { get; init; }

    public IReadOnlyList<MaskProfile> Profiles { get; init; } = [];

    public string Status { get; init; } = "Ready.";

    public bool UsedFallback { get; init; }

    public MaskProfileStoreState Normalize()
    {
        var profiles = (Profiles ?? [])
            .Select(profile => profile.Normalize())
            .Where(profile => !string.IsNullOrWhiteSpace(profile.ProfileId))
            .GroupBy(profile => profile.ProfileId, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(profile => profile.LastSeenAt).First())
            .OrderByDescending(profile => profile.LastSeenAt)
            .ToArray();
        var activeProfileId = profiles.Any(profile =>
                string.Equals(profile.ProfileId, ActiveProfileId, StringComparison.Ordinal))
            ? ActiveProfileId
            : string.Empty;

        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            ActiveProfileId = activeProfileId,
            Profiles = profiles,
            Status = string.IsNullOrWhiteSpace(Status) ? "Ready." : Status.Trim()
        };
    }

    public MaskProfile? GetActiveProfile() =>
        Normalize().Profiles.FirstOrDefault(profile =>
            string.Equals(profile.ProfileId, ActiveProfileId, StringComparison.Ordinal));

    public MaskProfileStoreState Upsert(MaskProfile profile, bool makeActive = false)
    {
        var normalized = Normalize();
        var normalizedProfile = profile.Normalize();
        var profiles = normalized.Profiles
            .Where(existing => !string.Equals(existing.ProfileId, normalizedProfile.ProfileId, StringComparison.Ordinal))
            .Append(normalizedProfile)
            .ToArray();
        return normalized with
        {
            ActiveProfileId = makeActive ? normalizedProfile.ProfileId : normalized.ActiveProfileId,
            Profiles = profiles
        };
    }
}
