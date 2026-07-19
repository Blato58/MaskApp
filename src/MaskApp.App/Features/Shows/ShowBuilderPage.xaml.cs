using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.App.Features.Shows;

public partial class ShowBuilderPage : ContentPage, IQueryAttributable
{
    private readonly SceneStudioViewModel viewModel;
    private string showId = string.Empty;

    public ShowBuilderPage(SceneStudioViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("showId", out var value))
        {
            showId = Uri.UnescapeDataString(value?.ToString() ?? string.Empty);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        if (!string.IsNullOrWhiteSpace(showId))
        {
            viewModel.SelectSetlist(showId);
        }
    }

    private void OnCueSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is SetlistCueRow row)
        {
            viewModel.SelectedCueIndex = row.Index;
        }
    }

    private void OnAddCue(object? sender, EventArgs e) => viewModel.AddCue();
    private void OnMoveCueEarlier(object? sender, EventArgs e) => viewModel.MoveSelectedCue(-1);
    private void OnMoveCueLater(object? sender, EventArgs e) => viewModel.MoveSelectedCue(1);
    private void OnRemoveCue(object? sender, EventArgs e) => viewModel.DeleteSelectedCue();

    private async void OnPreflightClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.ForPreflight("active-show", viewModel.CurrentSetlist.Id));
}
