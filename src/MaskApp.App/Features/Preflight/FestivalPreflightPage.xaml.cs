using MaskApp.App.Resources.Strings;
using MaskApp.Core.Features.Preflight;

namespace MaskApp.App.Features.Preflight;

public partial class FestivalPreflightPage : ContentPage
{
    private readonly FestivalPreflightViewModel viewModel;

    public FestivalPreflightPage(FestivalPreflightViewModel viewModel)
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

    private async void OnBackClicked(object? sender, EventArgs args) =>
        await Shell.Current.GoToAsync("..");

    private async void OnAcknowledgeFlashRiskClicked(object? sender, EventArgs args)
    {
        var accepted = await DisplayAlertAsync(
            AppText.Get("Ui374"),
            AppText.Get("Ui375"),
            AppText.Get("Ui376"),
            AppText.Get("Ui056"));
        if (accepted)
        {
            await viewModel.AcknowledgeBlockedFlashRiskAsync();
        }
    }

    private async void OnRevokeFlashRiskClicked(object? sender, EventArgs args)
    {
        var revoke = await DisplayAlertAsync(
            AppText.Get("Ui377"),
            AppText.Get("Ui378"),
            AppText.Get("Ui379"),
            AppText.Get("Ui380"));
        if (revoke)
        {
            await viewModel.RevokeFlashRiskOverridesAsync();
        }
    }
}
