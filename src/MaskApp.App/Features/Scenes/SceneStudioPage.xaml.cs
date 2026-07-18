using MaskApp.Core.Features.Scenes;

namespace MaskApp.App.Features.Scenes;

public partial class SceneStudioPage : ContentPage, IQueryAttributable
{
    private readonly SceneStudioViewModel viewModel;
    private bool initialized;
    private string pendingSceneId = string.Empty;
    private string pendingSetlistId = string.Empty;
    private int? draggedStepIndex;
    private int? draggedCueIndex;

    public SceneStudioPage(SceneStudioViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!initialized)
        {
            initialized = true;
            await viewModel.InitializeAsync();
        }

        ApplyPendingSelection();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        pendingSceneId = ReadQuery(query, "sceneId");
        pendingSetlistId = ReadQuery(query, "setlistId");
        if (initialized)
        {
            ApplyPendingSelection();
        }
    }

    private void ApplyPendingSelection()
    {
        if (!string.IsNullOrWhiteSpace(pendingSceneId))
        {
            viewModel.SelectScene(pendingSceneId);
            pendingSceneId = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(pendingSetlistId))
        {
            viewModel.SelectSetlist(pendingSetlistId);
            pendingSetlistId = string.Empty;
        }
    }

    private void OnSceneSelected(object? sender, EventArgs e)
    {
        if (ScenePicker.SelectedItem is PerformanceScene scene)
        {
            viewModel.SelectScene(scene.Id);
        }
    }

    private void OnSetlistSelected(object? sender, EventArgs e)
    {
        if (SetlistPicker.SelectedItem is PerformanceSetlist setlist)
        {
            viewModel.SelectSetlist(setlist.Id);
        }
    }

    private void OnAddStepClicked(object? sender, EventArgs e) => viewModel.AddStep();

    private void OnDuplicateStepClicked(object? sender, EventArgs e) => viewModel.DuplicateSelectedStep();

    private void OnDeleteStepClicked(object? sender, EventArgs e) => viewModel.DeleteSelectedStep();

    private void OnMoveStepEarlierClicked(object? sender, EventArgs e) => viewModel.MoveSelectedStep(-1);

    private void OnMoveStepLaterClicked(object? sender, EventArgs e) => viewModel.MoveSelectedStep(1);

    private void OnStepTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is SceneStepRow row)
        {
            viewModel.SelectedStepIndex = row.Index;
        }
    }

    private void OnStepDragStarting(object? sender, DragStartingEventArgs e)
    {
        draggedStepIndex = sender is DragGestureRecognizer { BindingContext: SceneStepRow row }
            ? row.Index
            : null;
        e.Cancel = draggedStepIndex is null;
    }

    private void OnStepDropped(object? sender, DropEventArgs e)
    {
        if (draggedStepIndex is int from
            && sender is DropGestureRecognizer { BindingContext: SceneStepRow row })
        {
            viewModel.MoveStep(from, row.Index);
        }

        draggedStepIndex = null;
    }

    private void OnAddCueClicked(object? sender, EventArgs e) => viewModel.AddCue();

    private void OnDuplicateCueClicked(object? sender, EventArgs e) => viewModel.DuplicateSelectedCue();

    private void OnDeleteCueClicked(object? sender, EventArgs e) => viewModel.DeleteSelectedCue();

    private void OnMoveCueEarlierClicked(object? sender, EventArgs e) => viewModel.MoveSelectedCue(-1);

    private void OnMoveCueLaterClicked(object? sender, EventArgs e) => viewModel.MoveSelectedCue(1);

    private void OnCueTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is SetlistCueRow row)
        {
            viewModel.SelectedCueIndex = row.Index;
        }
    }

    private void OnCueDragStarting(object? sender, DragStartingEventArgs e)
    {
        draggedCueIndex = sender is DragGestureRecognizer { BindingContext: SetlistCueRow row }
            ? row.Index
            : null;
        e.Cancel = draggedCueIndex is null;
    }

    private void OnCueDropped(object? sender, DropEventArgs e)
    {
        if (draggedCueIndex is int from
            && sender is DropGestureRecognizer { BindingContext: SetlistCueRow row })
        {
            viewModel.MoveCue(from, row.Index);
        }

        draggedCueIndex = null;
    }

    private static string ReadQuery(IDictionary<string, object> query, string key) =>
        query.TryGetValue(key, out var value)
            ? Uri.UnescapeDataString(value?.ToString() ?? string.Empty)
            : string.Empty;
}
