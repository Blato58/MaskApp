using MaskApp.Core.Features.Gallery;
using MaskApp.App.Infrastructure.Accessibility;

namespace MaskApp.App.Features.Gallery;

public partial class PagesPage : ContentPage
{
    private readonly PagesViewModel viewModel;
    private readonly IMotionPreference motionPreference;
    private string? draggedPageId;
    private string? draggedShortcutId;
    private string? draggedShortcutPageId;

    public PagesPage(PagesViewModel viewModel, IMotionPreference motionPreference)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.motionPreference = motionPreference;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        viewModel.StartObservingTransportState();
        await viewModel.InitializeAsync();
        if (viewModel.Shortcuts.Count > 0)
        {
            Dispatcher.Dispatch(() => ShortcutsView.ScrollTo(0, -1, ScrollToPosition.Start, animate: false));
        }
    }

    protected override void OnDisappearing()
    {
        viewModel.StopPreviewAnimations();
        viewModel.StopObservingTransportState();
        base.OnDisappearing();
    }

    private void OnShortcutsScrolled(object? sender, ItemsViewScrolledEventArgs e) =>
        viewModel.SetVisibleShortcutRange(e.FirstVisibleItemIndex, e.LastVisibleItemIndex, motionPreference.IsReducedMotionEnabled);

    private async void OnAddItemsClicked(object? sender, EventArgs e)
    {
        var pageId = Uri.EscapeDataString(viewModel.SelectedPage.PageId);
        await Shell.Current.GoToAsync($"page-add-item?pageId={pageId}");
    }

    private async void OnPreflightClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("preflight");

    private async void OnStageClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("stage");

    private void OnPageDragStarting(object? sender, DragStartingEventArgs e)
    {
        draggedPageId = viewModel.IsManageMode &&
            sender is DragGestureRecognizer { BindingContext: GalleryPageTab page }
                ? page.PageId
                : null;
        e.Cancel = draggedPageId is null;
    }

    private async void OnPageDropped(object? sender, DropEventArgs e)
    {
        try
        {
            if (draggedPageId is { } sourceId &&
                sender is DropGestureRecognizer { BindingContext: GalleryPageTab target })
            {
                await viewModel.MovePageToAsync(sourceId, target.PageId);
            }
        }
        finally
        {
            draggedPageId = null;
        }
    }

    private void OnShortcutDragStarting(object? sender, DragStartingEventArgs e)
    {
        if (viewModel.IsManageMode &&
            sender is DragGestureRecognizer { BindingContext: GalleryPageShortcutCard shortcut })
        {
            draggedShortcutId = shortcut.SlotId;
            draggedShortcutPageId = viewModel.SelectedPage.PageId;
        }
        else
        {
            draggedShortcutId = null;
            draggedShortcutPageId = null;
        }

        e.Cancel = draggedShortcutId is null;
    }

    private async void OnShortcutDropped(object? sender, DropEventArgs e)
    {
        try
        {
            if (draggedShortcutId is { } sourceId &&
                string.Equals(draggedShortcutPageId, viewModel.SelectedPage.PageId, StringComparison.Ordinal) &&
                sender is DropGestureRecognizer { BindingContext: GalleryPageShortcutCard target })
            {
                await viewModel.MoveItemToAsync(sourceId, target.SlotId);
            }
        }
        finally
        {
            draggedShortcutId = null;
            draggedShortcutPageId = null;
        }
    }
}
