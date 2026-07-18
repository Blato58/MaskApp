using System.Collections.Concurrent;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.MaskControl;

public sealed class MaskBleSchedulerTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task ConcurrentOperations_AreSerialized_AndControlPreemptsQueuedUpload()
    {
        var transport = new RecordingTransport
        {
            FaceRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var connection = new FakeConnection();
        await using var scheduler = new MaskBleScheduler(transport, transport, transport, connection);

        var faceUpload = scheduler.UploadAsync(CreateFacePackage(), CreateFaceOptions());
        await transport.FaceStarted.Task.WaitAsync(TestTimeout);

        var textUpload = scheduler.UploadAsync(CreateTextPackage(), CreateTextOptions());
        var control = scheduler.SendAsync(MaskCommandBuilder.Brightness(75));

        await Task.Delay(50);
        Assert.Equal(["face:start"], transport.Events);

        transport.FaceRelease.SetResult();
        var results = await Task.WhenAll(
            ToSucceededTask(faceUpload),
            ToSucceededTask(textUpload),
            ToSucceededTask(control));

        Assert.All(results, Assert.True);
        Assert.Equal(
            ["face:start", "face:end", "command:Brightness:start", "command:Brightness:end", "text:start", "text:end"],
            transport.Events);
        Assert.Equal(1, transport.MaxConcurrentOperations);
    }

    [Fact]
    public async Task QueuedBrightnessCommands_CoalesceToNewestValue()
    {
        var transport = new RecordingTransport
        {
            FaceRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        await using var scheduler = new MaskBleScheduler(transport, transport, transport, new FakeConnection());

        var faceUpload = scheduler.UploadAsync(CreateFacePackage(), CreateFaceOptions());
        await transport.FaceStarted.Task.WaitAsync(TestTimeout);

        var staleBrightness = scheduler.SendAsync(MaskCommandBuilder.Brightness(10));
        var currentBrightness = scheduler.SendAsync(MaskCommandBuilder.Brightness(90));

        var staleResult = await staleBrightness.WaitAsync(TestTimeout);
        Assert.False(staleResult.Succeeded);
        Assert.Contains("superseded", staleResult.Message, StringComparison.OrdinalIgnoreCase);

        transport.FaceRelease.SetResult();
        Assert.True((await currentBrightness.WaitAsync(TestTimeout)).Succeeded);
        Assert.True((await faceUpload.WaitAsync(TestTimeout)).Succeeded);

        Assert.Single(transport.Commands);
        Assert.Equal(90, transport.Commands.Single().Plaintext.Span[6]);
        Assert.Equal(1, scheduler.GetSnapshot().TotalSuperseded);
    }

    [Fact]
    public async Task ConnectionChange_InvalidatesActiveAndQueuedOperations()
    {
        var transport = new RecordingTransport
        {
            FaceRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var connection = new FakeConnection();
        await using var scheduler = new MaskBleScheduler(transport, transport, transport, connection);

        var faceUpload = scheduler.UploadAsync(CreateFacePackage(), CreateFaceOptions());
        await transport.FaceStarted.Task.WaitAsync(TestTimeout);
        var queuedControl = scheduler.SendAsync(MaskCommandBuilder.Brightness(50));

        connection.ChangeState(BleConnectionState.Disconnected, "Radio link lost.");

        var faceResult = await faceUpload.WaitAsync(TestTimeout);
        var commandResult = await queuedControl.WaitAsync(TestTimeout);
        Assert.False(faceResult.Succeeded);
        Assert.False(commandResult.Succeeded);
        Assert.Contains("connection changed", faceResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("connection changed", commandResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(transport.Commands);
        Assert.Equal(1, scheduler.GetSnapshot().ConnectionGeneration);
    }

    [Fact]
    public async Task QueuedCallerCancellation_NeverReachesTransport()
    {
        var transport = new RecordingTransport
        {
            FaceRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        await using var scheduler = new MaskBleScheduler(transport, transport, transport, new FakeConnection());
        using var cancellation = new CancellationTokenSource();

        var faceUpload = scheduler.UploadAsync(CreateFacePackage(), CreateFaceOptions());
        await transport.FaceStarted.Task.WaitAsync(TestTimeout);
        var textUpload = scheduler.UploadAsync(CreateTextPackage(), CreateTextOptions(), cancellation.Token);

        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => textUpload);

        transport.FaceRelease.SetResult();
        Assert.True((await faceUpload.WaitAsync(TestTimeout)).Succeeded);
        await WaitUntilAsync(() => scheduler.GetSnapshot().PendingOperationCount == 0);

        Assert.DoesNotContain("text:start", transport.Events);
    }

    [Fact]
    public async Task TimedOutOperation_ReleasesSchedulerForNextCommand()
    {
        var transport = new RecordingTransport
        {
            CommandRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var schedulerOptions = new MaskBleSchedulerOptions
        {
            CommandTimeout = TimeSpan.FromMilliseconds(50)
        };
        await using var scheduler = new MaskBleScheduler(
            transport,
            transport,
            transport,
            new FakeConnection(),
            schedulerOptions);

        var timedOut = await scheduler
            .SendAsync(MaskCommandBuilder.Animation(1))
            .WaitAsync(TestTimeout);

        Assert.False(timedOut.Succeeded);
        Assert.Contains("timed out", timedOut.Message, StringComparison.OrdinalIgnoreCase);

        transport.CommandRelease.SetResult();
        var next = await scheduler
            .SendAsync(MaskCommandBuilder.Image(2))
            .WaitAsync(TestTimeout);

        Assert.True(next.Succeeded);
        Assert.Equal(2, transport.Commands.Count);
        Assert.Equal(1, transport.MaxConcurrentOperations);
    }

    [Fact]
    public async Task Blackout_CancelsActiveAndQueuedVisualWork_ThenWritesAtEmergencyPriority()
    {
        var transport = new RecordingTransport
        {
            FaceRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        await using var scheduler = new MaskBleScheduler(transport, transport, transport, new FakeConnection());

        var faceUpload = scheduler.UploadAsync(CreateFacePackage(), CreateFaceOptions());
        await transport.FaceStarted.Task.WaitAsync(TestTimeout);
        var queuedText = scheduler.UploadAsync(CreateTextPackage(), CreateTextOptions());

        var blackout = scheduler.BlackoutAsync();

        var faceResult = await faceUpload.WaitAsync(TestTimeout);
        var textResult = await queuedText.WaitAsync(TestTimeout);
        var blackoutResult = await blackout.WaitAsync(TestTimeout);
        Assert.False(faceResult.Succeeded);
        Assert.False(textResult.Succeeded);
        Assert.True(blackoutResult.Succeeded);
        Assert.Contains("blackout", faceResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("blackout", textResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(transport.Commands);
        Assert.Equal(1, transport.Commands.Single().Plaintext.Span[6]);
        Assert.DoesNotContain("text:start", transport.Events);
        Assert.Equal(2, scheduler.GetSnapshot().TotalEmergencyCancellations);
        Assert.Equal(1, transport.MaxConcurrentOperations);
    }

    [Fact]
    public async Task Stop_CancelsVisualWork_WithoutInventingFallbackOutput()
    {
        var transport = new RecordingTransport
        {
            FaceRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        await using var scheduler = new MaskBleScheduler(transport, transport, transport, new FakeConnection());

        var faceUpload = scheduler.UploadAsync(CreateFacePackage(), CreateFaceOptions());
        await transport.FaceStarted.Task.WaitAsync(TestTimeout);
        var queuedText = scheduler.UploadAsync(CreateTextPackage(), CreateTextOptions());

        var stopResult = await scheduler.StopAsync();
        var faceResult = await faceUpload.WaitAsync(TestTimeout);
        var textResult = await queuedText.WaitAsync(TestTimeout);

        Assert.True(stopResult.Succeeded);
        Assert.False(faceResult.Succeeded);
        Assert.False(textResult.Succeeded);
        Assert.Contains("Stop", faceResult.Message, StringComparison.Ordinal);
        Assert.Contains("Stop", textResult.Message, StringComparison.Ordinal);
        Assert.Empty(transport.Commands);
    }

    [Fact]
    public async Task FullQueue_RejectsOrdinaryWork_ButNeverRejectsBlackout()
    {
        var transport = new RecordingTransport
        {
            FaceRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var schedulerOptions = new MaskBleSchedulerOptions { MaxPendingOperations = 1 };
        await using var scheduler = new MaskBleScheduler(
            transport,
            transport,
            transport,
            new FakeConnection(),
            schedulerOptions);

        var faceUpload = scheduler.UploadAsync(CreateFacePackage(), CreateFaceOptions());
        await transport.FaceStarted.Task.WaitAsync(TestTimeout);
        var acceptedText = scheduler.UploadAsync(CreateTextPackage(), CreateTextOptions());
        var rejectedText = scheduler.UploadAsync(CreateTextPackage(), CreateTextOptions());

        var rejection = await rejectedText.WaitAsync(TestTimeout);
        Assert.False(rejection.Succeeded);
        Assert.Contains("queue is full", rejection.Message, StringComparison.OrdinalIgnoreCase);

        var blackout = await scheduler.BlackoutAsync().WaitAsync(TestTimeout);
        Assert.True(blackout.Succeeded);
        Assert.False((await faceUpload.WaitAsync(TestTimeout)).Succeeded);
        Assert.False((await acceptedText.WaitAsync(TestTimeout)).Succeeded);
        Assert.Equal(1, scheduler.GetSnapshot().TotalRejected);
        Assert.Single(transport.Commands);
        Assert.Equal(1, transport.Commands.Single().Plaintext.Span[6]);
    }

    private static TextUploadPackage CreateTextPackage() =>
        TextUploadProtocol.CreatePackage("TEST", new TextLedColor(1, 2, 3), mode: 2, speed: 80);

    private static FaceUploadPackage CreateFacePackage() =>
        FaceUploadProtocol.CreatePackage(
            FacePatternFactory.CreateBlank(),
            FacePattern.MinSlot,
            finishTimestamp: 0);

    private static TextUploadOptions CreateTextOptions() =>
        new()
        {
            ResetDisplayBeforeUpload = false,
            PostUploadQuietPeriod = TimeSpan.Zero
        };

    private static FaceUploadOptions CreateFaceOptions() =>
        new()
        {
            DeleteSlotBeforeUpload = false,
            PlayAfterUpload = false,
            PostUploadQuietPeriod = TimeSpan.Zero
        };

    private static async Task<bool> ToSucceededTask(Task<FaceUploadResult> task) =>
        (await task.WaitAsync(TestTimeout)).Succeeded;

    private static async Task<bool> ToSucceededTask(Task<TextUploadResult> task) =>
        (await task.WaitAsync(TestTimeout)).Succeeded;

    private static async Task<bool> ToSucceededTask(Task<MaskCommandResult> task) =>
        (await task.WaitAsync(TestTimeout)).Succeeded;

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TestTimeout);
        while (!condition())
        {
            await Task.Delay(1, timeout.Token);
        }
    }

    private sealed class FakeConnection : IBleDeviceConnection
    {
        public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;

        public BleConnectionState State { get; private set; } = BleConnectionState.Connected;

        public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void ChangeState(BleConnectionState state, string message)
        {
            State = state;
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(state, message));
        }
    }

    private sealed class RecordingTransport :
        IMaskCommandTransport,
        ITextUploadTransport,
        IFaceUploadTransport
    {
        private readonly object sync = new();
        private readonly ConcurrentQueue<string> events = new();
        private readonly ConcurrentQueue<MaskCommand> commands = new();
        private int activeOperations;
        private int maxConcurrentOperations;

        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        event EventHandler<TextUploadTransportStateChangedEventArgs>? ITextUploadTransport.StateChanged
        {
            add { }
            remove { }
        }

        event EventHandler<FaceUploadTransportStateChangedEventArgs>? IFaceUploadTransport.StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Recording transport";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

        public string TransportStatusText => "Ready.";

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        TextUploadTransportState ITextUploadTransport.State => TextUploadTransportState.Simulated;

        FaceUploadTransportState IFaceUploadTransport.State => FaceUploadTransportState.Simulated;

        public string StatusText => "Ready.";

        public TaskCompletionSource FaceStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource? FaceRelease { get; init; }

        public TaskCompletionSource? CommandRelease { get; init; }

        public IReadOnlyList<string> Events => events.ToArray();

        public IReadOnlyList<MaskCommand> Commands => commands.ToArray();

        public int MaxConcurrentOperations
        {
            get
            {
                lock (sync)
                {
                    return maxConcurrentOperations;
                }
            }
        }

        public async Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default)
        {
            Enter($"command:{command.Kind}:start");
            commands.Enqueue(command);
            try
            {
                if (CommandRelease is not null)
                {
                    await CommandRelease.Task.WaitAsync(cancellationToken);
                }

                events.Enqueue($"command:{command.Kind}:end");
                return MaskCommandResult.Success("Sent.");
            }
            finally
            {
                Exit();
            }
        }

        public async Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            Enter("text:start");
            try
            {
                events.Enqueue("text:end");
                await Task.Yield();
                return TextUploadResult.Success("Uploaded.", package.Frames.Count);
            }
            finally
            {
                Exit();
            }
        }

        public async Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            Enter("face:start");
            FaceStarted.TrySetResult();
            try
            {
                if (FaceRelease is not null)
                {
                    await FaceRelease.Task.WaitAsync(cancellationToken);
                }

                events.Enqueue("face:end");
                return FaceUploadResult.Success("Uploaded.", package.Frames.Count);
            }
            finally
            {
                Exit();
            }
        }

        private void Enter(string eventName)
        {
            events.Enqueue(eventName);
            lock (sync)
            {
                activeOperations++;
                maxConcurrentOperations = Math.Max(maxConcurrentOperations, activeOperations);
            }
        }

        private void Exit()
        {
            lock (sync)
            {
                activeOperations--;
            }
        }
    }
}
