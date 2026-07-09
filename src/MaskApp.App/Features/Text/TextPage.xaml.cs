using MaskApp.Core.Features.Text;

namespace MaskApp.App.Features.Text;

public partial class TextPage : ContentPage, IQueryAttributable
{
    private string? pendingPresetId;

    public TextPage(TextUploadViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("presetId", out var value))
        {
            pendingPresetId = Uri.UnescapeDataString(value?.ToString() ?? string.Empty);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TextUploadViewModel viewModel)
        {
            await viewModel.InitializeAsync();
            if (!string.IsNullOrWhiteSpace(pendingPresetId))
            {
                await viewModel.OpenPresetByIdAsync(pendingPresetId);
                pendingPresetId = null;
            }
        }
    }

    private void OnColorClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not TextUploadViewModel viewModel ||
            sender is not Button { CommandParameter: string colorName })
        {
            return;
        }

        var color = viewModel.TextColorOptions.FirstOrDefault(option => option.Name == colorName);
        if (color is not null)
        {
            viewModel.SelectColor(color);
        }
    }

    private void OnToggleDiagnosticsClicked(object? sender, EventArgs e)
    {
        DiagnosticsPanel.IsVisible = !DiagnosticsPanel.IsVisible;
        DiagnosticsToggle.Text = DiagnosticsPanel.IsVisible ? "Hide diagnostics" : "Show diagnostics";
    }
}
