using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.Core.Features.Stage;

public sealed class PerformanceStageShowSource : IStageShowSource
{
    private readonly PagesStageShowSource pagesSource;
    private readonly SetlistCoordinator setlistCoordinator;
    private readonly ISceneCatalogSource catalogSource;
    private readonly IFacePatternStore facePatternStore;
    private readonly SceneReadinessEvaluator readinessEvaluator;
    private SetlistSnapshot setlist = new("", "Pages", 0, 0, null, null);

    public PerformanceStageShowSource(
        PagesStageShowSource pagesSource,
        SetlistCoordinator setlistCoordinator,
        ISceneCatalogSource catalogSource,
        IFacePatternStore facePatternStore,
        SceneReadinessEvaluator readinessEvaluator)
    {
        this.pagesSource = pagesSource;
        this.setlistCoordinator = setlistCoordinator;
        this.catalogSource = catalogSource;
        this.facePatternStore = facePatternStore;
        this.readinessEvaluator = readinessEvaluator;
    }

    public async Task<StageShowSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
    {
        setlist = await setlistCoordinator.InitializeAsync(cancellationToken).ConfigureAwait(false);
        return setlist.HasSetlist
            ? await CreateSetlistSnapshotAsync(cancellationToken).ConfigureAwait(false)
            : await pagesSource.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<StageShowSnapshot> SelectPageAsync(
        int pageIndex,
        CancellationToken cancellationToken = default)
    {
        if (!setlist.HasSetlist)
        {
            return await pagesSource.SelectPageAsync(pageIndex, cancellationToken).ConfigureAwait(false);
        }

        setlist = await setlistCoordinator.SelectAsync(pageIndex, cancellationToken).ConfigureAwait(false);
        return await CreateSetlistSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<GalleryActionResult> TriggerAsync(
        string tileId,
        CancellationToken cancellationToken = default)
    {
        if (!setlist.HasSetlist)
        {
            return await pagesSource.TriggerAsync(tileId, cancellationToken).ConfigureAwait(false);
        }

        if (setlist.CurrentCue is null || !string.Equals(setlist.CurrentCue.Id, tileId, StringComparison.Ordinal))
        {
            return GalleryActionResult.Failure("This setlist cue is no longer current.");
        }

        var result = await setlistCoordinator.TriggerCurrentAsync(cancellationToken).ConfigureAwait(false);
        return new GalleryActionResult(result.Succeeded, result.Message);
    }

    public void StartObservingTransportState() => pagesSource.StartObservingTransportState();

    public void StopObservingTransportState() => pagesSource.StopObservingTransportState();

    private async Task<StageShowSnapshot> CreateSetlistSnapshotAsync(CancellationToken cancellationToken)
    {
        if (setlist.CurrentCue is null || setlist.CurrentScene is null)
        {
            return new StageShowSnapshot(
                setlist.SetlistId,
                setlist.SetlistName,
                "#A78BFA",
                setlist.CueIndex,
                setlist.CueCount,
                [],
                "Cue",
                setlist.NextCue?.Label ?? string.Empty);
        }

        var catalog = (await catalogSource.LoadAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(item => item.Id, StringComparer.Ordinal);
        var faceState = await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var readiness = readinessEvaluator.Evaluate(setlist.CurrentScene, catalog, faceState);
        var scene = setlist.CurrentScene.Normalize();
        var cue = setlist.CurrentCue.Normalize(setlist.CueIndex);
        return new StageShowSnapshot(
            cue.Id,
            $"{setlist.SetlistName} · {cue.Label}",
            scene.ColorHex,
            setlist.CueIndex,
            setlist.CueCount,
            [new StageTile(
                cue.Id,
                cue.Label,
                scene.DisplayName,
                scene.ColorHex,
                GalleryItemType.Scene,
                readiness.IsReady,
                false,
                readiness.Message)],
            "Cue",
            setlist.NextCue?.Label ?? string.Empty);
    }
}
