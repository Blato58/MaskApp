using System.ComponentModel;
using MaskApp.App.Infrastructure.Media;
using MaskApp.Core.Features.Faces;
using Microsoft.Maui.Media;

namespace MaskApp.App.Features.Faces;

public partial class FaceStudioPage : ContentPage
{
    private readonly FaceStudioViewModel viewModel;
    private readonly IFaceImageDecoder imageDecoder;
    private readonly FaceGridDrawable drawable;

    public FaceStudioPage(FaceStudioViewModel viewModel, IFaceImageDecoder imageDecoder)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.imageDecoder = imageDecoder;
        drawable = new FaceGridDrawable(viewModel);
        FaceCanvas.Drawable = drawable;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        FaceCanvas.Invalidate();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FaceStudioViewModel.PreviewCells))
        {
            MainThread.BeginInvokeOnMainThread(FaceCanvas.Invalidate);
        }
    }

    private void OnCanvasInteraction(object? sender, TouchEventArgs e)
    {
        if (e.Touches.Length == 0)
        {
            return;
        }

        if (drawable.TryGetCell(e.Touches[0], out var column, out var row))
        {
            viewModel.SetCell(column, row);
            FaceCanvas.Invalidate();
        }
    }

    private void OnColorClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string colorName })
        {
            viewModel.SelectColor(colorName);
        }
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                SelectionLimit = 1,
                MaximumWidth = 720,
                MaximumHeight = 240,
                CompressionQuality = 90,
                RotateImage = true,
                PreserveMetaData = false,
                Title = "Choose a face image"
            });
            var file = results.FirstOrDefault();
            if (file is null)
            {
                return;
            }

            await ImportFileAsync(file, FacePatternSource.ImportedPhoto);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Import failed", GetShortErrorMessage(ex), "OK");
        }
    }

    private async void OnCapturePhotoClicked(object? sender, EventArgs e)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            await DisplayAlertAsync("Camera unavailable", "This device does not report camera capture support.", "OK");
            return;
        }

        try
        {
            var file = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                MaximumWidth = 720,
                MaximumHeight = 240,
                CompressionQuality = 90,
                RotateImage = true,
                PreserveMetaData = false,
                Title = "Capture a face image"
            });
            if (file is null)
            {
                return;
            }

            await ImportFileAsync(file, FacePatternSource.CapturedPhoto);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Camera failed", GetShortErrorMessage(ex), "OK");
        }
    }

    private async Task ImportFileAsync(FileResult file, FacePatternSource source)
    {
        await using var stream = await file.OpenReadAsync();
        var image = await imageDecoder.DecodeAsync(stream);
        if (image is null)
        {
            await DisplayAlertAsync("Import unavailable", "Image decoding is not available on this platform build.", "OK");
            return;
        }

        var name = Path.GetFileNameWithoutExtension(file.FileName);
        viewModel.ImportImage(image, source, name);
        FaceCanvas.Invalidate();
    }

    private static string GetShortErrorMessage(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message)
            ? ex.GetType().Name
            : ex.Message;
        return message.Length <= 120 ? message : string.Concat(message.AsSpan(0, 120), "...");
    }

    private void OnToggleDiagnosticsClicked(object? sender, EventArgs e)
    {
        DiagnosticsPanel.IsVisible = !DiagnosticsPanel.IsVisible;
        DiagnosticsToggle.Text = DiagnosticsPanel.IsVisible ? "Hide diagnostics" : "Show diagnostics";
    }
}
