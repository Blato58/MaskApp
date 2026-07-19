using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Experience;

namespace MaskApp.App.Features.Device;

public partial class DevicePickerPage : ContentPage
{
    private readonly ConnectViewModel viewModel;

    public DevicePickerPage(ConnectViewModel viewModel)
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

    private async void OnCloseClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync(AppRoutes.Back);

    private void OnOpenSettingsClicked(object? sender, EventArgs e) => AppInfo.ShowSettingsUI();
}
