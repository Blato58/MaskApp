using MaskApp.Core.Features.WatchRemote;

namespace MaskApp.Core.Tests.Features.WatchRemote;

public sealed class WatchRemoteActionProcessorTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ValidAction_DispatchesOnceAndReturnsFreshStateAndSuccessHaptic()
    {
        var dispatcher = new RecordingDispatcher();
        var processor = CreateProcessor(dispatcher);

        var result = await processor.ProcessAsync(CreateEnvelope(1, WatchRemoteActionKind.Blackout));

        Assert.True(result.Succeeded);
        Assert.Equal(WatchRemoteProcessStatus.Accepted, result.Status);
        Assert.Equal(WatchRemoteHaptic.Success, result.Haptic);
        Assert.Equal(1, result.State.Revision);
        Assert.Single(dispatcher.Actions);
        Assert.Equal(WatchRemoteActionKind.Blackout, dispatcher.Actions[0].Kind);
    }

    [Fact]
    public async Task DuplicateIdAndOutOfOrderSequence_AreIgnoredWithoutHardwareReplay()
    {
        var dispatcher = new RecordingDispatcher();
        var processor = CreateProcessor(dispatcher);
        var first = CreateEnvelope(2, WatchRemoteActionKind.Stop);

        await processor.ProcessAsync(first);
        var duplicateId = await processor.ProcessAsync(first with { Sequence = 3 });
        var oldSequence = await processor.ProcessAsync(CreateEnvelope(1, WatchRemoteActionKind.Blackout));

        Assert.Equal(WatchRemoteProcessStatus.Duplicate, duplicateId.Status);
        Assert.Equal(WatchRemoteProcessStatus.Duplicate, oldSequence.Status);
        Assert.Equal(WatchRemoteHaptic.Warning, duplicateId.Haptic);
        Assert.Single(dispatcher.Actions);
    }

    [Theory]
    [InlineData(-11)]
    [InlineData(6)]
    public async Task StaleOrFutureDatedAction_IsRejectedWithoutDispatch(int secondsFromNow)
    {
        var dispatcher = new RecordingDispatcher();
        var processor = CreateProcessor(dispatcher);
        var envelope = CreateEnvelope(1, WatchRemoteActionKind.Stop) with
        {
            SentAt = Now.AddSeconds(secondsFromNow)
        };

        var result = await processor.ProcessAsync(envelope);

        Assert.Equal(WatchRemoteProcessStatus.Stale, result.Status);
        Assert.Equal(WatchRemoteHaptic.Warning, result.Haptic);
        Assert.Empty(dispatcher.Actions);
    }

    [Theory]
    [InlineData(WatchRemoteActionKind.Unknown, null, null)]
    [InlineData(WatchRemoteActionKind.SetBrightness, 0, null)]
    [InlineData(WatchRemoteActionKind.SetBrightness, 101, null)]
    [InlineData(WatchRemoteActionKind.TriggerFavorite, null, "")]
    [InlineData(WatchRemoteActionKind.Stop, 50, null)]
    [InlineData(WatchRemoteActionKind.Blackout, null, "not-applicable")]
    public async Task InvalidActionShape_IsRejected(
        WatchRemoteActionKind kind,
        int? brightness,
        string? favoriteId)
    {
        var dispatcher = new RecordingDispatcher();
        var processor = CreateProcessor(dispatcher);
        var envelope = CreateEnvelope(1, kind) with
        {
            Action = new WatchRemoteAction
            {
                Kind = kind,
                Brightness = brightness,
                FavoriteId = favoriteId ?? string.Empty
            }
        };

        var result = await processor.ProcessAsync(envelope);

        Assert.Equal(WatchRemoteProcessStatus.Rejected, result.Status);
        Assert.Empty(dispatcher.Actions);
    }

    [Fact]
    public async Task FailedDispatch_IsRememberedSoRetryCannotReplayPartialOutput()
    {
        var dispatcher = new RecordingDispatcher
        {
            Result = WatchRemoteDispatchResult.Failure("Transport failed after acceptance.")
        };
        var processor = CreateProcessor(dispatcher);
        var envelope = CreateEnvelope(1, WatchRemoteActionKind.TriggerCurrentCue);

        var failed = await processor.ProcessAsync(envelope);
        dispatcher.Result = WatchRemoteDispatchResult.Success("Would replay.");
        var retry = await processor.ProcessAsync(envelope);

        Assert.Equal(WatchRemoteProcessStatus.Failed, failed.Status);
        Assert.Equal(WatchRemoteProcessStatus.Duplicate, retry.Status);
        Assert.Single(dispatcher.Actions);
    }

    [Fact]
    public async Task ReplayGuard_DoesNotHoldBlackoutBehindAStartedAction()
    {
        var dispatcher = new RecordingDispatcher
        {
            BlockedKind = WatchRemoteActionKind.TriggerCurrentCue
        };
        var processor = CreateProcessor(dispatcher);

        var longRunning = processor.ProcessAsync(CreateEnvelope(1, WatchRemoteActionKind.TriggerCurrentCue));
        await dispatcher.BlockedDispatchStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var blackout = await processor.ProcessAsync(CreateEnvelope(2, WatchRemoteActionKind.Blackout))
            .WaitAsync(TimeSpan.FromSeconds(1));
        dispatcher.ReleaseBlockedDispatch.TrySetResult();
        await longRunning;

        Assert.True(blackout.Succeeded);
        Assert.Equal(2, dispatcher.MaximumConcurrentDispatches);
        Assert.Equal(2, dispatcher.Actions.Count);
    }

    [Fact]
    public async Task ExpectedOperationalFailure_ReturnsFailureResultInsteadOfFaultingReplyPath()
    {
        var dispatcher = new RecordingDispatcher
        {
            Exception = new UnauthorizedAccessException("Profile store is unavailable.")
        };
        var processor = CreateProcessor(dispatcher);

        var result = await processor.ProcessAsync(CreateEnvelope(1, WatchRemoteActionKind.Stop));

        Assert.False(result.Succeeded);
        Assert.Equal(WatchRemoteProcessStatus.Failed, result.Status);
        Assert.Equal(WatchRemoteHaptic.Failure, result.Haptic);
        Assert.Contains("unavailable", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmergencyCancellationOfOrdinaryAction_ReturnsFailureInsteadOfFaultingReplyPath()
    {
        var dispatcher = new RecordingDispatcher
        {
            Exception = new OperationCanceledException("Scene execution was stopped.")
        };
        var processor = CreateProcessor(dispatcher);

        var result = await processor.ProcessAsync(CreateEnvelope(1, WatchRemoteActionKind.TriggerCurrentCue));

        Assert.False(result.Succeeded);
        Assert.Equal(WatchRemoteProcessStatus.Failed, result.Status);
        Assert.Equal(WatchRemoteHaptic.Failure, result.Haptic);
        Assert.Contains("cancelled", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StateProviderAccessFailure_StillReturnsBoundedActionResult()
    {
        var dispatcher = new RecordingDispatcher();
        var processor = new WatchRemoteActionProcessor(
            dispatcher,
            new ThrowingStateProvider(),
            () => Now);

        var result = await processor.ProcessAsync(CreateEnvelope(1, WatchRemoteActionKind.Blackout));

        Assert.True(result.Succeeded);
        Assert.Equal(WatchRemoteProcessStatus.Accepted, result.Status);
        Assert.Contains("state unavailable", result.State.ReadinessSummary, StringComparison.OrdinalIgnoreCase);
    }

    private static WatchRemoteActionProcessor CreateProcessor(RecordingDispatcher dispatcher) =>
        new(dispatcher, new StaticStateProvider(), () => Now);

    private static WatchRemoteEnvelope CreateEnvelope(long sequence, WatchRemoteActionKind kind) => new()
    {
        MessageId = Guid.NewGuid(),
        SenderInstanceId = "watch-installation",
        Sequence = sequence,
        SentAt = Now,
        Action = new WatchRemoteAction { Kind = kind }
    };

    private sealed class RecordingDispatcher : IWatchRemoteActionDispatcher
    {
        private int activeDispatches;

        public List<WatchRemoteAction> Actions { get; } = [];

        public WatchRemoteDispatchResult Result { get; set; } =
            WatchRemoteDispatchResult.Success("Action completed.");

        public Exception? Exception { get; init; }

        public WatchRemoteActionKind? BlockedKind { get; init; }

        public TaskCompletionSource BlockedDispatchStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource ReleaseBlockedDispatch { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int MaximumConcurrentDispatches { get; private set; }

        public async Task<WatchRemoteDispatchResult> DispatchAsync(
            WatchRemoteAction action,
            CancellationToken cancellationToken = default)
        {
            var active = Interlocked.Increment(ref activeDispatches);
            MaximumConcurrentDispatches = Math.Max(MaximumConcurrentDispatches, active);
            try
            {
                Actions.Add(action);
                if (Exception is not null)
                {
                    throw Exception;
                }

                if (action.Kind == BlockedKind)
                {
                    BlockedDispatchStarted.TrySetResult();
                    await ReleaseBlockedDispatch.Task.WaitAsync(cancellationToken);
                }

                return Result;
            }
            finally
            {
                Interlocked.Decrement(ref activeDispatches);
            }
        }
    }

    private sealed class StaticStateProvider : IWatchRemoteStateProvider
    {
        private long revision;

        public Task<WatchRemoteState> GetStateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new WatchRemoteState
            {
                Revision = Interlocked.Increment(ref revision),
                GeneratedAt = Now
            });
    }

    private sealed class ThrowingStateProvider : IWatchRemoteStateProvider
    {
        public Task<WatchRemoteState> GetStateAsync(CancellationToken cancellationToken = default) =>
            throw new ObjectDisposedException("profile-store");
    }
}
