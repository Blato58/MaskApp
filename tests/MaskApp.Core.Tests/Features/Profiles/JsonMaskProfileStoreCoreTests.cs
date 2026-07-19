using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Profiles;

namespace MaskApp.Core.Tests.Features.Profiles;

public sealed class JsonMaskProfileStoreCoreTests
{
    [Fact]
    public async Task SaveAndLoad_RoundTripsVersionedProfilesAtomically()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maskapp-profile-tests-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "mask-profiles.json");
        try
        {
            var store = new JsonMaskProfileStoreCore(filePath);
            var profile = new MaskProfile
            {
                ProfileId = "mask-test",
                DisplayName = "Test Mask",
                FirstSeenAt = DateTimeOffset.Parse("2026-07-17T10:00:00Z"),
                LastSeenAt = DateTimeOffset.Parse("2026-07-17T11:00:00Z")
            };
            var state = new MaskProfileStoreState
            {
                ActiveProfileId = profile.ProfileId,
                LegacyGlobalSlotLedgerMigrated = true,
                Profiles = [profile]
            };

            await store.SaveAsync(state);
            var loaded = await store.LoadAsync();

            Assert.False(loaded.UsedFallback);
            Assert.True(loaded.LegacyGlobalSlotLedgerMigrated);
            Assert.Equal("Test Mask", Assert.Single(loaded.Profiles).DisplayName);
            Assert.Equal("mask-test", loaded.ActiveProfileId);
            Assert.False(File.Exists($"{filePath}.tmp"));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CorruptDocument_ReturnsExplicitEmptyFallback()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maskapp-profile-tests-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "mask-profiles.json");
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(filePath, "{not-json");
            var loaded = await new JsonMaskProfileStoreCore(filePath).LoadAsync();

            Assert.True(loaded.UsedFallback);
            Assert.Empty(loaded.Profiles);
            Assert.Contains("could not be read", loaded.Status, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CorruptDocument_IsNotSilentlyOverwrittenByProfileActivation()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maskapp-profile-tests-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "mask-profiles.json");
        Directory.CreateDirectory(directory);
        const string corruptContent = "{not-json";
        try
        {
            await File.WriteAllTextAsync(filePath, corruptContent);
            var session = new MaskProfileSession(new JsonMaskProfileStoreCore(filePath));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                session.ActivateAsync(new("mask-a", "Mask A", -40)));

            Assert.Contains("not overwritten", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(corruptContent, await File.ReadAllTextAsync(filePath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task PreAudioProfileDocument_LoadsInPlaceWithSafeNewFieldDefaults()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maskapp-profile-tests-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "mask-profiles.json");
        Directory.CreateDirectory(directory);
        try
        {
            const string legacyJson = """
                {
                  "schemaVersion": 1,
                  "activeProfileId": "mask-legacy",
                  "legacyGlobalSlotLedgerMigrated": true,
                  "profiles": [
                    {
                      "profileId": "mask-legacy",
                      "displayName": "Legacy Mask",
                      "firstSeenAt": "2026-07-17T10:00:00Z",
                      "lastSeenAt": "2026-07-17T11:00:00Z",
                      "capabilities": {
                        "commandWriteAvailable": true,
                        "textUploadAvailable": true,
                        "faceUploadAvailable": true,
                        "acknowledgementMode": "writeOnly",
                        "diySlotCapacity": 20,
                        "transportName": "iOS CoreBluetooth"
                      },
                      "preparedSlots": [
                        {
                          "slot": 7,
                          "contentFingerprint": "LEGACY-CONTENT",
                          "verification": "acknowledged",
                          "installedAt": "2026-07-17T10:30:00Z"
                        }
                      ]
                    }
                  ]
                }
                """;
            await File.WriteAllTextAsync(filePath, legacyJson);

            var loaded = await new JsonMaskProfileStoreCore(filePath).LoadAsync();

            Assert.False(loaded.UsedFallback);
            var profile = Assert.Single(loaded.Profiles);
            Assert.Equal("mask-legacy", loaded.ActiveProfileId);
            Assert.Single(profile.PreparedSlots);
            Assert.False(profile.Capabilities.AudioVisualizationWriteAvailable);
            Assert.Equal(
                AudioVisualizationEvidenceStatus.Unknown,
                profile.AudioVisualizationEvidence.Status);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
