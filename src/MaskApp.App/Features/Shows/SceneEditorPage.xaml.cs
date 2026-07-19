using MaskApp.Core.Features.Scenes;

namespace MaskApp.App.Features.Shows;

public partial class SceneEditorPage : ContentPage, IQueryAttributable
{
    private readonly SceneStudioViewModel viewModel;
    private string sceneId = string.Empty;

    public SceneEditorPage(SceneStudioViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("sceneId", out var value))
        {
            sceneId = Uri.UnescapeDataString(value?.ToString() ?? string.Empty);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        if (!string.IsNullOrWhiteSpace(sceneId))
        {
            viewModel.SelectScene(sceneId);
        }
    }

    private void OnStepSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is SceneStepRow row)
        {
            viewModel.SelectedStepIndex = row.Index;
        }
    }

    private void OnAddStep(object? sender, EventArgs e) => viewModel.AddStep();
    private void OnMoveStepEarlier(object? sender, EventArgs e) => viewModel.MoveSelectedStep(-1);
    private void OnMoveStepLater(object? sender, EventArgs e) => viewModel.MoveSelectedStep(1);
    private void OnRemoveStep(object? sender, EventArgs e) => viewModel.DeleteSelectedStep();
    private void OnReplaceStep(object? sender, EventArgs e) => viewModel.ReplaceSelectedStepContent();
}
