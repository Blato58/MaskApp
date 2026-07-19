using MaskApp.App.Resources.Strings;
using System.ComponentModel;
using System.Diagnostics;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Experience;
using Microsoft.Maui.Storage;

namespace MaskApp.App.Features.Animations;

public partial class AnimationStudioPage : ContentPage, IQueryAttributable
{
    private readonly AnimationStudioViewModel viewModel;
    private readonly AnimationFrameGridDrawable drawable;
    private int? draggedFrameIndex;
    private bool initialized;
    private string pendingProjectId = string.Empty;

    public AnimationStudioPage(AnimationStudioViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        drawable = new AnimationFrameGridDrawable(viewModel);
        FrameCanvas.Drawable = drawable;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!initialized)
        {
            initialized = true;
            await viewModel.InitializeAsync();
        }

        if (!string.IsNullOrWhiteSpace(pendingProjectId))
        {
            viewModel.SelectProject(pendingProjectId);
            pendingProjectId = string.Empty;
        }

        FrameCanvas.Invalidate();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        pendingProjectId = query.TryGetValue("projectId", out var value)
            ? Uri.UnescapeDataString(value?.ToString() ?? string.Empty)
            : string.Empty;
        if (initialized && !string.IsNullOrWhiteSpace(pendingProjectId))
        {
            viewModel.SelectProject(pendingProjectId);
            pendingProjectId = string.Empty;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AnimationStudioViewModel.PreviewCells)
            or nameof(AnimationStudioViewModel.OnionSkinPattern)
            or nameof(AnimationStudioViewModel.SelectedFrameIndex)
            or nameof(AnimationStudioViewModel.GuidesEnabled)
            or nameof(AnimationStudioViewModel.SelectionBounds))
        {
            MainThread.BeginInvokeOnMainThread(FrameCanvas.Invalidate);
        }
    }

    private void OnCanvasStartInteraction(object? sender, TouchEventArgs e)
    {
        if (e.Touches.Length > 0 && drawable.TryGetCell(e.Touches[0], out var column, out var row))
        {
            viewModel.BeginCanvasInteraction(column, row);
            FrameCanvas.Invalidate();
        }
    }

    private void OnCanvasDragInteraction(object? sender, TouchEventArgs e)
    {
        if (e.Touches.Length > 0 && drawable.TryGetCell(e.Touches[0], out var column, out var row))
        {
            viewModel.ContinueCanvasInteraction(column, row);
            FrameCanvas.Invalidate();
        }
    }

    private void OnCanvasEndInteraction(object? sender, TouchEventArgs e)
    {
        viewModel.EndCanvasInteraction();
        FrameCanvas.Invalidate();
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync(AppRoutes.Back);

    private void OnProjectSelected(object? sender, EventArgs e)
    {
        if (ProjectPicker.SelectedItem is AnimationProject project)
        {
            viewModel.SelectProject(project.Id);
        }
    }

    private void OnColorClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string colorName })
        {
            viewModel.SelectColor(colorName);
        }
    }

    private void OnClearFrameClicked(object? sender, EventArgs e) => viewModel.ClearSelectedFrame();

    private void OnMirrorFrameClicked(object? sender, EventArgs e) => viewModel.MirrorSelectedFrameHorizontally();

    private void OnInsertFrameClicked(object? sender, EventArgs e) => viewModel.InsertBlankFrame();

    private void OnDuplicateFrameClicked(object? sender, EventArgs e) => viewModel.DuplicateSelectedFrame();

    private void OnDeleteFrameClicked(object? sender, EventArgs e) => viewModel.DeleteSelectedFrame();

    private void OnMoveEarlierClicked(object? sender, EventArgs e) => viewModel.MoveSelectedFrame(-1);

    private void OnMoveLaterClicked(object? sender, EventArgs e) => viewModel.MoveSelectedFrame(1);

    private void OnFrameTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is AnimationTimelineFrame frame)
        {
            viewModel.SelectFrame(frame.Index);
        }
    }

    private void OnFrameDragStarting(object? sender, DragStartingEventArgs e)
    {
        draggedFrameIndex = sender is DragGestureRecognizer { BindingContext: AnimationTimelineFrame frame }
            ? frame.Index
            : null;
        e.Cancel = draggedFrameIndex is null;
    }

    private void OnFrameDropped(object? sender, DropEventArgs e)
    {
        if (draggedFrameIndex is int from &&
            sender is DropGestureRecognizer { BindingContext: AnimationTimelineFrame frame })
        {
            viewModel.MoveFrame(from, frame.Index);
        }

        draggedFrameIndex = null;
    }

    private async void OnImportGifClicked(object? sender, EventArgs e) =>
        await ImportAsync(AnimationMediaKind.Gif);

    private async void OnImportVideoClicked(object? sender, EventArgs e) =>
        await ImportAsync(AnimationMediaKind.Video);

    private async Task ImportAsync(AnimationMediaKind kind)
    {
        try
        {
            var fileTypes = kind == AnimationMediaKind.Gif
                ? new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.iOS] = ["com.compuserve.gif", "image/gif"],
                    [DevicePlatform.Android] = ["image/gif"]
                })
                : FilePickerFileType.Videos;
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = kind == AnimationMediaKind.Gif ? AppText.Get("Ui402") : AppText.Get("Ui403"),
                FileTypes = fileTypes
            });
            if (file is null)
            {
                return;
            }

            await using var stream = await file.OpenReadAsync();
            var name = Path.GetFileNameWithoutExtension(file.FileName);
            var result = await viewModel.ImportMediaAsync(
                stream,
                name,
                kind,
                viewModel.BuildImportOptions(),
                TimeSpan.FromMilliseconds(viewModel.ImportSampleMilliseconds));
            if (!result.Succeeded)
            {
                await DisplayAlertAsync(AppText.Get("Ui404"), result.Message, AppText.Get("Ui390"));
            }
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync(AppText.Get("Ui395"), ShortMessage(exception), AppText.Get("Ui390"));
        }
    }

    private void OnTapTempoClicked(object? sender, EventArgs e)
    {
        viewModel.AddTap(TimeSpan.FromSeconds(
            Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency));
    }

    private static string ShortMessage(Exception exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;
        return message.Length <= 160 ? message : string.Concat(message.AsSpan(0, 160), "...");
    }
}
