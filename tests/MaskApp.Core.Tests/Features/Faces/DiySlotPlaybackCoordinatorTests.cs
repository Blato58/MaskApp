using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class DiySlotPlaybackCoordinatorTests
{
    [Fact]
    public async Task PlayFaceAsync_FirstPlayUploadsOnce_SecondPlayUsesStoredSlot()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(store, faceTransport, commandTransport);
        var face = FacePatternFactory.CreateBuiltIns().Single(item => item.DisplayName == "Holy Priest · Cross");

        var first = await coordinator.PlayFaceAsync(face);
        var second = await coordinator.PlayFaceAsync(face);

        Assert.True(first.Succeeded);
        Assert.Equal(1, first.UploadedSlotCount);
        Assert.True(second.Succeeded);
        Assert.Equal(0, second.UploadedSlotCount);
        Assert.Equal(1, second.ReusedSlotCount);
        Assert.Single(faceTransport.Packages);
        Assert.False(Assert.Single(faceTransport.Options).PlayAfterUpload);
        Assert.Equal(2, commandTransport.Commands.Count);
        Assert.All(commandTransport.Commands, command => Assert.Equal(MaskCommandKind.FacePlay, command.Kind));
        Assert.Contains("no upload", second.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PlayAnimationAsync_UploadsMissingFramesOnce_ThenSendsOnlyPlaySequence()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.Zero);
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];

        var first = await coordinator.PlayAnimationAsync(animation);
        var second = await coordinator.PlayAnimationAsync(animation);

        Assert.True(first.Succeeded);
        Assert.Equal(animation.Frames.Count, first.UploadedSlotCount);
        Assert.True(second.Succeeded);
        Assert.Equal(0, second.UploadedSlotCount);
        Assert.Equal(animation.Frames.Count, second.ReusedSlotCount);
        Assert.Equal(animation.Frames.Count, faceTransport.Packages.Count);
        Assert.All(faceTransport.Options, options => Assert.False(options.PlayAfterUpload));
        var expectedSlots = animation.PlaybackSlots
            .Concat(animation.PlaybackSlots)
            .Select(slot => (byte)slot)
            .ToArray();
        Assert.Equal(expectedSlots.Length, commandTransport.Commands.Count);
        Assert.All(commandTransport.Commands, command =>
        {
            Assert.Equal(MaskCommandKind.FacePlay, command.Kind);
            Assert.Equal(1, command.Plaintext.Span[5]);
        });
        Assert.Equal(
            expectedSlots,
            commandTransport.Commands.Select(command => command.Plaintext.Span[6]).ToArray());
        Assert.True(DiySlotPlaybackCoordinator.IsAnimationPrepared(animation, await store.LoadAsync()));
    }

    [Fact]
    public async Task PrepareAnimationAsync_DoesNotPlayAndReusesPreparedFrames()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(store, faceTransport, commandTransport);
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[1];

        var first = await coordinator.PrepareAnimationAsync(animation);
        var second = await coordinator.PrepareAnimationAsync(animation);

        Assert.True(first.Succeeded);
        Assert.False(first.PlayCommandSent);
        Assert.Equal(animation.Frames.Count, first.UploadedSlotCount);
        Assert.True(second.Succeeded);
        Assert.Equal(0, second.UploadedSlotCount);
        Assert.Empty(commandTransport.Commands);
        Assert.Equal(animation.Frames.Count, faceTransport.Packages.Count);
    }

    [Fact]
    public async Task RefreshAnimationAsync_ReuploadsEveryFrameAndClearsStaleStateBeforeFailure()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(store, faceTransport, commandTransport);
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];

        var prepared = await coordinator.PrepareAnimationAsync(animation);
        faceTransport.FailOnUploadNumber = faceTransport.Packages.Count + 2;
        var refresh = await coordinator.RefreshAnimationAsync(animation);

        Assert.True(prepared.Succeeded);
        Assert.False(refresh.Succeeded);
        Assert.Equal(1, refresh.UploadedSlotCount);
        Assert.Empty(commandTransport.Commands);
        Assert.Equal(animation.Frames.Count + 2, faceTransport.Packages.Count);

        var state = await store.LoadAsync();
        Assert.NotNull(state.GetSlotInstallation(animation.Frames[0].Slot));
        Assert.Null(state.GetSlotInstallation(animation.Frames[1].Slot));
    }

    [Fact]
    public async Task PlayAnimationAsync_AfterPartialFailure_RetryUploadsOnlyRemainingFrames()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport { FailOnUploadNumber = 2 };
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.Zero);
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];

        var failed = await coordinator.PlayAnimationAsync(animation);

        Assert.False(failed.Succeeded);
        Assert.Equal(1, failed.UploadedSlotCount);
        Assert.Empty(commandTransport.Commands);
        var partialState = await store.LoadAsync();
        Assert.NotNull(partialState.GetSlotInstallation(animation.Frames[0].Slot));
        Assert.Null(partialState.GetSlotInstallation(animation.Frames[1].Slot));

        faceTransport.FailOnUploadNumber = null;
        var retried = await coordinator.PlayAnimationAsync(animation);

        Assert.True(retried.Succeeded);
        Assert.Equal(1, retried.UploadedSlotCount);
        Assert.Equal(1, retried.ReusedSlotCount);
        Assert.Equal(3, faceTransport.Packages.Count);
        Assert.Equal(animation.PlaybackSlots.Count, commandTransport.Commands.Count);
    }

    [Fact]
    public async Task PlayAnimationAsync_StopsRapidPlaybackWhenACommandFails()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport { FailOnCommandNumber = 3 };
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.Zero);
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[1];

        var result = await coordinator.PlayAnimationAsync(animation);

        Assert.False(result.Succeeded);
        Assert.False(result.PlayCommandSent);
        Assert.Equal(animation.Frames.Count, result.UploadedSlotCount);
        Assert.Equal(3, commandTransport.Commands.Count);
        Assert.Contains("failed", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingFaceTransport : IFaceUploadTransport
    {
        public event EventHandler<FaceUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Face recorder";

        public bool IsSimulated => true;

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        public FaceUploadTransportState State => FaceUploadTransportState.Ready;

        public string StatusText => "Ready.";

        public List<FaceUploadPackage> Packages { get; } = [];

        public List<FaceUploadOptions> Options { get; } = [];

        public int? FailOnUploadNumber { get; set; }

        public Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            Packages.Add(package);
            Options.Add(options);
            if (Packages.Count == FailOnUploadNumber)
            {
                return Task.FromResult(FaceUploadResult.Failure("Upload failed.", 0));
            }

            return Task.FromResult(FaceUploadResult.Success("Uploaded.", package.Frames.Count));
        }
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Command recorder";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

        public string TransportStatusText => "Ready.";

        public List<MaskCommand> Commands { get; } = [];

        public int? FailOnCommandNumber { get; init; }

        public Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command);
            if (Commands.Count == FailOnCommandNumber)
            {
                return Task.FromResult(MaskCommandResult.Failure("Command failed."));
            }

            return Task.FromResult(MaskCommandResult.Success("Sent."));
        }
    }
}
