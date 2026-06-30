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
}
