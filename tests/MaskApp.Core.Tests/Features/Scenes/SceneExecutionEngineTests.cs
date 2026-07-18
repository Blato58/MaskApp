using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.Core.Tests.Features.Scenes;

public sealed class SceneExecutionEngineTests
{
    [Fact]
    public async Task Execute_RunsTypedStepsInOrder_AndRestoresPreviousVisual()
    {
        var dispatcher = new RecordingDispatcher();
        var engine = CreateEngine(dispatcher, new ImmediateClock());
        var scene = Scene(
            Content("one", SceneStepKind.Face, "face:one"),
            Content("two", SceneStepKind.Face, "face:two"),
            Step("restore", SceneStepKind.RestorePrevious));

        var result = await engine.ExecuteAsync(scene);

        Assert.Equal(SceneExecutionState.Completed, result.State);
        Assert.True(result.Succeeded);
        Assert.Equal(
            ["trigger:face:one", "trigger:face:two", "trigger:face:one"],
            dispatcher.Actions);
        Assert.Equal(3, result.Steps.Count);
    }

    [Fact]
    public async Task Execute_ContinuePolicyReportsPartialFailureAndRunsLaterSteps()
    {
        var dispatcher = new RecordingDispatcher { FailItemId = "face:one" };
        var engine = CreateEngine(dispatcher, new ImmediateClock());
        var scene = Scene(
            Content("one", SceneStepKind.Face, "face:one"),
            Step("brightness", SceneStepKind.Brightness) with { Value = 70 }) with
        {
            FailurePolicy = SceneFailurePolicy.Continue
        };

        var result = await engine.ExecuteAsync(scene);

        Assert.Equal(SceneExecutionState.CompletedWithFailures, result.State);
        Assert.False(result.Succeeded);
        Assert.Equal(["trigger:face:one", "brightness:70"], dispatcher.Actions);
        Assert.Single(result.Steps, step => !step.Succeeded);
    }

    [Fact]
    public async Task Execute_StopPolicyPreventsLaterOutputAfterFailure()
    {
        var dispatcher = new RecordingDispatcher { FailItemId = "face:one" };
        var engine = CreateEngine(dispatcher, new ImmediateClock());
        var result = await engine.ExecuteAsync(Scene(
            Content("one", SceneStepKind.Face, "face:one"),
            Step("brightness", SceneStepKind.Brightness) with { Value = 70 }));

        Assert.Equal(SceneExecutionState.Failed, result.State);
        Assert.Equal(["trigger:face:one"], dispatcher.Actions);
    }

    [Fact]
    public async Task Blackout_CancelsWaitAndNoLaterStepRuns()
    {
        var clock = new BlockingClock();
        var dispatcher = new RecordingDispatcher();
        var engine = CreateEngine(dispatcher, clock);
        var running = engine.ExecuteAsync(Scene(
            Step("wait", SceneStepKind.Wait) with { Duration = TimeSpan.FromSeconds(10) },
            Content("later", SceneStepKind.Face, "face:one")));
        await WaitUntilAsync(() => clock.HasWaiter);

        var blackout = await engine.BlackoutAsync();
        var result = await running;

        Assert.True(blackout.Succeeded);
        Assert.Equal(SceneExecutionState.BlackedOut, result.State);
        Assert.Equal(["blackout"], dispatcher.Actions);
        Assert.DoesNotContain(dispatcher.Actions, action => action.StartsWith("trigger:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ConcurrentScene_IsRejectedUntilActiveSceneIsCancelled()
    {
        var clock = new BlockingClock();
        var dispatcher = new RecordingDispatcher();
        var engine = CreateEngine(dispatcher, clock);
        var first = engine.ExecuteAsync(Scene(
            Step("wait", SceneStepKind.Wait) with { Duration = TimeSpan.FromSeconds(10) }));
        await WaitUntilAsync(() => clock.HasWaiter);

        var second = await engine.ExecuteAsync(Scene(Step("brightness", SceneStepKind.Brightness)));
        engine.RequestCancel();
        var cancelled = await first;

        Assert.Equal(SceneExecutionState.Failed, second.State);
        Assert.Contains("already running", second.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(SceneExecutionState.Cancelled, cancelled.State);
    }

    [Fact]
    public async Task StopAndBlackoutStepsAreTerminal()
    {
        var stopDispatcher = new RecordingDispatcher();
        var stop = await CreateEngine(stopDispatcher, new ImmediateClock()).ExecuteAsync(Scene(
            Step("stop", SceneStepKind.Stop),
            Step("later", SceneStepKind.Brightness)));
        var blackoutDispatcher = new RecordingDispatcher();
        var blackout = await CreateEngine(blackoutDispatcher, new ImmediateClock()).ExecuteAsync(Scene(
            Step("blackout", SceneStepKind.Blackout),
            Step("later", SceneStepKind.Brightness)));

        Assert.Equal(SceneExecutionState.Stopped, stop.State);
        Assert.True(stop.Succeeded);
        Assert.Equal(["stop"], stopDispatcher.Actions);
        Assert.Equal(SceneExecutionState.BlackedOut, blackout.State);
        Assert.True(blackout.Succeeded);
        Assert.Equal(["blackout"], blackoutDispatcher.Actions);
    }

    private static SceneExecutionEngine CreateEngine(RecordingDispatcher dispatcher, IAnimationClock clock) =>
        new(
            new SceneValidator(),
            new StaticCatalogSource(
            [
                new GalleryItem { Id = "face:one", Type = GalleryItemType.CustomStaticFace, Title = "One" },
                new GalleryItem { Id = "face:two", Type = GalleryItemType.CustomStaticFace, Title = "Two" }
            ]),
            dispatcher,
            clock);

    private static PerformanceScene Scene(params PerformanceSceneStep[] steps) => new()
    {
        Id = $"scene-{Guid.NewGuid():N}",
        DisplayName = "Test Scene",
        Steps = steps
    };

    private static PerformanceSceneStep Step(string id, SceneStepKind kind) => new()
    {
        Id = id,
        Kind = kind,
        Value = 50
    };

    private static PerformanceSceneStep Content(string id, SceneStepKind kind, string itemId) =>
        Step(id, kind) with { GalleryItemId = itemId };

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        for (var index = 0; index < 5000; index++)
        {
            if (predicate())
            {
                return;
            }

            await Task.Yield();
        }

        throw new TimeoutException("Condition was not reached.");
    }

    private sealed class StaticCatalogSource(IReadOnlyList<GalleryItem> items) : ISceneCatalogSource
    {
        public Task<IReadOnlyList<GalleryItem>> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class RecordingDispatcher : ISceneItemDispatcher
    {
        public List<string> Actions { get; } = [];

        public string FailItemId { get; init; } = string.Empty;

        public Task<GalleryActionResult> TriggerAsync(GalleryItem item, CancellationToken cancellationToken = default)
        {
            Actions.Add($"trigger:{item.Id}");
            return Task.FromResult(item.Id == FailItemId
                ? GalleryActionResult.Failure("Injected failure.")
                : GalleryActionResult.Success("Triggered."));
        }

        public Task<MaskCommandResult> SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default)
        {
            Actions.Add($"brightness:{brightness}");
            return Task.FromResult(MaskCommandResult.Success("Brightness."));
        }

        public Task<MaskCommandResult> SetAnimationSpeedAsync(int speed, CancellationToken cancellationToken = default)
        {
            Actions.Add($"speed:{speed}");
            return Task.FromResult(MaskCommandResult.Success("Speed."));
        }

        public Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default)
        {
            Actions.Add("stop");
            return Task.FromResult(MaskCommandResult.Success("Stopped."));
        }

        public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default)
        {
            Actions.Add("blackout");
            return Task.FromResult(MaskCommandResult.Success("Blacked out."));
        }
    }

    private sealed class ImmediateClock : IAnimationClock
    {
        public long GetTimestamp() => 0;
        public long Add(long timestamp, TimeSpan duration) => timestamp + duration.Ticks;
        public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
            TimeSpan.FromTicks(endingTimestamp - startingTimestamp);
        public Task DelayUntilAsync(long deadlineTimestamp, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class BlockingClock : IAnimationClock
    {
        private volatile bool hasWaiter;

        public bool HasWaiter => hasWaiter;

        public long GetTimestamp() => 0;
        public long Add(long timestamp, TimeSpan duration) => timestamp + duration.Ticks;
        public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
            TimeSpan.FromTicks(endingTimestamp - startingTimestamp);

        public async Task DelayUntilAsync(long deadlineTimestamp, CancellationToken cancellationToken)
        {
            hasWaiter = true;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
    }
}
