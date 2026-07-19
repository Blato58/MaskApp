using MaskApp.App.Controls;
using MaskApp.App.Resources.Strings;
using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Live;

namespace MaskApp.App.Features.Live;

public partial class LivePage : ContentPage
{
    private readonly LiveViewModel viewModel;

    public LivePage(LiveViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        viewModel.StartObserving();
    }

    protected override void OnDisappearing()
    {
        viewModel.StopObserving();
        base.OnDisappearing();
    }

    private async void OnConnectionClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.DevicePicker);

    private async void OnEditDeckClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.ForDeckEditor(viewModel.SelectedDeck.PageId));

    private async void OnDeckChanged(object? sender, EventArgs e)
    {
        if (sender is Picker { SelectedItem: GalleryPageTab deck })
        {
            await viewModel.SelectDeckAsync(deck.PageId);
        }
    }

    private async void OnTileConfirmationRequested(object? sender, EventArgs e)
    {
        if (sender is not LiveTile { Item: { } item } tile)
        {
            return;
        }

        if (await DisplayAlertAsync(
            AppText.Get("SendActionTitle"),
            string.Format(AppText.Get("SendActionMessage"), item.Label),
            AppText.Get("Send"),
            AppText.Cancel))
        {
            tile.ExecuteAction();
        }
    }
}
