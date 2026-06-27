using MaskApp.Core.Features.Home;

namespace MaskApp.App.Features.Home;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private static Task OpenConnectAsync() => Shell.Current.GoToAsync("//connect");

    private static Task OpenTextAsync() => Shell.Current.GoToAsync("//text");

    private async void OnOpenConnectClicked(object? sender, EventArgs e)
    {
        await OpenConnectAsync();
    }

    private async void OnOpenTextClicked(object? sender, EventArgs e)
    {
        await OpenTextAsync();
    }
}
