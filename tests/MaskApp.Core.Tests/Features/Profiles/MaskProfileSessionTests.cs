using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Profiles;

namespace MaskApp.Core.Tests.Features.Profiles;

public sealed class MaskProfileSessionTests
{
    private static readonly DiscoveredMaskDevice FirstMask = new("device-a", "Festival Mask A", -42);
    private static readonly DiscoveredMaskDevice SecondMask = new("device-b", "Festival Mask B", -55);

    [Fact]
    public async Task AlternatingMasks_NeverSharePreparedSlotClaims()
    {
        var profileStore = new InMemoryMaskProfileStore();
        var rawFaceStore = new InMemoryFacePatternStore();
        var now = DateTimeOffset.Parse("2026-07-17T12:00:00Z");
        var session = new MaskProfileSession(profileStore, rawFaceStore, () => now);
        var faceStore = new ProfiledFacePatternStore(rawFaceStore, session);

        await session.ActivateAsync(FirstMask);
        var firstState = await faceStore.LoadAsync();
        await faceStore.SaveAsync(firstState.MarkSlotInstalled(7, "FIRST", "first-face", now));

        now = now.AddMinutes(1);
        var secondProfile = await session.ActivateAsync(SecondMask);
        var secondState = await faceStore.LoadAsync();
        Assert.Empty(secondProfile.PreparedSlots);
        Assert.Null(secondState.GetSlotInstallation(7));
        await faceStore.SaveAsync(secondState.MarkSlotInstalled(7, "SECOND", "second-face", now));

        now = now.AddMinutes(1);
        var restoredFirstProfile = await session.ActivateAsync(FirstMask);
        var restoredFirstState = await faceStore.LoadAsync();
        Assert.Equal("FIRST", Assert.Single(restoredFirstProfile.PreparedSlots).ContentFingerprint);
        Assert.Equal("FIRST", restoredFirstState.GetSlotInstallation(7)?.ContentFingerprint);

        var persisted = await profileStore.LoadAsync();
        Assert.Equal(2, persisted.Profiles.Count);
        Assert.Equal(
            "SECOND",
            persisted.Profiles
                .Single(profile => profile.ProfileId == MaskProfileSession.DeriveProfileId(SecondMask.Id))
                .PreparedSlots
                .Single()
                .ContentFingerprint);
    }

    [Fact]
    public async Task FirstIdentifiedMask_ReceivesLegacySlotsAsUnverified_ExactlyOnce()
    {
        var installedAt = DateTimeOffset.Parse("2026-07-16T20:00:00Z");
        var rawState = FacePatternStoreState.Seeded.MarkSlotInstalled(12, "LEGACY", "old-animation", installedAt);
        var rawFaceStore = new InMemoryFacePatternStore(rawState);
        var profileStore = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(
            profileStore,
            rawFaceStore,
            () => DateTimeOffset.Parse("2026-07-17T12:00:00Z"));

        var firstProfile = await session.ActivateAsync(FirstMask);

        var migratedSlot = Assert.Single(firstProfile.PreparedSlots);
        Assert.Equal(12, migratedSlot.Slot);
        Assert.Equal("LEGACY", migratedSlot.ContentFingerprint);
        Assert.Equal(MaskPreparedSlotVerification.UnverifiedLegacy, migratedSlot.Verification);
        Assert.Empty((await rawFaceStore.LoadAsync()).SlotInstallations);
        Assert.True((await profileStore.LoadAsync()).LegacyGlobalSlotLedgerMigrated);

        var secondProfile = await session.ActivateAsync(SecondMask);
        Assert.Empty(secondProfile.PreparedSlots);
    }

    [Fact]
    public async Task CapabilityChange_InvalidatesPreparedSlots_WhileSameCapabilitiesPreserveThem()
    {
        var now = DateTimeOffset.Parse("2026-07-17T12:00:00Z");
        var profileStore = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(profileStore, getUtcNow: () => now);
        await session.ActivateAsync(FirstMask);
        var acknowledged = CreateCapabilities(MaskAcknowledgementMode.Acknowledged);
        await session.ObserveCapabilitiesAsync(acknowledged);
        await session.ReplacePreparedSlotsAsync(
        [
            new FaceSlotInstallation
            {
                Slot = 9,
                ContentFingerprint = "CONTENT",
                SourceId = "scene-1",
                InstalledAt = now
            }
        ]);

        now = now.AddMinutes(1);
        var unchanged = await session.ObserveCapabilitiesAsync(acknowledged);
        Assert.Equal(MaskPreparedSlotVerification.Acknowledged, Assert.Single(unchanged!.PreparedSlots).Verification);

        now = now.AddMinutes(1);
        var changed = await session.ObserveCapabilitiesAsync(
            CreateCapabilities(MaskAcknowledgementMode.WriteOnly));
        Assert.Empty(changed!.PreparedSlots);
        Assert.Contains("invalidated", changed.PreparedStateStatus, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProfileId_IsStableCaseInsensitive_AndDoesNotExposeRawDeviceId()
    {
        var lower = MaskProfileSession.DeriveProfileId("aa:bb:cc:dd:ee:ff");
        var upper = MaskProfileSession.DeriveProfileId("AA:BB:CC:DD:EE:FF");

        Assert.Equal(lower, upper);
        Assert.StartsWith("mask-", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("aa:bb:cc:dd:ee:ff", lower, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(37, lower.Length);
    }

    private static MaskCapabilitySnapshot CreateCapabilities(MaskAcknowledgementMode acknowledgementMode) =>
        new()
        {
            CommandWriteAvailable = true,
            TextUploadAvailable = true,
            FaceUploadAvailable = true,
            AcknowledgementMode = acknowledgementMode,
            DiySlotCapacity = 20,
            TransportName = "Fake BLE"
        };
}
