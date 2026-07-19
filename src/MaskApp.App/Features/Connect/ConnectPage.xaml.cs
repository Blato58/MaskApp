using MaskApp.App.Resources.Strings;
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
        AdvancedToggle.Text = AdvancedPanel.IsVisible ? AppText.Get("Ui387") : AppText.Get("Ui313");
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
                AppText.Get("Ui388"),
                new ShareFile(path)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            await DisplayAlertAsync(AppText.Get("Ui389"), ex.Message, AppText.Get("Ui390"));
        }
    }

    private async void OnResetPreparedSlotsClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            AppText.Get("Ui391"),
            AppText.Get("Ui392"),
            AppText.Get("Ui393"),
            AppText.Get("Ui056"));
        if (confirmed)
        {
            await viewModel.ResetPreparedSlotsAsync();
        }
    }
}
