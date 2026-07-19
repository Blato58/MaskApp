using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Experience;

namespace MaskApp.App.Features.Onboarding;

public partial class OnboardingPage : ContentPage
{
    private readonly ConnectViewModel connectViewModel;
    private readonly IAppExperienceSettingsStore settingsStore;

    public OnboardingPage(ConnectViewModel connectViewModel, IAppExperienceSettingsStore settingsStore)
    {
        InitializeComponent();
        this.connectViewModel = connectViewModel;
        this.settingsStore = settingsStore;
        BindingContext = connectViewModel;
        LocationRationale.IsVisible = DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major <= 11;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        connectViewModel.PropertyChanged += OnConnectPropertyChanged;
        await connectViewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        connectViewModel.PropertyChanged -= OnConnectPropertyChanged;
        base.OnDisappearing();
    }

    private void OnContinueClicked(object? sender, EventArgs e)
    {
        WelcomePanel.IsVisible = false;
        BluetoothPanel.IsVisible = true;
    }

    private void OnAllowBluetoothClicked(object? sender, EventArgs e)
    {
        BluetoothPanel.IsVisible = false;
        ChoosePanel.IsVisible = true;
        connectViewModel.StartScanCommand.Execute(null);
    }

    private void OnConnectClicked(object? sender, EventArgs e) => connectViewModel.ConnectCommand.Execute(null);

    private async void OnSkipClicked(object? sender, EventArgs e) => await CompleteAsync();

    private void OnOpenSettingsClicked(object? sender, EventArgs e) => AppInfo.ShowSettingsUI();

    private async void OnConnectPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectViewModel.ConnectionState) && connectViewModel.IsConnected)
        {
            await CompleteAsync();
        }
    }

    private async Task CompleteAsync()
    {
        var settings = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(settings with { OnboardingCompleted = true, OnboardingStep = 2 });
        await Shell.Current.GoToAsync(AppRoutes.LiveRoot);
    }
}
