using MaskApp.Core.Features.Rave;

namespace MaskApp.App.Features.Rave;

public partial class RavePage : ContentPage
{
    private readonly RaveViewModel viewModel;

    public RavePage(RaveViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeArchiveAsync();
    }

    private static async void OnConnectClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//connect");
    }
}
