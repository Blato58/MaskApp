using MaskApp.Core.Features.AnimationPacks;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App.Features.AnimationPacks;

public partial class MaskPackPage : ContentPage, IQueryAttributable
{
    private readonly MaskPackViewModel viewModel;
    private bool initialized;
    private string mode = "import";

    public MaskPackPage(MaskPackViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var value))
        {
            mode = Uri.UnescapeDataString(value?.ToString() ?? "import");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (initialized)
        {
            return;
        }

        initialized = true;
        await viewModel.InitializeAsync();
        if (string.Equals(mode, "export", StringComparison.OrdinalIgnoreCase))
        {
            await TransferScroll.ScrollToAsync(ExportSection, ScrollToPosition.Start, false);
        }
    }

    private async void OnChoosePackClicked(object? sender, EventArgs e)
    {
        try
        {
            var selected = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = AppText.Get("ChooseMaskPackArchive")
            });
            if (selected is null)
            {
                return;
            }

            await using var stream = await selected.OpenReadAsync();
            await viewModel.InspectAsync(stream);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidDataException or InvalidOperationException)
        {
            await DisplayAlertAsync(AppText.Get("MaskPackInspectionFailed"), ShortMessage(exception), AppText.Get("Ok"));
        }
    }

    private async void OnExportPackClicked(object? sender, EventArgs e)
    {
        string? exportPath = null;
        try
        {
            var directory = Path.Combine(FileSystem.CacheDirectory, "MaskPacks");
            Directory.CreateDirectory(directory);
            exportPath = Path.Combine(directory, $"{SafeFileName(viewModel.PackName)}.maskpack.zip");
            MaskPackExportResult result;
            await using (var stream = File.Create(exportPath))
            {
                result = await viewModel.ExportAsync(stream);
            }

            if (!result.Succeeded)
            {
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }

                await DisplayAlertAsync(AppText.Get("MaskPackExportFailed"), result.Message, AppText.Get("Ok"));
                return;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = AppText.Get("ShareMaskPack"),
                File = new ShareFile(exportPath)
            });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or NotSupportedException)
        {
            await DisplayAlertAsync(AppText.Get("MaskPackExportFailed"), ShortMessage(exception), AppText.Get("Ok"));
        }
    }

    private static string SafeFileName(string source)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var safe = new string((source ?? string.Empty)
            .Trim()
            .Where(character => !invalid.Contains(character))
            .Take(80)
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "MaskApp-Show" : safe;
    }

    private static string ShortMessage(Exception exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;
        return message.Length <= 180 ? message : string.Concat(message.AsSpan(0, 180), "...");
    }
}
