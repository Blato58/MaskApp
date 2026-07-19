using System.ComponentModel;
using System.Diagnostics;
using MaskApp.Core.Features.Stage;
using MaskApp.Core.Features.Experience;
using MaskApp.App.Features.Device;
using MaskApp.App.Resources.Strings;
using Microsoft.Extensions.DependencyInjection;

namespace MaskApp.App.Features.Stage;

public partial class StageModePage : ContentPage
{
    private readonly StageModeViewModel viewModel;
    private readonly IServiceProvider services;
    private long? unlockPressedAt;

    public StageModePage(StageModeViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.services = services;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        await viewModel.ActivateAsync();
        UpdateItemsLayout();
    }

    protected override async void OnDisappearing()
    {
        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        await viewModel.DeactivateAsync();
        base.OnDisappearing();
    }

    private async void OnStageTileClicked(object? sender, EventArgs args)
    {
        if (sender is Button { CommandParameter: StageTile tile })
        {
            await viewModel.TriggerAsync(tile);
        }
    }

    private async void OnHoldTilePressed(object? sender, EventArgs args)
    {
        if (sender is Button { CommandParameter: StageTile tile })
        {
            await viewModel.BeginHoldAsync(tile);
        }
    }

    private async void OnHoldTileReleased(object? sender, EventArgs args)
    {
        if (sender is Button { CommandParameter: StageTile tile })
        {
            await viewModel.EndHoldAsync(tile);
        }
    }

    private async void OnAccessibleHoldTileClicked(object? sender, EventArgs args)
    {
        if (sender is not Button { CommandParameter: StageTile tile })
        {
            return;
        }

        if (!await DisplayAlertAsync(
            AppText.Get("StartHoldTitle"),
            string.Format(AppText.Get("StartHoldMessage"), tile.Label),
            AppText.Get("Start"),
            AppText.Cancel))
        {
            return;
        }

        await viewModel.BeginHoldAsync(tile);
        await DisplayAlertAsync(AppText.Get("HoldActionPlaying"), AppText.Get("HoldActionPlayingDetail"), AppText.Stop);
        await viewModel.EndHoldAsync(tile);
    }

    private void OnUnlockPressed(object? sender, EventArgs args) =>
        unlockPressedAt = Stopwatch.GetTimestamp();

    private async void OnUnlockReleased(object? sender, EventArgs args)
    {
        if (unlockPressedAt is not long startedAt)
        {
            return;
        }

        unlockPressedAt = null;
        if (viewModel.TryUnlock(Stopwatch.GetElapsedTime(startedAt)))
        {
            await Shell.Current.GoToAsync(AppRoutes.Back);
        }
    }

    private async void OnAccessibleExitClicked(object? sender, EventArgs args)
    {
        if (!await DisplayAlertAsync(AppText.Get("ExitStageTitle"), AppText.Get("ExitStageDetail"), AppText.Continue, AppText.Cancel) ||
            !await DisplayAlertAsync(AppText.Get("ConfirmExit"), AppText.Get("NoReplayExitDetail"), AppText.Get("ExitStage"), AppText.Get("Stay")))
        {
            return;
        }

        if (viewModel.TryUnlock(TimeSpan.FromSeconds(2)))
        {
            await Shell.Current.GoToAsync(AppRoutes.Back);
        }
    }

    private async void OnPageSwiped(object? sender, SwipedEventArgs args)
    {
        if (args.Direction == SwipeDirection.Left)
        {
            await viewModel.NextPageCommand.ExecuteAsync();
        }
        else if (args.Direction == SwipeDirection.Right)
        {
            await viewModel.PreviousPageCommand.ExecuteAsync();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(StageModeViewModel.LayoutMode)
            or nameof(StageModeViewModel.GridSpan))
        {
            Dispatcher.Dispatch(UpdateItemsLayout);
        }
    }

    private void UpdateItemsLayout()
    {
        StageTilesView.ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
        {
            Span = viewModel.GridSpan,
            HorizontalItemSpacing = 6,
            VerticalItemSpacing = 6
        };
    }

    private async void OnReconnectClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.DevicePicker);

    private async void OnOpenDeviceClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(services.GetRequiredService<DevicePage>());
}
