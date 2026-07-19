using MaskApp.Core.Features.Connect;
using MaskApp.App.Resources.Strings;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace MaskApp.App.Features.Device;

public partial class DiagnosticsPage : ContentPage
{
    private readonly ConnectViewModel viewModel;

    public DiagnosticsPage(ConnectViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RefreshDiagnosticsAsync();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e) => await viewModel.RefreshDiagnosticsAsync();

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        try
        {
            var report = await viewModel.BuildRedactedDiagnosticsReportAsync();
            var path = Path.Combine(FileSystem.CacheDirectory, $"maskapp-diagnostics-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.txt");
            await File.WriteAllTextAsync(path, report);
            await Share.Default.RequestAsync(new ShareFileRequest(AppText.Get("ShareDiagnostics"), new ShareFile(path)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            await DisplayAlertAsync(AppText.Get("ExportFailed"), ex.Message, AppText.Get("Ok"));
        }
    }

    private async void OnResetClicked(object? sender, EventArgs e)
    {
        if (await DisplayAlertAsync(
            AppText.Get("ResetLedgerTitle"),
            AppText.Get("ResetLedgerDetail"),
            AppText.Get("ResetLedger"),
            AppText.Cancel))
        {
            await viewModel.ResetPreparedSlotsAsync();
        }
    }
}
