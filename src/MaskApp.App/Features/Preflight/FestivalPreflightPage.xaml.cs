using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Experience;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App.Features.Preflight;

public partial class FestivalPreflightPage : ContentPage, IQueryAttributable
{
    private readonly FestivalPreflightViewModel viewModel;
    private string scope = "whole-show";
    private string? sourceId;

    public FestivalPreflightPage(FestivalPreflightViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("scope", out var scopeValue))
        {
            scope = Uri.UnescapeDataString(scopeValue?.ToString() ?? "whole-show");
        }

        if (query.TryGetValue("sourceId", out var sourceValue))
        {
            sourceId = Uri.UnescapeDataString(sourceValue?.ToString() ?? string.Empty);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync(scope, sourceId);
    }

    private async void OnBackClicked(object? sender, EventArgs args) =>
        await Shell.Current.GoToAsync(AppRoutes.Back);

    private async void OnAcknowledgeFlashRiskClicked(object? sender, EventArgs args)
    {
        var accepted = await DisplayAlertAsync(
            AppText.Get("PhotosensitivityWarning"),
            AppText.Get("PhotosensitivityWarningDetail"),
            AppText.Get("AcknowledgeExactRevisions"),
            AppText.Cancel);
        if (accepted)
        {
            await viewModel.AcknowledgeBlockedFlashRiskAsync();
        }
    }

    private async void OnRevokeFlashRiskClicked(object? sender, EventArgs args)
    {
        var revoke = await DisplayAlertAsync(
            AppText.Get("RevokeFlashOverridesTitle"),
            AppText.Get("RevokeFlashOverridesDetail"),
            AppText.Get("Revoke"),
            AppText.Get("KeepOverrides"));
        if (revoke)
        {
            await viewModel.RevokeFlashRiskOverridesAsync();
        }
    }
}
