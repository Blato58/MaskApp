using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Features.Gallery;

public partial class GalleryPage : ContentPage
{
    private readonly GalleryViewModel viewModel;

    public GalleryPage(GalleryViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
    }

    private void OnSearchDone(object? sender, EventArgs e) => DismissSearch();

    private void OnDismissSearchClicked(object? sender, EventArgs e) => DismissSearch();

    private void OnToggleHeaderClicked(object? sender, EventArgs e)
    {
        LibraryHeaderDetails.IsVisible = !LibraryHeaderDetails.IsVisible;
        LibraryHeaderToggleButton.Text = LibraryHeaderDetails.IsVisible ? "Hide" : "Tools";
        if (!LibraryHeaderDetails.IsVisible)
        {
            DismissSearch();
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        DismissSearch();
        await Shell.Current.GoToAsync("library-add");
    }

    private async void OnBrowseCardTapped(object? sender, TappedEventArgs e)
    {
        DismissSearch();
        if (e.Parameter is GalleryItem item)
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
            await Shell.Current.GoToAsync("builtins");
            return;
        }

        if (string.Equals(item.ManageTarget, "faces", StringComparison.Ordinal))
        {
            await Shell.Current.GoToAsync("faces");
        }
    }

    private void DismissSearch() => SearchBox.Unfocus();
}
