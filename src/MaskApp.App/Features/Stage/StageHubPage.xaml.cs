using MaskApp.App.Resources.Strings;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.Stage;
using MaskApp.Core.Navigation;

namespace MaskApp.App.Features.Stage;

public partial class StageHubPage : ContentPage
{
    private readonly StageHubViewModel viewModel;
    private int? draggedCueIndex;

    public StageHubPage(StageHubViewModel viewModel)
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

    private void OnBuildClicked(object? sender, EventArgs e) => viewModel.ShowBuildCommand.Execute(null);

    private void OnPreflightClicked(object? sender, EventArgs e)
    {
        viewModel.ShowPreflightCommand.Execute(null);
        viewModel.Preflight.RunWholeShowCommand.Execute(null);
    }

    private async void OnPagesClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("pages-manage");

    private async void OnDeviceClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRouteCatalog.AbsoluteRoot(AppRouteCatalog.DeviceRoot));

    private async void OnShowBuilderClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("scene-studio");

    private async void OnAudioLabsClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("audio-labs");

    private void OnAddCueClicked(object? sender, EventArgs e)
    {
        if (viewModel.SceneStudio.AddCue())
        {
            viewModel.SceneStudio.SaveSetlistCommand.Execute(null);
        }
    }

    private void OnCueEarlierClicked(object? sender, EventArgs e) => MoveCue(sender, -1);

    private void OnCueLaterClicked(object? sender, EventArgs e) => MoveCue(sender, 1);

    private void OnCueDragStarting(object? sender, DragStartingEventArgs e)
    {
        draggedCueIndex = sender is DragGestureRecognizer { BindingContext: SetlistCueRow row }
            ? row.Index
            : null;
        e.Cancel = draggedCueIndex is null;
    }

    private void OnCueDropped(object? sender, DropEventArgs e)
    {
        if (draggedCueIndex is int fromIndex
            && sender is DropGestureRecognizer { BindingContext: SetlistCueRow row }
            && viewModel.SceneStudio.MoveCue(fromIndex, row.Index))
        {
            viewModel.SceneStudio.SaveSetlistCommand.Execute(null);
        }

        draggedCueIndex = null;
    }

    private void MoveCue(object? sender, int offset)
    {
        if (sender is Button { CommandParameter: SetlistCueRow row })
        {
            viewModel.SceneStudio.SelectedCueIndex = row.Index;
            if (viewModel.SceneStudio.MoveSelectedCue(offset))
            {
                viewModel.SceneStudio.SaveSetlistCommand.Execute(null);
            }
        }
    }

    private void OnCueRemoveClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: SetlistCueRow row })
        {
            viewModel.SceneStudio.SelectedCueIndex = row.Index;
            if (viewModel.SceneStudio.DeleteSelectedCue())
            {
                viewModel.SceneStudio.SaveSetlistCommand.Execute(null);
            }
        }
    }

    private async void OnEnterStageClicked(object? sender, EventArgs e)
    {
        if (viewModel.Preflight.CurrentReport?.Status == FestivalPreflightStatus.Degraded)
        {
            var proceed = await DisplayAlertAsync(
                AppText.Get("Ui381"),
                AppText.Get("Ui382"),
                AppText.Get("Ui383"),
                AppText.Get("Ui384"));
            if (!proceed)
            {
                return;
            }
        }

        await Shell.Current.GoToAsync("stage");
    }

    private async void OnAcknowledgeFlashRiskClicked(object? sender, EventArgs e)
    {
        var accepted = await DisplayAlertAsync(
            AppText.Get("Ui374"),
            AppText.Get("Ui385"),
            AppText.Get("Ui376"),
            AppText.Get("Ui056"));
        if (accepted)
        {
            await viewModel.Preflight.AcknowledgeBlockedFlashRiskAsync();
        }
    }

    private async void OnRevokeFlashRiskClicked(object? sender, EventArgs e)
    {
        var revoke = await DisplayAlertAsync(
            AppText.Get("Ui377"),
            AppText.Get("Ui386"),
            AppText.Get("Ui379"),
            AppText.Get("Ui380"));
        if (revoke)
        {
            await viewModel.Preflight.RevokeFlashRiskOverridesAsync();
        }
    }
}
