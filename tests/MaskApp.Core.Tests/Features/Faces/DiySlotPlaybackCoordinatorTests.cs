using System.Collections.Concurrent;
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
    public async Task PlayAnimationAsync_LoopsBeyondConfiguredSequence_StopsCleanly_AndReusesFrames()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.FromMilliseconds(1));
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];

        var first = await coordinator.PlayAnimationAsync(animation);
        await WaitUntilAsync(() => commandTransport.Commands.Count >= animation.PlaybackSlots.Count + 3);
        Assert.True(coordinator.IsAnimationPlaying);
        await coordinator.StopAnimationAsync();
        var commandCountAfterStop = commandTransport.Commands.Count;
        await Task.Delay(10);

        Assert.True(first.Succeeded);
        Assert.Equal(animation.Frames.Count, first.UploadedSlotCount);
        Assert.Contains("continuous", first.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(coordinator.IsAnimationPlaying);
        Assert.Equal(commandCountAfterStop, commandTransport.Commands.Count);
        var sentCommands = commandTransport.Commands.ToArray();
        Assert.All(sentCommands, command =>
        {
            Assert.Equal(MaskCommandKind.FacePlay, command.Kind);
            Assert.Equal(1, command.Plaintext.Span[5]);
        });
        Assert.Equal(
            sentCommands
                .Select((_, index) => (byte)animation.PlaybackSlots[index % animation.PlaybackSlots.Count]),
            sentCommands.Select(command => command.Plaintext.Span[6]));

        var second = await coordinator.PlayAnimationAsync(animation);
        await coordinator.StopAnimationAsync();

        Assert.True(second.Succeeded);
        Assert.Equal(0, second.UploadedSlotCount);
        Assert.Equal(animation.Frames.Count, second.ReusedSlotCount);
        Assert.Equal(animation.Frames.Count, faceTransport.Packages.Count);
        Assert.All(faceTransport.Options, options => Assert.False(options.PlayAfterUpload));
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
        var coordinator = new DiySlotPlaybackCoordinator(store, faceTransport, commandTransport);
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
        Assert.Single(commandTransport.Commands);
        Assert.Equal(MaskCommandKind.FacePlay, commandTransport.Commands.Single().Kind);
        await coordinator.StopAnimationAsync();
    }

    [Fact]
    public async Task PlayAnimationAsync_StopsContinuousPlaybackWhenALaterPlayCommandFails()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport { FailOnCommandNumber = 3 };
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.FromMilliseconds(1));
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[1];

        var result = await coordinator.PlayAnimationAsync(animation);
        await WaitUntilAsync(() => commandTransport.Commands.Count >= 3);
        await WaitUntilAsync(() => !coordinator.IsAnimationPlaying);

        Assert.True(result.Succeeded);
        Assert.True(result.PlayCommandSent);
        Assert.Equal(animation.Frames.Count, result.UploadedSlotCount);
        Assert.Equal(3, commandTransport.Commands.Count);
        Assert.All(commandTransport.Commands, command => Assert.Equal(MaskCommandKind.FacePlay, command.Kind));
    }

    [Fact]
    public async Task PlayAnimationAsync_ReturnsFailureWhenInitialPlayCommandFails()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport { FailOnCommandNumber = 1 };
        var coordinator = new DiySlotPlaybackCoordinator(store, faceTransport, commandTransport);
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[1];

        var result = await coordinator.PlayAnimationAsync(animation);

        Assert.False(result.Succeeded);
        Assert.False(result.PlayCommandSent);
        Assert.False(coordinator.IsAnimationPlaying);
        Assert.Single(commandTransport.Commands);
        Assert.Contains("failed", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TransportDisconnect_StopsContinuousPlayback()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.FromSeconds(1));
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];

        var result = await coordinator.PlayAnimationAsync(animation);
        commandTransport.RaiseStateChanged(MaskCommandTransportState.Disconnected, "Disconnected.");
        await WaitUntilAsync(() => !coordinator.IsAnimationPlaying);
        var commandCountAfterDisconnect = commandTransport.Commands.Count;
        await Task.Delay(10);

        Assert.True(result.Succeeded);
        Assert.Equal(1, commandCountAfterDisconnect);
        Assert.Equal(commandCountAfterDisconnect, commandTransport.Commands.Count);
    }

    [Fact]
    public async Task RequestStopAnimation_DuringUpload_PreventsLoopFromStartingAfterUploadCompletes()
    {
        var uploadRelease = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport { UploadRelease = uploadRelease.Task };
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.FromMilliseconds(1));
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];

        var playTask = coordinator.PlayAnimationAsync(animation);
        await faceTransport.UploadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        coordinator.RequestStopAnimation();
        uploadRelease.SetResult(true);
        var result = await playTask;

        Assert.False(result.Succeeded);
        Assert.Contains("stopped", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(coordinator.IsAnimationPlaying);
        Assert.Empty(commandTransport.Commands);
    }

    [Fact]
    public async Task StopAnimationAsync_WithCanceledWaitToken_StillCancelsActiveLoop()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport();
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.FromMilliseconds(1));
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];
        using var cancellation = new CancellationTokenSource();

        var result = await coordinator.PlayAnimationAsync(animation);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => coordinator.StopAnimationAsync(cancellation.Token));
        await WaitUntilAsync(() => !coordinator.IsAnimationPlaying);
        var commandCountAfterStop = commandTransport.Commands.Count;
        await Task.Delay(10);

        Assert.True(result.Succeeded);
        Assert.Equal(commandCountAfterStop, commandTransport.Commands.Count);
    }

    [Fact]
    public async Task UnexpectedBackgroundCommandException_DoesNotBlockNextSend()
    {
        var store = new InMemoryFacePatternStore();
        var faceTransport = new RecordingFaceTransport();
        var commandTransport = new RecordingCommandTransport { ThrowOnCommandNumber = 2 };
        var coordinator = new DiySlotPlaybackCoordinator(
            store,
            faceTransport,
            commandTransport,
            fastAnimationFrameInterval: TimeSpan.FromMilliseconds(1));
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];
        var face = FacePatternFactory.CreateBuiltIns().Single(item => item.DisplayName == "Holy Priest · Cross");

        var animationResult = await coordinator.PlayAnimationAsync(animation);
        await WaitUntilAsync(() => commandTransport.Commands.Count >= 2);
        await WaitUntilAsync(() => !coordinator.IsAnimationPlaying);
        var faceResult = await coordinator.PlayFaceAsync(face);

        Assert.True(animationResult.Succeeded);
        Assert.True(faceResult.Succeeded);
        Assert.Equal(3, commandTransport.Commands.Count);
        Assert.Equal(MaskCommandKind.FacePlay, commandTransport.Commands.Last().Kind);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            await Task.Delay(1, timeout.Token);
        }
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

        public TaskCompletionSource<bool> UploadStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task? UploadRelease { get; init; }

        public async Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            Packages.Add(package);
            Options.Add(options);
            UploadStarted.TrySetResult(true);
            if (UploadRelease is not null)
            {
                await UploadRelease.WaitAsync(cancellationToken);
            }

            if (Packages.Count == FailOnUploadNumber)
            {
                return FaceUploadResult.Failure("Upload failed.", 0);
            }

            return FaceUploadResult.Success("Uploaded.", package.Frames.Count);
        }
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        private int commandCount;

        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        public string TransportDisplayName => "Command recorder";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; private set; } = MaskCommandTransportState.Ready;

        public string TransportStatusText { get; private set; } = "Ready.";

        public ConcurrentQueue<MaskCommand> Commands { get; } = new();

        public int? FailOnCommandNumber { get; init; }

        public int? ThrowOnCommandNumber { get; init; }

        public Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default)
        {
            Commands.Enqueue(command);
            var currentCommandNumber = Interlocked.Increment(ref commandCount);
            if (currentCommandNumber == ThrowOnCommandNumber)
            {
                throw new InvalidOperationException("Unexpected command transport failure.");
            }

            if (currentCommandNumber == FailOnCommandNumber)
            {
                return Task.FromResult(MaskCommandResult.Failure("Command failed."));
            }

            return Task.FromResult(MaskCommandResult.Success("Sent."));
        }

        public void RaiseStateChanged(MaskCommandTransportState state, string message)
        {
            TransportState = state;
            TransportStatusText = message;
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message));
        }
    }
}
