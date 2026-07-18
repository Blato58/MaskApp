using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Preflight;

namespace MaskApp.Core.Tests.Features.Preflight;

public sealed class FestivalShowPreparationServiceTests
{
    [Fact]
    public async Task PrepareAsync_UploadsWithoutPlaying_AndRecordsOnlyActiveMaskSlot()
    {
        var profileStore = new InMemoryMaskProfileStore();
        var rawFaceStore = new InMemoryFacePatternStore();
        var session = new MaskProfileSession(profileStore, rawFaceStore);
        var faceStore = new ProfiledFacePatternStore(rawFaceStore, session);
        var firstMask = new DiscoveredMaskDevice("mask-a", "Mask A", -40);
        var secondMask = new DiscoveredMaskDevice("mask-b", "Mask B", -50);
        await session.ActivateAsync(firstMask);
        await session.ObserveCapabilitiesAsync(new MaskCapabilitySnapshot
        {
            CommandWriteAvailable = true,
            TextUploadAvailable = true,
            FaceUploadAvailable = true,
            AcknowledgementMode = MaskAcknowledgementMode.Acknowledged,
            DiySlotCapacity = 20,
            TransportName = "Fake"
        });

        var face = FacePatternFactory.CreateBlank("Show face", preferredSlot: 7);
        var fingerprint = FaceContentFingerprint.Compute(face);
        var item = new GalleryItem
        {
            Id = $"face:{face.Id}",
            Type = GalleryItemType.CustomStaticFace,
            Title = face.DisplayName,
            FacePattern = face
        };
        var allocation = new DiySlotAllocation(
            "requirement",
            item.Id,
            fingerprint,
            PreferredSlot: 7,
            AssignedSlot: 7,
            IsPrepared: false,
            Verification: null,
            ReplacedSourceId: string.Empty);
        var report = new FestivalPreflightReport(
            FestivalPreflightStatus.Degraded,
            "DEGRADED",
            [],
            [allocation],
            []);
        var transport = new RecordingFaceUploadTransport();
        var service = new FestivalShowPreparationService(faceStore, transport);

        var result = await service.PrepareAsync(report, [item]);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.UploadedSlotCount);
        Assert.False(Assert.Single(transport.Options).PlayAfterUpload);
        var firstProfile = await session.GetActiveProfileAsync();
        var prepared = Assert.Single(firstProfile!.PreparedSlots);
        Assert.Equal(fingerprint, prepared.ContentFingerprint);
        Assert.Equal(MaskPreparedSlotVerification.Acknowledged, prepared.Verification);

        var secondProfile = await session.ActivateAsync(secondMask);
        Assert.Empty(secondProfile.PreparedSlots);
        Assert.Empty((await faceStore.LoadAsync()).SlotInstallations);
    }

    private sealed class RecordingFaceUploadTransport : IFaceUploadTransport
    {
        public event EventHandler<FaceUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        public FaceUploadTransportState State => FaceUploadTransportState.Ready;

        public string StatusText => "Ready.";

        public List<FaceUploadOptions> Options { get; } = [];

        public Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            Options.Add(options);
            return Task.FromResult(FaceUploadResult.Success("Uploaded.", package.Frames.Count));
        }
    }
}
