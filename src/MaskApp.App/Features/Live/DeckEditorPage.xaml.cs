using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Live;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App.Features.Live;

public partial class DeckEditorPage : ContentPage, IQueryAttributable
{
    private readonly LiveViewModel viewModel;
    private string deckId = string.Empty;

    public DeckEditorPage(LiveViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("deckId", out var value))
        {
            deckId = Uri.UnescapeDataString(value?.ToString() ?? string.Empty);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        if (!string.IsNullOrWhiteSpace(deckId))
        {
            await viewModel.SelectDeckAsync(deckId);
        }

        viewModel.Pages.IsManageMode = true;
        viewModel.StartObserving();
    }

    protected override void OnDisappearing()
    {
        viewModel.StopObserving();
        base.OnDisappearing();
    }

    private async void OnAddClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new ContentPickerPage(viewModel.Pages));

    private async void OnPreflightClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.ForPreflight("live-deck", viewModel.SelectedDeck.PageId));

    private void OnToggleSettingsClicked(object? sender, EventArgs e)
    {
        if (!DeckSettingsPanel.IsVisible)
        {
            viewModel.Pages.TogglePageEditorSheetCommand.Execute(null);
        }

        DeckSettingsPanel.IsVisible = !DeckSettingsPanel.IsVisible;
    }

    private async void OnDeleteDeckClicked(object? sender, EventArgs e)
    {
        if (!viewModel.Pages.ConfirmRemovePageCommand.CanExecute(null))
        {
            await DisplayAlertAsync(AppText.Get("KeepOneDeckTitle"), AppText.Get("KeepOneDeckDetail"), AppText.Get("Ok"));
            return;
        }

        if (await DisplayAlertAsync(
            AppText.Get("DeleteDeckTitle"),
            AppText.Get("DeleteDeckDetail"),
            AppText.DeleteDeck,
            AppText.Cancel))
        {
            await viewModel.Pages.ConfirmRemovePageCommand.ExecuteAsync();
            await Shell.Current.GoToAsync(AppRoutes.Back);
        }
    }
}
