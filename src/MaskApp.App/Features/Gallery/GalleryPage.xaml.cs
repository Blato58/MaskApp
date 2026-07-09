using MaskApp.Core.Features.Gallery;
using MaskApp.App.Infrastructure.Accessibility;

namespace MaskApp.App.Features.Gallery;

public partial class GalleryPage : ContentPage
{
    private readonly GalleryViewModel viewModel;
    private readonly IMotionPreference motionPreference;

    public GalleryPage(GalleryViewModel viewModel, IMotionPreference motionPreference)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.motionPreference = motionPreference;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        if (viewModel.Rows.Count > 0)
        {
            Dispatcher.Dispatch(() => LibraryView.ScrollTo(0, -1, ScrollToPosition.Start, animate: false));
        }
    }

    protected override void OnDisappearing()
    {
        viewModel.StopPreviewAnimations();
        base.OnDisappearing();
    }

    private void OnLibraryScrolled(object? sender, ItemsViewScrolledEventArgs e) =>
        viewModel.SetVisibleRowRange(e.FirstVisibleItemIndex, e.LastVisibleItemIndex, motionPreference.IsReducedMotionEnabled);

    private void OnSearchDone(object? sender, EventArgs e) => DismissSearch();

    private void OnDismissSearchClicked(object? sender, EventArgs e) => DismissSearch();

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        DismissSearch();
        await Shell.Current.GoToAsync("library-add");
    }

    private async void OnSendClicked(object? sender, EventArgs e)
    {
        DismissSearch();
        if (sender is Button { CommandParameter: GalleryItem item })
        {
            await viewModel.SendAsync(item);
        }
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        DismissSearch();
        if (sender is Button { CommandParameter: GalleryItem item })
        {
            await OpenEditorAsync(item);
        }
    }

    private static async Task OpenEditorAsync(GalleryItem item)
    {
        if (string.Equals(item.ManageTarget, "text", StringComparison.Ordinal))
        {
            var presetId = item.TextPreset?.Id.Value;
            var route = string.IsNullOrWhiteSpace(presetId)
                ? "text"
                : $"text?presetId={Uri.EscapeDataString(presetId)}";
            await Shell.Current.GoToAsync(route);
            return;
        }

        if (string.Equals(item.ManageTarget, "builtins", StringComparison.Ordinal))
        {
            var record = item.BuiltInAssetRecord;
            var route = record is null
                ? "builtins"
                : $"built-in-detail?type={record.Type}&id={record.Id}";
            await Shell.Current.GoToAsync(route);
            return;
        }

        if (string.Equals(item.ManageTarget, "faces", StringComparison.Ordinal))
        {
            await Shell.Current.GoToAsync("faces");
        }
    }

    private void DismissSearch() => SearchBox.Unfocus();
}
