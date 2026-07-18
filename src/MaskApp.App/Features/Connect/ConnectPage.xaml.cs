using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace MaskApp.App.Features.Connect;

public partial class ConnectPage : ContentPage
{
    private readonly ConnectViewModel viewModel;

    public ConnectPage(ConnectViewModel viewModel, MaskControlViewModel maskControlViewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        MaskControls.BindingContext = maskControlViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
    }

    private void OnToggleAdvancedClicked(object? sender, EventArgs e)
    {
        AdvancedPanel.IsVisible = !AdvancedPanel.IsVisible;
        AdvancedToggle.Text = AdvancedPanel.IsVisible ? "Hide diagnostics" : "Show diagnostics";
        if (AdvancedPanel.IsVisible)
        {
            _ = viewModel.RefreshDiagnosticsAsync();
        }
    }

    private async void OnRefreshDiagnosticsClicked(object? sender, EventArgs e) =>
        await viewModel.RefreshDiagnosticsAsync();

    private async void OnExportDiagnosticsClicked(object? sender, EventArgs e)
    {
        try
        {
            var report = await viewModel.BuildRedactedDiagnosticsReportAsync();
            var path = Path.Combine(
                FileSystem.CacheDirectory,
                $"maskapp-diagnostics-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.txt");
            await File.WriteAllTextAsync(path, report);
            await Share.Default.RequestAsync(new ShareFileRequest(
                "Share redacted MaskApp diagnostics",
                new ShareFile(path)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            await DisplayAlertAsync("Export failed", ex.Message, "OK");
        }
    }

    private async void OnResetPreparedSlotsClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            "Reset this mask's prepared-slot ledger?",
            "This clears only MaskApp's prepared-content record for the active mask. It does not erase physical mask slots, other masks, presets, scenes, or animations.",
            "Reset active ledger",
            "Cancel");
        if (confirmed)
        {
            await viewModel.ResetPreparedSlotsAsync();
        }
    }
}
