using MaskApp.Core.Features.Home;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Navigation;

namespace MaskApp.App.Features.Home;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HomeViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private static Task OpenConnectAsync() =>
        Shell.Current.GoToAsync(AppRouteCatalog.AbsoluteRoot(AppRouteCatalog.DeviceRoot));

    private static Task OpenTextAsync() => Shell.Current.GoToAsync("text");

    private async void OnOpenConnectClicked(object? sender, EventArgs e)
    {
        await OpenConnectAsync();
    }

    private async void OnOpenTextClicked(object? sender, EventArgs e)
    {
        await OpenTextAsync();
    }

    private void OnForegroundColorClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not HomeViewModel viewModel ||
            sender is not Button { CommandParameter: QuickCaptionForegroundPresetOption option })
        {
            return;
        }

        viewModel.SelectedQuickCaptionForegroundPreset = option;
    }
}
