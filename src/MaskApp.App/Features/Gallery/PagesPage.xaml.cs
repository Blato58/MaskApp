using MaskApp.Core.Features.Gallery;
using MaskApp.App.Infrastructure.Accessibility;

namespace MaskApp.App.Features.Gallery;

public partial class PagesPage : ContentPage
{
    private readonly PagesViewModel viewModel;
    private readonly IMotionPreference motionPreference;

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
        viewModel.StopMaskAnimation();
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
}
