using MaskApp.Core.Features.Gallery;

namespace MaskApp.Core.Features.Stage;

public sealed class PagesStageShowSource : IStageShowSource
{
    private readonly PagesViewModel pagesViewModel;

    public PagesStageShowSource(PagesViewModel pagesViewModel)
    {
        this.pagesViewModel = pagesViewModel;
    }

    public async Task<StageShowSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
    {
        await pagesViewModel.InitializeAsync(cancellationToken).ConfigureAwait(false);
        return CreateSnapshot();
    }

    public async Task<StageShowSnapshot> SelectPageAsync(
        int pageIndex,
        CancellationToken cancellationToken = default)
    {
        var pages = pagesViewModel.Pages;
        if (pages.Count == 0)
        {
            return CreateSnapshot();
        }

        var normalizedIndex = Math.Clamp(pageIndex, 0, pages.Count - 1);
        await pagesViewModel.SelectPageAsync(pages[normalizedIndex].PageId, cancellationToken)
            .ConfigureAwait(false);
        return CreateSnapshot();
    }

    public async Task<GalleryActionResult> TriggerAsync(
        string tileId,
        CancellationToken cancellationToken = default)
    {
        return await pagesViewModel.SendStageShortcutAsync(tileId, cancellationToken).ConfigureAwait(false);
    }

    public void StartObservingTransportState() => pagesViewModel.StartObservingTransportState();

    public void StopObservingTransportState() => pagesViewModel.StopObservingTransportState();

    private StageShowSnapshot CreateSnapshot()
    {
        var pages = pagesViewModel.Pages;
        var pageIndex = Array.FindIndex(
            pages.ToArray(),
            item => string.Equals(item.PageId, pagesViewModel.SelectedPage.PageId, StringComparison.Ordinal));
        return new StageShowSnapshot(
            pagesViewModel.SelectedPage.PageId,
            pagesViewModel.SelectedPageTitle,
            pagesViewModel.SelectedPageColorHex,
            Math.Max(0, pageIndex),
            pages.Count,
            pagesViewModel.Shortcuts.Select(CreateTile).ToArray());
    }

    private static StageTile CreateTile(GalleryPageShortcutCard shortcut) =>
        new(
            shortcut.SlotId,
            shortcut.Label,
            shortcut.Subtitle,
            shortcut.ColorHex,
            shortcut.Item.Type,
            !shortcut.IsFastSlotCapable || shortcut.IsFastSlotPrepared,
            false,
            shortcut.FastSlotStatusText,
            shortcut.Item.PreviewResourceName,
            shortcut.Item.FacePattern,
            shortcut.Item.PreviewIsAnimated);
}
