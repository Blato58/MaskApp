using MaskApp.Core.Features.AnimationPacks;

namespace MaskApp.App.Features.AnimationPacks;

public partial class MaskPackPage : ContentPage
{
    private readonly MaskPackViewModel viewModel;
    private bool initialized;

    public MaskPackPage(MaskPackViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
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
    }

    private async void OnChoosePackClicked(object? sender, EventArgs e)
    {
        try
        {
            var selected = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose a MaskPack ZIP archive"
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
            await DisplayAlertAsync("MaskPack inspection failed", ShortMessage(exception), "OK");
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

                await DisplayAlertAsync("MaskPack export failed", result.Message, "OK");
                return;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Share MaskPack",
                File = new ShareFile(exportPath)
            });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or NotSupportedException)
        {
            await DisplayAlertAsync("MaskPack export failed", ShortMessage(exception), "OK");
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
