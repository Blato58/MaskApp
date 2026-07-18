using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Scenes;

public interface ISceneCatalogSource
{
    Task<IReadOnlyList<GalleryItem>> LoadAsync(CancellationToken cancellationToken = default);
}

public interface ISceneItemDispatcher
{
    Task<GalleryActionResult> TriggerAsync(GalleryItem item, CancellationToken cancellationToken = default);

    Task<MaskCommandResult> SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default);

    Task<MaskCommandResult> SetAnimationSpeedAsync(int speed, CancellationToken cancellationToken = default);

    Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default);

    Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default);
}

public sealed class GallerySceneItemDispatcher : ISceneItemDispatcher
{
    private readonly ITextPresetDispatcher textPresetDispatcher;
    private readonly IMaskCommandTransport commandTransport;
    private readonly IQuickActionDispatcher quickActionDispatcher;
    private readonly DiySlotPlaybackCoordinator playbackCoordinator;
    private readonly IMaskEmergencyControl emergencyControl;

    public GallerySceneItemDispatcher(
        ITextPresetDispatcher textPresetDispatcher,
        IMaskCommandTransport commandTransport,
        IQuickActionDispatcher quickActionDispatcher,
        DiySlotPlaybackCoordinator playbackCoordinator,
        IMaskEmergencyControl emergencyControl)
    {
        this.textPresetDispatcher = textPresetDispatcher;
        this.commandTransport = commandTransport;
        this.quickActionDispatcher = quickActionDispatcher;
        this.playbackCoordinator = playbackCoordinator;
        this.emergencyControl = emergencyControl;
    }

    public async Task<GalleryActionResult> TriggerAsync(
        GalleryItem item,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        await playbackCoordinator.StopAnimationAsync(cancellationToken).ConfigureAwait(false);
        return item.Type switch
        {
            GalleryItemType.TextPreset when item.TextPreset is not null =>
                ToResult(await textPresetDispatcher.SendAsync(item.TextPreset, cancellationToken).ConfigureAwait(false)),
            GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation when item.BuiltInAssetRecord is not null =>
                ToResult(await commandTransport.SendAsync(
                    BuiltInAssetCommandFactory.CreateCommand(item.BuiltInAssetRecord),
                    cancellationToken).ConfigureAwait(false)),
            GalleryItemType.CustomStaticFace when item.FacePattern is not null =>
                ToResult(await playbackCoordinator.PlayFaceAsync(item.FacePattern, cancellationToken).ConfigureAwait(false)),
            GalleryItemType.AppBuiltInAnimation when item.AppAnimation is not null =>
                ToResult(await playbackCoordinator.PlayAnimationAsync(item.AppAnimation, cancellationToken).ConfigureAwait(false)),
            GalleryItemType.CustomAnimation when item.PerformanceAnimation is not null =>
                ToResult(await playbackCoordinator.PlayAnimationAsync(item.PerformanceAnimation, cancellationToken).ConfigureAwait(false)),
            GalleryItemType.QuickAction when item.QuickActionId is QuickActionId actionId =>
                ToResult(await quickActionDispatcher.TriggerAsync(actionId, cancellationToken: cancellationToken).ConfigureAwait(false)),
            _ => GalleryActionResult.Failure($"{item.Title} is not a supported Scene content step.")
        };
    }

    public Task<MaskCommandResult> SetBrightnessAsync(
        int brightness,
        CancellationToken cancellationToken = default) =>
        commandTransport.SendAsync(MaskCommandBuilder.Brightness(brightness), cancellationToken);

    public Task<MaskCommandResult> SetAnimationSpeedAsync(
        int speed,
        CancellationToken cancellationToken = default) =>
        commandTransport.SendAsync(MaskCommandBuilder.AnimationSpeed(speed), cancellationToken);

    public async Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default)
    {
        await playbackCoordinator.StopAnimationAsync(cancellationToken).ConfigureAwait(false);
        return await emergencyControl.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default) =>
        emergencyControl.BlackoutAsync(cancellationToken);

    private static GalleryActionResult ToResult(TextPresetDispatchResult result) =>
        new(result.Succeeded, result.Message);

    private static GalleryActionResult ToResult(MaskCommandResult result) =>
        new(result.Succeeded, result.Message);

    private static GalleryActionResult ToResult(DiySlotPlaybackResult result) =>
        new(result.Succeeded, result.Message);

    private static GalleryActionResult ToResult(QuickActionResult result) =>
        new(result.Succeeded, result.Message);
}

public enum SceneExecutionState
{
    Idle,
    Running,
    Completed,
    CompletedWithFailures,
    Failed,
    Cancelled,
    Stopped,
    BlackedOut
}

public sealed record SceneStepExecutionResult(
    string StepId,
    SceneStepKind Kind,
    bool Succeeded,
    string Message);

public sealed record SceneExecutionResult(
    SceneExecutionState State,
    string Message,
    IReadOnlyList<SceneStepExecutionResult> Steps,
    bool CompletedByTerminalStep = false)
{
    public bool Succeeded => State == SceneExecutionState.Completed || CompletedByTerminalStep;
}

public sealed record SceneExecutionSnapshot
{
    public SceneExecutionState State { get; init; }

    public string SceneId { get; init; } = string.Empty;

    public string SceneName { get; init; } = string.Empty;

    public int CurrentStepIndex { get; init; } = -1;

    public int ExpandedStepCount { get; init; }

    public string CurrentStepId { get; init; } = string.Empty;

    public string LastMessage { get; init; } = string.Empty;

    public bool IsActive => State == SceneExecutionState.Running;
}

public interface ISceneExecutionControl
{
    void RequestCancel();
}

public sealed class SceneExecutionEngine : ISceneExecutionControl
{
    private readonly SceneValidator validator;
    private readonly ISceneCatalogSource catalogSource;
    private readonly ISceneItemDispatcher dispatcher;
    private readonly IAnimationClock clock;
    private readonly SemaphoreSlim executionGate = new(1, 1);
    private readonly object sessionSync = new();
    private CancellationTokenSource? activeSession;
    private SceneExecutionState requestedTerminalState = SceneExecutionState.Cancelled;
    private SceneExecutionSnapshot snapshot = new();

    public SceneExecutionEngine(
        SceneValidator validator,
        ISceneCatalogSource catalogSource,
        ISceneItemDispatcher dispatcher,
        IAnimationClock clock)
    {
        this.validator = validator;
        this.catalogSource = catalogSource;
        this.dispatcher = dispatcher;
        this.clock = clock;
    }

    public event EventHandler<SceneExecutionSnapshot>? SnapshotChanged;

    public SceneExecutionSnapshot GetSnapshot()
    {
        lock (sessionSync)
        {
            return snapshot;
        }
    }

    public async Task<SceneExecutionResult> ExecuteAsync(
        PerformanceScene source,
        CancellationToken cancellationToken = default)
    {
        if (!await executionGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return new SceneExecutionResult(
                SceneExecutionState.Failed,
                "Another Scene is already running; use Stop or Blackout before triggering a different cue.",
                []);
        }

        CancellationTokenSource? session = null;
        var stepResults = new List<SceneStepExecutionResult>();
        try
        {
            var scene = source.Normalize();
            var catalog = (await catalogSource.LoadAsync(cancellationToken).ConfigureAwait(false))
                .ToDictionary(item => item.Id, StringComparer.Ordinal);
            var validation = validator.Validate(scene, catalog);
            if (!validation.IsValid)
            {
                var message = string.Join(" ", validation.Issues
                    .Where(issue => issue.Severity == SceneValidationSeverity.Blocking)
                    .Select(issue => issue.Message));
                Publish(new SceneExecutionSnapshot
                {
                    State = SceneExecutionState.Failed,
                    SceneId = scene.Id,
                    SceneName = scene.DisplayName,
                    ExpandedStepCount = validation.ExpandedStepCount,
                    LastMessage = message
                });
                return new SceneExecutionResult(SceneExecutionState.Failed, message, []);
            }

            session = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            lock (sessionSync)
            {
                activeSession = session;
                requestedTerminalState = SceneExecutionState.Cancelled;
            }

            Publish(new SceneExecutionSnapshot
            {
                State = SceneExecutionState.Running,
                SceneId = scene.Id,
                SceneName = scene.DisplayName,
                ExpandedStepCount = validation.ExpandedStepCount,
                LastMessage = "Scene started."
            });

            string? currentVisualItemId = null;
            string? previousVisualItemId = null;
            for (var index = 0; index < validation.ExpandedSteps.Count; index++)
            {
                session.Token.ThrowIfCancellationRequested();
                var step = validation.ExpandedSteps[index];
                Publish(GetSnapshot() with
                {
                    CurrentStepIndex = index,
                    CurrentStepId = step.Id,
                    LastMessage = $"Running {step.Kind}."
                });
                var result = await ExecuteStepAsync(
                    step,
                    catalog,
                    previousVisualItemId,
                    session.Token).ConfigureAwait(false);
                stepResults.Add(new SceneStepExecutionResult(step.Id, step.Kind, result.Succeeded, result.Message));

                if (result.Succeeded && result.VisualItemId is not null)
                {
                    if (step.Kind == SceneStepKind.RestorePrevious)
                    {
                        (currentVisualItemId, previousVisualItemId) =
                            (previousVisualItemId, currentVisualItemId);
                    }
                    else
                    {
                        previousVisualItemId = currentVisualItemId;
                        currentVisualItemId = result.VisualItemId;
                    }
                }

                if (step.Kind == SceneStepKind.Stop && result.Succeeded)
                {
                    return Finish(
                        SceneExecutionState.Stopped,
                        "Scene reached its Stop step.",
                        stepResults,
                        completedByTerminalStep: true);
                }

                if (step.Kind == SceneStepKind.Blackout && result.Succeeded)
                {
                    return Finish(
                        SceneExecutionState.BlackedOut,
                        "Scene reached its Blackout step; later steps were not run.",
                        stepResults,
                        completedByTerminalStep: true);
                }

                if (!result.Succeeded && scene.FailurePolicy == SceneFailurePolicy.StopScene)
                {
                    return Finish(
                        SceneExecutionState.Failed,
                        $"Scene stopped at {step.Kind}: {result.Message}",
                        stepResults);
                }
            }

            var failures = stepResults.Count(result => !result.Succeeded);
            return Finish(
                failures == 0 ? SceneExecutionState.Completed : SceneExecutionState.CompletedWithFailures,
                failures == 0
                    ? $"Scene {scene.DisplayName} completed."
                    : $"Scene {scene.DisplayName} completed with {failures} failed step(s) under Continue policy.",
                stepResults);
        }
        catch (OperationCanceledException) when (session?.IsCancellationRequested == true || cancellationToken.IsCancellationRequested)
        {
            SceneExecutionState terminal;
            lock (sessionSync)
            {
                terminal = requestedTerminalState;
            }

            return Finish(
                terminal,
                terminal switch
                {
                    SceneExecutionState.Stopped => "Scene stopped; queued output was cancelled.",
                    SceneExecutionState.BlackedOut => "Scene cancelled by Blackout.",
                    _ => "Scene cancelled."
                },
                stepResults);
        }
        finally
        {
            lock (sessionSync)
            {
                if (ReferenceEquals(activeSession, session))
                {
                    activeSession = null;
                }
            }

            session?.Dispose();
            executionGate.Release();
        }
    }

    public void RequestCancel() => Cancel(SceneExecutionState.Cancelled);

    public async Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default)
    {
        Cancel(SceneExecutionState.Stopped);
        return await dispatcher.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default)
    {
        Cancel(SceneExecutionState.BlackedOut);
        return await dispatcher.BlackoutAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<StepResult> ExecuteStepAsync(
        PerformanceSceneStep step,
        IReadOnlyDictionary<string, GalleryItem> catalog,
        string? previousVisualItemId,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (step.Kind)
            {
                case SceneStepKind.Brightness:
                    return ToStepResult(await dispatcher.SetBrightnessAsync(step.Value, cancellationToken).ConfigureAwait(false));
                case SceneStepKind.AnimationSpeed:
                    return ToStepResult(await dispatcher.SetAnimationSpeedAsync(step.Value, cancellationToken).ConfigureAwait(false));
                case SceneStepKind.Face:
                case SceneStepKind.Text:
                case SceneStepKind.Animation:
                {
                    var item = catalog[step.GalleryItemId];
                    var action = await dispatcher.TriggerAsync(item, cancellationToken).ConfigureAwait(false);
                    return new StepResult(action.Succeeded, action.Message, action.Succeeded ? item.Id : null);
                }
                case SceneStepKind.Wait:
                {
                    var deadline = clock.Add(clock.GetTimestamp(), step.Duration);
                    await clock.DelayUntilAsync(deadline, cancellationToken).ConfigureAwait(false);
                    return new StepResult(true, $"Waited {step.Duration.TotalMilliseconds:0} ms.", null);
                }
                case SceneStepKind.RestorePrevious:
                {
                    if (previousVisualItemId is null || !catalog.TryGetValue(previousVisualItemId, out var previous))
                    {
                        return new StepResult(false, "No previous visual exists in this Scene execution.", null);
                    }

                    var action = await dispatcher.TriggerAsync(previous, cancellationToken).ConfigureAwait(false);
                    return new StepResult(action.Succeeded, action.Message, action.Succeeded ? previous.Id : null);
                }
                case SceneStepKind.Stop:
                    return ToStepResult(await dispatcher.StopAsync(cancellationToken).ConfigureAwait(false));
                case SceneStepKind.Blackout:
                    return ToStepResult(await dispatcher.BlackoutAsync(cancellationToken).ConfigureAwait(false));
                default:
                    return new StepResult(false, $"Unsupported expanded step {step.Kind}.", null);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new StepResult(false, ShortMessage(exception), null);
        }
    }

    private SceneExecutionResult Finish(
        SceneExecutionState state,
        string message,
        IReadOnlyList<SceneStepExecutionResult> steps,
        bool completedByTerminalStep = false)
    {
        Publish(GetSnapshot() with
        {
            State = state,
            CurrentStepId = string.Empty,
            LastMessage = message
        });
        return new SceneExecutionResult(state, message, steps, completedByTerminalStep);
    }

    private void Cancel(SceneExecutionState terminalState)
    {
        lock (sessionSync)
        {
            requestedTerminalState = terminalState;
            activeSession?.Cancel();
        }
    }

    private void Publish(SceneExecutionSnapshot value)
    {
        lock (sessionSync)
        {
            snapshot = value;
        }

        SnapshotChanged?.Invoke(this, value);
    }

    private static StepResult ToStepResult(MaskCommandResult result) =>
        new(result.Succeeded, result.Message, null);

    private static string ShortMessage(Exception exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;
        return message.Length <= 160 ? message : string.Concat(message.AsSpan(0, 160), "...");
    }

    private sealed record StepResult(bool Succeeded, string Message, string? VisualItemId);
}
