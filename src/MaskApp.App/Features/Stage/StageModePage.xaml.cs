using System.ComponentModel;
using System.Diagnostics;
using MaskApp.Core.Features.Stage;

namespace MaskApp.App.Features.Stage;

public partial class StageModePage : ContentPage
{
    private readonly StageModeViewModel viewModel;
    private long? unlockPressedAt;

    public StageModePage(StageModeViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public StageModeViewModel ViewModel => viewModel;

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
            await Shell.Current.GoToAsync("..");
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
}
