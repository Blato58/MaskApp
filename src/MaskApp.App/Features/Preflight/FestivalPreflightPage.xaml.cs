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
            "Photosensitivity warning",
            "These exact animation revisions exceed the conservative limit of three full flashes per second. Rapid flashing may trigger seizures or other photosensitive reactions. Acknowledging does not make the content safe, and Blackout must remain immediately available. Record an explicit override for the revisions shown in this Preflight report?",
            "Acknowledge exact revisions",
            "Cancel");
        if (accepted)
        {
            await viewModel.AcknowledgeBlockedFlashRiskAsync();
        }
    }

    private async void OnRevokeFlashRiskClicked(object? sender, EventArgs args)
    {
        var revoke = await DisplayAlertAsync(
            "Revoke flash-risk overrides?",
            "The affected animation revisions will be blocked again until they are edited to pass safety analysis or explicitly acknowledged again.",
            "Revoke",
            "Keep overrides");
        if (revoke)
        {
            await viewModel.RevokeFlashRiskOverridesAsync();
        }
    }
}
