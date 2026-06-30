using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Features.Gallery;

public partial class PagesPage : ContentPage
{
    private readonly PagesViewModel viewModel;

    public PagesPage(PagesViewModel viewModel)
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

    private void OnToggleHeaderClicked(object? sender, EventArgs e)
    {
        PagesHeaderDetails.IsVisible = !PagesHeaderDetails.IsVisible;
        PagesHeaderToggleButton.Text = PagesHeaderDetails.IsVisible ? "Hide" : "Tools";
    }

    private async void OnAddItemsClicked(object? sender, EventArgs e)
    {
        var pageId = Uri.EscapeDataString(viewModel.SelectedPage.PageId);
        await Shell.Current.GoToAsync($"page-add-item?pageId={pageId}");
    }
}
