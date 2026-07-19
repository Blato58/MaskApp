using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Shows;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App.Features.Shows;

public partial class ShowsPage : ContentPage
{
    private readonly ShowsViewModel viewModel;

    public ShowsPage(ShowsViewModel viewModel)
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

    private async void OnDeviceClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.DevicePicker);

    private async void OnEditShowClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.ForShowBuilder(viewModel.ActiveShow.Id));

    private async void OnPreflightClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.ForPreflight("active-show", viewModel.ActiveShow.Id));

    private async void OnStageClicked(object? sender, EventArgs e)
    {
        if (await DisplayAlertAsync(
            AppText.Get("EnterStageModeTitle"),
            AppText.Get("EnterStageModeDetail"),
            AppText.EnterStage,
            AppText.Cancel))
        {
            await Shell.Current.GoToAsync(AppRoutes.Stage);
        }
    }
}
