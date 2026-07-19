using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.MaskControl;
using MaskApp.App.Resources.Strings;
using System.Globalization;
using MaskApp.App.Infrastructure.Accessibility;
using MaskApp.App.Features.Stage;

namespace MaskApp.App.Features.Device;

public partial class DevicePage : ContentPage
{
    private readonly ConnectViewModel viewModel;
    private readonly IAppExperienceSettingsStore settingsStore;
    private readonly ExperienceMotionPreference motionPreference;
    private readonly MauiStageDeviceFeedback stageFeedback;
    private AppExperienceSettings settings = AppExperienceSettings.Defaults;
    private bool loadingSettings;

    public DevicePage(
        ConnectViewModel viewModel,
        MaskControlViewModel maskControlViewModel,
        IAppExperienceSettingsStore settingsStore,
        ExperienceMotionPreference motionPreference,
        MauiStageDeviceFeedback stageFeedback)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.settingsStore = settingsStore;
        this.motionPreference = motionPreference;
        this.stageFeedback = stageFeedback;
        BindingContext = viewModel;
        MaskControls.BindingContext = maskControlViewModel;
        AppearancePicker.ItemsSource = Enum.GetValues<AppAppearance>().Select(value => value.ToString()).ToArray();
        LanguagePicker.ItemsSource = Enum.GetValues<AppLanguage>().Select(value => value.ToString()).ToArray();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        await LoadSettingsAsync();
        CloseButton.IsVisible = Navigation.ModalStack.Contains(this);
    }

    private async void OnChooseMaskClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.DevicePicker);

    private async void OnDiagnosticsClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.Diagnostics);

    private async void OnCloseClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();

    private async Task LoadSettingsAsync()
    {
        loadingSettings = true;
        settings = await settingsStore.LoadAsync();
        AppearancePicker.SelectedIndex = (int)settings.Appearance;
        LanguagePicker.SelectedIndex = (int)settings.Language;
        ReduceMotionSwitch.IsToggled = settings.ReduceMotionOverride ?? false;
        HapticsSwitch.IsToggled = settings.HapticsEnabled;
        loadingSettings = false;
        ApplyAppearance(settings.Appearance);
    }

    private async void OnAppearanceChanged(object? sender, EventArgs e)
    {
        if (loadingSettings || AppearancePicker.SelectedIndex < 0)
        {
            return;
        }

        settings = settings with { Appearance = (AppAppearance)AppearancePicker.SelectedIndex };
        ApplyAppearance(settings.Appearance);
        await settingsStore.SaveAsync(settings);
    }

    private async void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (loadingSettings || LanguagePicker.SelectedIndex < 0)
        {
            return;
        }

        settings = settings with { Language = (AppLanguage)LanguagePicker.SelectedIndex };
        AppText.Culture = settings.Language switch
        {
            AppLanguage.English => CultureInfo.GetCultureInfo("en"),
            AppLanguage.Czech => CultureInfo.GetCultureInfo("cs-CZ"),
            _ => null
        };
        await settingsStore.SaveAsync(settings);
    }

    private async void OnReduceMotionToggled(object? sender, ToggledEventArgs e)
    {
        if (loadingSettings)
        {
            return;
        }

        settings = settings with { ReduceMotionOverride = e.Value };
        motionPreference.SetOverride(e.Value);
        await settingsStore.SaveAsync(settings);
    }

    private async void OnHapticsToggled(object? sender, ToggledEventArgs e)
    {
        if (loadingSettings)
        {
            return;
        }

        settings = settings with { HapticsEnabled = e.Value };
        stageFeedback.SetEnabled(e.Value);
        await settingsStore.SaveAsync(settings);
    }

    private async void OnResetOnboardingClicked(object? sender, EventArgs e)
    {
        settings = settings with { OnboardingCompleted = false, OnboardingStep = 0 };
        await settingsStore.SaveAsync(settings);
        await Shell.Current.GoToAsync(AppRoutes.Onboarding);
    }

    private static void ApplyAppearance(AppAppearance appearance) =>
        Application.Current!.UserAppTheme = appearance switch
        {
            AppAppearance.Dark => AppTheme.Dark,
            AppAppearance.Light => AppTheme.Light,
            _ => AppTheme.Unspecified
        };
}
