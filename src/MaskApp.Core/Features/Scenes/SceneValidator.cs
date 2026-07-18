using MaskApp.Core.Features.Gallery;

namespace MaskApp.Core.Features.Scenes;

public enum SceneValidationSeverity
{
    Warning,
    Blocking
}

public sealed record SceneValidationIssue(
    string Code,
    SceneValidationSeverity Severity,
    string Message,
    string RecoveryAction,
    string? StepId = null);

public sealed record SceneValidationResult(
    bool IsValid,
    IReadOnlyList<PerformanceSceneStep> ExpandedSteps,
    IReadOnlyList<SceneValidationIssue> Issues)
{
    public int ExpandedStepCount => ExpandedSteps.Count;
}

public sealed class SceneValidator
{
    public SceneValidationResult Validate(
        PerformanceScene source,
        IReadOnlyDictionary<string, GalleryItem> catalog)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(catalog);
        PerformanceScene scene;
        try
        {
            scene = source.Normalize();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return new SceneValidationResult(
                false,
                [],
                [new SceneValidationIssue("scene-invalid", SceneValidationSeverity.Blocking, exception.Message, "Edit the invalid Scene field or step.")]);
        }

        var issues = new List<SceneValidationIssue>();
        var expanded = new List<PerformanceSceneStep>();
        var totalWait = TimeSpan.Zero;
        for (var index = 0; index < scene.Steps.Count; index++)
        {
            var step = scene.Steps[index];
            if (step.Kind == SceneStepKind.Repeat)
            {
                var repeatStart = scene.Steps.ToList().FindIndex(candidate =>
                    string.Equals(candidate.Id, step.RepeatFromStepId, StringComparison.Ordinal));
                if (repeatStart < 0 || repeatStart >= index)
                {
                    issues.Add(new SceneValidationIssue(
                        "repeat-target-invalid",
                        SceneValidationSeverity.Blocking,
                        "Repeat must target an earlier Scene step.",
                        "Choose the first step of an earlier bounded segment.",
                        step.Id));
                    continue;
                }

                var segment = scene.Steps.Skip(repeatStart).Take(index - repeatStart).ToArray();
                if (segment.Any(candidate => candidate.Kind == SceneStepKind.Repeat))
                {
                    issues.Add(new SceneValidationIssue(
                        "nested-repeat",
                        SceneValidationSeverity.Blocking,
                        "Repeat segments cannot contain another Repeat step.",
                        "Split the Scene into explicit bounded segments.",
                        step.Id));
                    continue;
                }

                for (var repeat = 1; repeat < step.RepeatCount; repeat++)
                {
                    expanded.AddRange(segment);
                    totalWait += TimeSpan.FromTicks(segment
                        .Where(candidate => candidate.Kind == SceneStepKind.Wait)
                        .Sum(candidate => candidate.Duration.Ticks));
                }

                continue;
            }

            ValidateContentStep(step, catalog, issues);
            expanded.Add(step);
            if (step.Kind == SceneStepKind.Wait)
            {
                totalWait += step.Duration;
            }
        }

        if (expanded.Count > PerformanceScene.MaxExpandedSteps)
        {
            issues.Add(new SceneValidationIssue(
                "expanded-step-limit",
                SceneValidationSeverity.Blocking,
                $"The Scene expands to {expanded.Count} steps, exceeding the {PerformanceScene.MaxExpandedSteps}-step execution limit.",
                "Reduce repeat counts or split the Scene."));
        }

        if (totalWait > PerformanceScene.MaxTotalWaitDuration)
        {
            issues.Add(new SceneValidationIssue(
                "wait-budget-exceeded",
                SceneValidationSeverity.Blocking,
                $"The expanded Scene waits for {totalWait.TotalSeconds:0} seconds, exceeding the {PerformanceScene.MaxTotalWaitDuration.TotalMinutes:0}-minute limit.",
                "Reduce waits or repeat counts."));
        }

        return new SceneValidationResult(
            issues.All(issue => issue.Severity != SceneValidationSeverity.Blocking),
            expanded.Take(PerformanceScene.MaxExpandedSteps).ToArray(),
            issues);
    }

    public IReadOnlyList<SceneValidationIssue> ValidateSetlist(
        PerformanceSetlist source,
        IReadOnlyDictionary<string, PerformanceScene> scenes)
    {
        PerformanceSetlist setlist;
        try
        {
            setlist = source.Normalize();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return [new SceneValidationIssue("setlist-invalid", SceneValidationSeverity.Blocking, exception.Message, "Edit the invalid setlist field or cue.")];
        }

        return setlist.Cues
            .Where(cue => !scenes.ContainsKey(cue.SceneId))
            .Select(cue => new SceneValidationIssue(
                "setlist-scene-missing",
                SceneValidationSeverity.Blocking,
                $"{cue.Label} references missing Scene {cue.SceneId}.",
                "Choose an existing Scene or remove the cue.",
                cue.Id))
            .ToArray();
    }

    private static void ValidateContentStep(
        PerformanceSceneStep step,
        IReadOnlyDictionary<string, GalleryItem> catalog,
        ICollection<SceneValidationIssue> issues)
    {
        if (step.Kind is not (SceneStepKind.Face or SceneStepKind.Text or SceneStepKind.Animation))
        {
            return;
        }

        if (!catalog.TryGetValue(step.GalleryItemId, out var item))
        {
            issues.Add(new SceneValidationIssue(
                "content-missing",
                SceneValidationSeverity.Blocking,
                $"{step.Kind} step references missing Library item {step.GalleryItemId}.",
                "Choose an existing compatible Library item.",
                step.Id));
            return;
        }

        var isCompatible = step.Kind switch
        {
            SceneStepKind.Face => item.Type is GalleryItemType.CustomStaticFace or GalleryItemType.BuiltInStaticImage,
            SceneStepKind.Text => item.Type == GalleryItemType.TextPreset,
            SceneStepKind.Animation => item.Type is GalleryItemType.BuiltInAnimation
                or GalleryItemType.AppBuiltInAnimation
                or GalleryItemType.CustomAnimation,
            _ => true
        };
        if (!isCompatible)
        {
            issues.Add(new SceneValidationIssue(
                "content-type-mismatch",
                SceneValidationSeverity.Blocking,
                $"{item.Title} is not compatible with a {step.Kind} step.",
                $"Choose a {step.Kind.ToString().ToLowerInvariant()} Library item.",
                step.Id));
        }
        else if (!item.CanSend)
        {
            issues.Add(new SceneValidationIssue(
                "content-unavailable",
                SceneValidationSeverity.Blocking,
                $"{item.Title} is not currently sendable.",
                "Open the item and resolve its validation error.",
                step.Id));
        }
    }
}
