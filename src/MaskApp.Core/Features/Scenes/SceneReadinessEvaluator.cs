using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;

namespace MaskApp.Core.Features.Scenes;

public sealed record SceneReadinessResult(
    bool IsReady,
    string Message,
    SceneValidationResult Validation);

public sealed class SceneReadinessEvaluator
{
    private readonly SceneValidator validator;

    public SceneReadinessEvaluator(SceneValidator? validator = null)
    {
        this.validator = validator ?? new SceneValidator();
    }

    public SceneReadinessResult Evaluate(
        PerformanceScene scene,
        IReadOnlyDictionary<string, GalleryItem> catalog,
        FacePatternStoreState faceState)
    {
        var validation = validator.Validate(scene, catalog);
        if (!validation.IsValid)
        {
            return new SceneReadinessResult(
                false,
                string.Join(" ", validation.Issues
                    .Where(issue => issue.Severity == SceneValidationSeverity.Blocking)
                    .Select(issue => issue.Message)),
                validation);
        }

        var missing = validation.ExpandedSteps
            .Where(step => !string.IsNullOrWhiteSpace(step.GalleryItemId))
            .Select(step => catalog[step.GalleryItemId])
            .Where(item => !IsPersistentDependencyPrepared(item, faceState))
            .DistinctBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => item.Title)
            .ToArray();
        return missing.Length == 0
            ? new SceneReadinessResult(true, "Every persistent DIY dependency is prepared.", validation)
            : new SceneReadinessResult(
                false,
                $"Prepare {string.Join(", ", missing)} before Stage use.",
                validation);
    }

    private static bool IsPersistentDependencyPrepared(
        GalleryItem item,
        FacePatternStoreState faceState) =>
        item.Type switch
        {
            GalleryItemType.CustomStaticFace when item.FacePattern is not null =>
                DiySlotPlaybackCoordinator.IsFacePrepared(item.FacePattern, faceState),
            GalleryItemType.AppBuiltInAnimation when item.AppAnimation is not null =>
                DiySlotPlaybackCoordinator.IsAnimationPrepared(item.AppAnimation, faceState),
            GalleryItemType.CustomAnimation when item.PerformanceAnimation is not null =>
                DiySlotPlaybackCoordinator.IsAnimationPrepared(item.PerformanceAnimation, faceState),
            _ => true
        };
}
