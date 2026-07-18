using System.Collections.Concurrent;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class PerformanceAnimationEngineTests
{
    [Fact]
    public async Task FinitePlayback_UsesPerFrameDeadlines_AndRestoresOnce()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var animation = CreateSafeAnimation(AnimationLoopMode.Finite, loops: 2);
        var restoreCount = 0;

        var started = await engine.StartAsync(animation, new AnimationPlaybackRequest
        {
            RestorePreviousLookAsync = _ =>
            {
                Interlocked.Increment(ref restoreCount);
                return Task.FromResult(MaskCommandResult.Success("Restored."));
            }
        });
        Assert.True(started.Succeeded);
        Assert.Single(transport.Commands);

        clock.AdvanceBy(TimeSpan.FromMilliseconds(100));
        await WaitUntilAsync(() => transport.Commands.Count == 2);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(200));
        await WaitUntilAsync(() => transport.Commands.Count == 3);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(100));
        await WaitUntilAsync(() => transport.Commands.Count == 4);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(200));
        await WaitUntilAsync(() => engine.GetSnapshot().State == AnimationPlaybackState.Completed);

        Assert.Equal([1, 2, 1, 2], transport.Slots);
        Assert.Equal(1, restoreCount);
        Assert.Equal(4, engine.GetSnapshot().FramesSent);
    }

    [Fact]
    public async Task Pause_ShiftsDeadline_WithoutBurstingFrames()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));

        Assert.True(started.Handle!.Pause());
        clock.AdvanceBy(TimeSpan.FromSeconds(1));
        await Task.Yield();
        Assert.Single(transport.Commands);

        Assert.True(started.Handle.Resume());
        await Task.Yield();
        clock.AdvanceBy(TimeSpan.FromMilliseconds(99));
        await Task.Yield();
        Assert.Single(transport.Commands);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(1));
        await WaitUntilAsync(() => transport.Commands.Count == 2);
        await started.Handle.StopAsync(restorePreviousLook: false);
    }

    [Fact]
    public async Task BackgroundHandoff_TransfersContinuousLoopToMask_ThenResumesExactTiming()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));

        var handedOff = await engine.HandOffToMaskForBackgroundAsync();

        Assert.True(handedOff.Succeeded);
        Assert.Contains("mask-owned", handedOff.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(AnimationPlaybackState.Backgrounded, engine.GetSnapshot().State);
        Assert.Equal(2, transport.Commands.Count);
        var firmwareCommand = transport.Commands.Last().Plaintext.ToArray();
        Assert.Equal(2, firmwareCommand[5]);
        Assert.Equal(new byte[] { 1, 2 }, firmwareCommand[6..8]);

        clock.AdvanceBy(TimeSpan.FromSeconds(1));
        await Task.Yield();
        Assert.Equal(2, transport.Commands.Count);

        var resumed = await engine.ResumeFromBackgroundAsync();
        Assert.True(resumed.Succeeded);
        Assert.Contains("Reclaimed", resumed.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(AnimationPlaybackState.Playing, engine.GetSnapshot().State);
        Assert.Equal(3, transport.Commands.Count);
        Assert.Equal(1, transport.Slots[^1]);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(99));
        await Task.Yield();
        Assert.Equal(3, transport.Commands.Count);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(1));
        await WaitUntilAsync(() => transport.Commands.Count == 4);
        Assert.Equal(2, transport.Slots[^1]);

        await started.Handle!.StopAsync(restorePreviousLook: false);
    }

    [Fact]
    public async Task BackgroundHandoff_SamplesLongLoopToPlayCommandLimit()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var source = CreateSafeAnimation(AnimationLoopMode.Continuous);
        var animation = new PerformanceAnimationBuilder().WithRevision(source with
        {
            Frames = Enumerable.Range(0, 20)
                .Select(index => new PerformanceAnimationFrame
                {
                    Slot = index % 2 == 0 ? 1 : 2,
                    Duration = TimeSpan.FromMilliseconds(100)
                })
                .ToArray()
        });
        var started = await engine.StartAsync(animation);

        var handedOff = await engine.HandOffToMaskForBackgroundAsync();

        Assert.True(handedOff.Succeeded);
        Assert.Contains("first 10", handedOff.Message, StringComparison.OrdinalIgnoreCase);
        var firmwareCommand = transport.Commands.Last().Plaintext.ToArray();
        Assert.Equal(10, firmwareCommand[5]);
        Assert.Equal(16, firmwareCommand.Length);
        Assert.Equal(
            new byte[] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 },
            firmwareCommand[6..16]);
        await started.Handle!.StopAsync(restorePreviousLook: false);
    }

    [Fact]
    public async Task StaleBackgroundHandoff_DoesNotOverrideANewerForegroundResume()
    {
        var clock = new ManualAnimationClock();
        var firstSendStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstSend = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new RecordingCommandTransport
        {
            BeforeSendCompletesAsync = async (count, cancellationToken) =>
            {
                if (count != 1)
                {
                    return;
                }

                firstSendStarted.TrySetResult(true);
                await releaseFirstSend.Task.WaitAsync(cancellationToken);
            }
        };
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var startTask = engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));
        await firstSendStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var handoffTask = engine.HandOffToMaskForBackgroundAsync();
        var resumeTask = engine.ResumeFromBackgroundAsync();
        releaseFirstSend.TrySetResult(true);

        var started = await startTask;
        var handoff = await handoffTask;
        var resumed = await resumeTask;

        Assert.True(started.Succeeded);
        Assert.True(handoff.Succeeded);
        Assert.True(resumed.Succeeded);
        Assert.Contains("stale", handoff.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(transport.Commands);
        Assert.Equal(AnimationPlaybackState.Playing, engine.GetSnapshot().State);
        await started.Handle!.StopAsync(restorePreviousLook: false);
    }

    [Fact]
    public async Task BackgroundHandoffWaitingForFrameSend_SkipsWhenAppResumes()
    {
        var clock = new ManualAnimationClock();
        var secondSendStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecondSend = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new RecordingCommandTransport
        {
            BeforeSendCompletesAsync = async (count, cancellationToken) =>
            {
                if (count != 2)
                {
                    return;
                }

                secondSendStarted.TrySetResult(true);
                await releaseSecondSend.Task.WaitAsync(cancellationToken);
            }
        };
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));
        clock.AdvanceBy(TimeSpan.FromMilliseconds(100));
        await secondSendStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var handoffTask = engine.HandOffToMaskForBackgroundAsync();
        var resumeTask = engine.ResumeFromBackgroundAsync();
        releaseSecondSend.TrySetResult(true);

        var handoff = await handoffTask;
        var resumed = await resumeTask;

        Assert.True(handoff.Succeeded);
        Assert.True(resumed.Succeeded);
        Assert.Contains("stale", handoff.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, transport.Commands.Count);
        Assert.All(transport.Commands, command => Assert.Equal(1, command.Plaintext.Span[5]));
        Assert.Equal(AnimationPlaybackState.Playing, engine.GetSnapshot().State);
        await started.Handle!.StopAsync(restorePreviousLook: false);
    }

    [Fact]
    public async Task AnimationStartedAfterWindowStops_IsImmediatelyHandedToMask()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        await engine.HandOffToMaskForBackgroundAsync();

        var started = await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));

        Assert.True(started.Succeeded);
        Assert.Equal(AnimationPlaybackState.Backgrounded, engine.GetSnapshot().State);
        Assert.Equal(2, transport.Commands.Count);
        Assert.Equal(2, transport.Commands.Last().Plaintext.Span[5]);
        await engine.ResumeFromBackgroundAsync();
        await started.Handle!.StopAsync(restorePreviousLook: false);
    }

    [Fact]
    public async Task BackgroundingFinitePlayback_PausesWithoutStartingAnEndlessFirmwareLoop()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Finite));

        var handedOff = await engine.HandOffToMaskForBackgroundAsync();

        Assert.True(handedOff.Succeeded);
        Assert.Contains("paused", handedOff.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(AnimationPlaybackState.Backgrounded, engine.GetSnapshot().State);
        Assert.Single(transport.Commands);

        await engine.ResumeFromBackgroundAsync();
        await started.Handle!.StopAsync(restorePreviousLook: false);
    }

    [Fact]
    public async Task StoppingBackgroundPlayback_ReclaimsTheFirmwareLoopWithOneSlot()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));
        await engine.HandOffToMaskForBackgroundAsync();

        var stopped = await started.Handle!.StopAsync(restorePreviousLook: false);

        Assert.True(stopped.Succeeded);
        Assert.Equal(3, transport.Commands.Count);
        var reclaimCommand = transport.Commands.Last().Plaintext.ToArray();
        Assert.Equal(1, reclaimCommand[5]);
        Assert.Equal(1, reclaimCommand[6]);
        Assert.Equal(AnimationPlaybackState.Stopped, engine.GetSnapshot().State);
    }

    [Fact]
    public async Task LateTransport_DropsExpiredFrames_InsteadOfCatchUpBurst()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport
        {
            AfterSend = count =>
            {
                if (count == 2)
                {
                    clock.AdvanceBy(TimeSpan.FromMilliseconds(350));
                }
            }
        };
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(
            AnimationLoopMode.Continuous,
            firstDuration: TimeSpan.FromMilliseconds(100),
            secondDuration: TimeSpan.FromMilliseconds(100)));

        clock.AdvanceBy(TimeSpan.FromMilliseconds(100));
        await WaitUntilAsync(() => engine.GetSnapshot().FramesDropped >= 2);
        Assert.Equal(3, transport.Commands.Count);
        Assert.True(engine.GetSnapshot().LateFrames >= 1);
        await started.Handle!.StopAsync(restorePreviousLook: false);
        Assert.True(engine.GetSnapshot().FramesDropped >= 2);
    }

    [Fact]
    public async Task TransportBackpressure_DoesNotOverlapFrameWrites_AndDropsExpiredFrames()
    {
        var clock = new ManualAnimationClock();
        var secondSendStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecondSend = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new RecordingCommandTransport
        {
            BeforeSendCompletesAsync = async (count, cancellationToken) =>
            {
                if (count != 2)
                {
                    return;
                }

                secondSendStarted.TrySetResult(true);
                await releaseSecondSend.Task.WaitAsync(cancellationToken);
            }
        };
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(
            AnimationLoopMode.Continuous,
            firstDuration: TimeSpan.FromMilliseconds(100),
            secondDuration: TimeSpan.FromMilliseconds(100)));

        clock.AdvanceBy(TimeSpan.FromMilliseconds(100));
        await secondSendStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        clock.AdvanceBy(TimeSpan.FromSeconds(1));
        await Task.Yield();

        Assert.Equal(2, transport.Commands.Count);

        releaseSecondSend.TrySetResult(true);
        await WaitUntilAsync(() => engine.GetSnapshot().FramesDropped > 0);
        await started.Handle!.StopAsync(restorePreviousLook: false);

        Assert.True(engine.GetSnapshot().FramesDropped > 0);
    }

    [Fact]
    public async Task Disconnect_EndsSession_AndReconnectDoesNotReplay()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var started = await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));

        transport.SetState(MaskCommandTransportState.Disconnected, "Radio lost.");
        await WaitUntilAsync(() => engine.GetSnapshot().State == AnimationPlaybackState.Disconnected);
        transport.SetState(MaskCommandTransportState.Ready, "Ready.");
        clock.AdvanceBy(TimeSpan.FromSeconds(5));
        await Task.Yield();

        Assert.True(started.Succeeded);
        Assert.Single(transport.Commands);
        Assert.Contains("Radio lost", engine.GetSnapshot().LastError, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HoldRelease_RestoresExactlyOnce()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var engine = new PerformanceAnimationEngine(transport, clock: clock);
        var restores = 0;
        var started = await engine.StartAsync(
            CreateSafeAnimation(AnimationLoopMode.Continuous),
            new AnimationPlaybackRequest
            {
                IsHoldToPlay = true,
                RestoreWhenReleased = true,
                RestorePreviousLookAsync = _ =>
                {
                    Interlocked.Increment(ref restores);
                    return Task.FromResult(MaskCommandResult.Success("Restored."));
                }
            });

        await started.Handle!.ReleaseAsync();
        await started.Handle.ReleaseAsync();

        Assert.Equal(1, restores);
        Assert.Equal(AnimationPlaybackState.Stopped, engine.GetSnapshot().State);
    }

    [Fact]
    public async Task Blackout_CancelsPlaybackBeforeEmergencyCommand_AndNeverRestores()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var emergency = new RecordingEmergencyControl();
        var engine = new PerformanceAnimationEngine(transport, emergency, clock);
        var restores = 0;
        await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous), new AnimationPlaybackRequest
        {
            RestorePreviousLookAsync = _ =>
            {
                restores++;
                return Task.FromResult(MaskCommandResult.Success("Restored."));
            }
        });

        var result = await engine.BlackoutAsync();
        clock.AdvanceBy(TimeSpan.FromSeconds(1));

        Assert.True(result.Succeeded);
        Assert.Equal(1, emergency.BlackoutCount);
        Assert.Equal(0, restores);
        Assert.Single(transport.Commands);
        Assert.Equal(AnimationPlaybackState.BlackedOut, engine.GetSnapshot().State);
    }

    [Fact]
    public async Task Blackout_DoesNotWaitForANormalReclaimAfterBackgroundHandoff()
    {
        var clock = new ManualAnimationClock();
        var transport = new RecordingCommandTransport();
        var emergency = new RecordingEmergencyControl();
        var engine = new PerformanceAnimationEngine(transport, emergency, clock);
        await engine.StartAsync(CreateSafeAnimation(AnimationLoopMode.Continuous));
        await engine.HandOffToMaskForBackgroundAsync();

        var result = await engine.BlackoutAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(1, emergency.BlackoutCount);
        Assert.Equal(2, transport.Commands.Count);
        Assert.Equal(AnimationPlaybackState.BlackedOut, engine.GetSnapshot().State);
    }

    [Fact]
    public async Task UnsafeRevision_IsBlockedUntilExactAcknowledgementExists()
    {
        var transport = new RecordingCommandTransport();
        var store = new InMemoryFlashSafetyAcknowledgementStore();
        var analyzer = new FlashSafetyAnalyzer();
        var engine = new PerformanceAnimationEngine(
            transport,
            clock: new ManualAnimationClock(),
            flashSafetyAnalyzer: analyzer,
            flashSafetyAcknowledgementStore: store);
        var animation = FlashSafetyAnalyzerTests.CreateBlackWhiteAnimation(
            TimeSpan.FromMilliseconds(100),
            AnimationLoopMode.Continuous,
            1,
            [1, 2]);

        var blocked = await engine.StartAsync(animation);
        var assessment = analyzer.Analyze(animation);
        await new FlashSafetyAcknowledgementService(store).AcknowledgeAsync(assessment);
        var allowed = await engine.StartAsync(animation);

        Assert.False(blocked.Succeeded);
        Assert.Contains("blocked", blocked.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(allowed.Succeeded);
        Assert.Single(transport.Commands);
        await allowed.Handle!.StopAsync(restorePreviousLook: false);
    }

    private static PerformanceAnimation CreateSafeAnimation(
        AnimationLoopMode loopMode,
        int loops = 1,
        TimeSpan? firstDuration = null,
        TimeSpan? secondDuration = null)
    {
        var black = FlashSafetyAnalyzerTests.CreateSolidPattern("black-safe", FaceColor.Black, 1, lit: false);
        var onePixel = black.WithPixel(0, 0, new FacePixel(true, new FaceColor(255, 255, 255))) with
        {
            Id = "one-pixel",
            DisplayName = "One Pixel",
            PreferredSlot = 2
        };
        var animation = new PerformanceAnimation
        {
            Id = "safe-animation",
            DisplayName = "Safe Animation",
            StoredFrames =
            [
                new PerformanceAnimationStoredFrame { Slot = 1, Pattern = black },
                new PerformanceAnimationStoredFrame { Slot = 2, Pattern = onePixel }
            ],
            Frames =
            [
                new PerformanceAnimationFrame
                {
                    Slot = 1,
                    Duration = firstDuration ?? TimeSpan.FromMilliseconds(100)
                },
                new PerformanceAnimationFrame
                {
                    Slot = 2,
                    Duration = secondDuration ?? TimeSpan.FromMilliseconds(200)
                }
            ],
            LoopMode = loopMode,
            FiniteLoopCount = loops,
            Bpm = 120
        };
        return new PerformanceAnimationBuilder().WithRevision(animation);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition())
        {
            if (DateTime.UtcNow >= timeout)
            {
                throw new TimeoutException("Condition was not reached.");
            }

            await Task.Yield();
        }
    }

    private sealed class ManualAnimationClock : IAnimationClock
    {
        private readonly object sync = new();
        private readonly List<Waiter> waiters = [];
        private long timestamp;

        public long GetTimestamp()
        {
            lock (sync)
            {
                return timestamp;
            }
        }

        public long Add(long timestamp, TimeSpan duration) => checked(timestamp + duration.Ticks);

        public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
            TimeSpan.FromTicks(endingTimestamp - startingTimestamp);

        public Task DelayUntilAsync(long deadlineTimestamp, CancellationToken cancellationToken)
        {
            lock (sync)
            {
                if (deadlineTimestamp <= timestamp)
                {
                    return Task.CompletedTask;
                }

                var waiter = new Waiter(deadlineTimestamp);
                waiter.CancellationRegistration = cancellationToken.Register(() =>
                    waiter.Completion.TrySetCanceled(cancellationToken));
                waiters.Add(waiter);
                return waiter.Completion.Task;
            }
        }

        public void AdvanceBy(TimeSpan duration)
        {
            Waiter[] completed;
            lock (sync)
            {
                timestamp = checked(timestamp + duration.Ticks);
                completed = waiters.Where(waiter => waiter.Deadline <= timestamp).ToArray();
                foreach (var waiter in completed)
                {
                    waiters.Remove(waiter);
                }
            }

            foreach (var waiter in completed)
            {
                waiter.CancellationRegistration.Dispose();
                waiter.Completion.TrySetResult(true);
            }
        }

        private sealed class Waiter(long deadline)
        {
            public long Deadline { get; } = deadline;

            public TaskCompletionSource<bool> Completion { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            public CancellationTokenRegistration CancellationRegistration { get; set; }
        }
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        private int sendCount;

        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; private set; } = MaskCommandTransportState.Ready;

        public string TransportStatusText { get; private set; } = "Ready.";

        public ConcurrentQueue<MaskCommand> Commands { get; } = new();

        public IReadOnlyList<int> Slots => Commands.Select(command => (int)command.Plaintext.Span[6]).ToArray();

        public Action<int>? AfterSend { get; init; }

        public Func<int, CancellationToken, Task>? BeforeSendCompletesAsync { get; init; }

        public async Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Commands.Enqueue(command);
            var count = Interlocked.Increment(ref sendCount);
            AfterSend?.Invoke(count);
            if (BeforeSendCompletesAsync is not null)
            {
                await BeforeSendCompletesAsync(count, cancellationToken);
            }

            return MaskCommandResult.Success("Sent.");
        }

        public void SetState(MaskCommandTransportState state, string message)
        {
            TransportState = state;
            TransportStatusText = message;
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message));
        }
    }

    private sealed class RecordingEmergencyControl : IMaskEmergencyControl
    {
        public int BlackoutCount { get; private set; }

        public Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Stopped."));

        public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default)
        {
            BlackoutCount++;
            return Task.FromResult(MaskCommandResult.Success("Blackout."));
        }
    }
}
