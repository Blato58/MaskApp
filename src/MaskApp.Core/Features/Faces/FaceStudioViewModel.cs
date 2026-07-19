using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Faces;

public enum FaceEditorTool
{
    Draw,
    Fill,
    SelectMove
}

public sealed record FaceSelectionBounds(int Left, int Top, int Right, int Bottom);

public sealed class FaceStudioViewModel : INotifyPropertyChanged
{
    private const string OffColor = "#05070D";
    private const int MaxDiagnosticHexLength = 1024;
    private const int MaxHistoryDepth = 50;

    private readonly IFacePatternStore store;
    private readonly IFaceUploadTransport transport;
    private readonly Stack<EditorSnapshot> undoHistory = new();
    private readonly Stack<EditorSnapshot> redoHistory = new();
    private FacePatternStoreState storeState = new();
    private FacePattern currentPattern = FacePatternFactory.CreateBuiltIns()[0];
    private IReadOnlyList<FacePattern> patterns = [];
    private IReadOnlyList<FacePatternCard> patternCards = [];
    private IReadOnlyList<FacePreviewCell> previewCells = [];
    private IReadOnlyList<FaceSavedPalette> savedPalettes = [];
    private IReadOnlyList<FaceColorOption> activeColorOptions = FaceColorOption.Defaults;
    private FaceSavedPalette? selectedPalette;
    private FaceColorOption selectedColor;
    private string faceName = "Happy Smiley";
    private string paletteName = "Palette 1";
    private string paletteStatusText = "Use colors from this face to save a reusable palette.";
    private int selectedSlot = 1;
    private bool isEraseMode;
    private FaceEditorTool selectedTool;
    private bool isHorizontalSymmetryEnabled;
    private bool isVerticalSymmetryEnabled;
    private HashSet<int> selectedCellIndices = [];
    private FaceSelectionBounds? selectionBounds;
    private EditorSnapshot? editTransactionStart;
    private (int Column, int Row)? lastInteractionCell;
    private (int Column, int Row)? selectionDragStart;
    private FacePattern? selectionDragPattern;
    private int[] selectionDragIndices = [];
    private bool isSending;
    private string statusText;
    private string storeStatusText = "Face library ready.";
    private string lastPayloadHex = "None";
    private string lastCommandText = "None";
    private int frameCount;
    private bool useCompatibilityWriteOnly;
    private bool supportsAcknowledgements;
    private FaceUploadTransportState transportState;

    public FaceStudioViewModel(
        IFacePatternStore store,
        IFaceUploadTransport transport)
    {
        this.store = store;
        this.transport = transport;
        selectedColor = FaceColorOptions[0];
        supportsAcknowledgements = transport.SupportsAcknowledgements;
        transportState = transport.State;
        useCompatibilityWriteOnly = ShouldDefaultToCompatibilityMode(transport.State, transport.SupportsAcknowledgements);
        statusText = transport.StatusText;
        NewBlankCommand = new AsyncRelayCommand(NewBlankAsync);
        ClearCommand = new AsyncRelayCommand(_ =>
        {
            Clear();
            return Task.CompletedTask;
        });
        MirrorCommand = new AsyncRelayCommand(_ =>
        {
            MirrorHorizontally();
            return Task.CompletedTask;
        });
        MirrorVerticalCommand = new AsyncRelayCommand(_ =>
        {
            MirrorVertically();
            return Task.CompletedTask;
        });
        UndoCommand = new AsyncRelayCommand(_ =>
        {
            Undo();
            return Task.CompletedTask;
        }, () => CanUndo);
        RedoCommand = new AsyncRelayCommand(_ =>
        {
            Redo();
            return Task.CompletedTask;
        }, () => CanRedo);
        SelectDrawToolCommand = CreateToolCommand(FaceEditorTool.Draw);
        SelectFillToolCommand = CreateToolCommand(FaceEditorTool.Fill);
        SelectMoveToolCommand = CreateToolCommand(FaceEditorTool.SelectMove);
        NewPaletteCommand = new AsyncRelayCommand(_ =>
        {
            SelectedPalette = null;
            PaletteName = $"Palette {SavedPalettes.Count + 1}";
            PaletteStatusText = "Enter a name, then save the colors used by this face.";
            return Task.CompletedTask;
        });
        SavePaletteCommand = new AsyncRelayCommand(SavePaletteAsync, () => !string.IsNullOrWhiteSpace(PaletteName));
        DeletePaletteCommand = new AsyncRelayCommand(DeletePaletteAsync, () => SelectedPalette is not null);
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        SaveAsCopyCommand = new AsyncRelayCommand(SaveAsCopyAsync, CanSave);
        UploadCommand = new AsyncRelayCommand(UploadAsync, CanUpload);
        DeleteCommand = new AsyncRelayCommand(DeleteCurrentAsync, () => !CurrentPattern.IsBuiltIn);
        transport.StateChanged += OnTransportStateChanged;
        ApplyPattern(currentPattern);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<FaceColorOption> FaceColorOptions => FaceColorOption.Defaults;

    public AsyncRelayCommand NewBlankCommand { get; }

    public AsyncRelayCommand ClearCommand { get; }

    public AsyncRelayCommand MirrorCommand { get; }

    public AsyncRelayCommand MirrorVerticalCommand { get; }

    public AsyncRelayCommand UndoCommand { get; }

    public AsyncRelayCommand RedoCommand { get; }

    public AsyncRelayCommand SelectDrawToolCommand { get; }

    public AsyncRelayCommand SelectFillToolCommand { get; }

    public AsyncRelayCommand SelectMoveToolCommand { get; }

    public AsyncRelayCommand NewPaletteCommand { get; }

    public AsyncRelayCommand SavePaletteCommand { get; }

    public AsyncRelayCommand DeletePaletteCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand SaveAsCopyCommand { get; }

    public AsyncRelayCommand UploadCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }

    public IReadOnlyList<FacePattern> Patterns
    {
        get => patterns;
        private set => SetField(ref patterns, value);
    }

    public IReadOnlyList<FacePatternCard> PatternCards
    {
        get => patternCards;
        private set => SetField(ref patternCards, value);
    }

    public IReadOnlyList<FacePreviewCell> PreviewCells
    {
        get => previewCells;
        private set => SetField(ref previewCells, value);
    }

    public IReadOnlyList<FaceSavedPalette> SavedPalettes
    {
        get => savedPalettes;
        private set => SetField(ref savedPalettes, value);
    }

    public IReadOnlyList<FaceColorOption> ActiveColorOptions
    {
        get => activeColorOptions;
        private set => SetField(ref activeColorOptions, value);
    }

    public FaceSavedPalette? SelectedPalette
    {
        get => selectedPalette;
        set
        {
            if (!SetField(ref selectedPalette, value))
            {
                return;
            }

            PaletteName = value?.DisplayName ?? PaletteName;
            ActiveColorOptions = value is null
                ? FaceColorOption.Defaults
                : value.Colors
                    .Select((color, index) => new FaceColorOption(GetColorName(color, index), color))
                    .ToArray();
            SelectedColor = ActiveColorOptions.FirstOrDefault() ?? FaceColorOption.Defaults[0];
            PaletteStatusText = value is null
                ? "Default drawing colors active."
                : $"{value.DisplayName} active with {value.Colors.Length} color(s).";
            DeletePaletteCommand.RaiseCanExecuteChanged();
        }
    }

    public string PaletteName
    {
        get => paletteName;
        set
        {
            if (SetField(ref paletteName, value ?? string.Empty))
            {
                SavePaletteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string PaletteStatusText
    {
        get => paletteStatusText;
        private set => SetField(ref paletteStatusText, value);
    }

    public FacePattern CurrentPattern
    {
        get => currentPattern;
        private set
        {
            if (SetField(ref currentPattern, value.Normalize()))
            {
                OnPropertyChanged(nameof(CurrentPatternSubtitle));
                OnPropertyChanged(nameof(IsEditingBuiltIn));
                OnPropertyChanged(nameof(DeleteButtonText));
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string FaceName
    {
        get => faceName;
        set
        {
            if (SetField(ref faceName, value ?? string.Empty))
            {
                SaveCommand.RaiseCanExecuteChanged();
                SaveAsCopyCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int SelectedSlot
    {
        get => selectedSlot;
        set
        {
            if (SetField(ref selectedSlot, Math.Clamp(value, FacePattern.MinSlot, FacePattern.MaxSlot)))
            {
                OnPropertyChanged(nameof(SlotSummaryText));
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SlotSummaryText => $"DIY slot {SelectedSlot}/20";

    public FaceColorOption SelectedColor
    {
        get => selectedColor;
        set
        {
            if (value is not null && SetField(ref selectedColor, value))
            {
                OnPropertyChanged(nameof(SelectedColorSummary));
            }
        }
    }

    public string SelectedColorSummary => $"{SelectedColor.Name} {SelectedColor.Hex}";

    public bool IsEraseMode
    {
        get => isEraseMode;
        set
        {
            if (SetField(ref isEraseMode, value))
            {
                OnPropertyChanged(nameof(DrawModeText));
            }
        }
    }

    public string DrawModeText => IsEraseMode ? "Erase" : "Paint";

    public FaceEditorTool SelectedTool
    {
        get => selectedTool;
        private set
        {
            if (SetField(ref selectedTool, value))
            {
                OnPropertyChanged(nameof(IsDrawTool));
                OnPropertyChanged(nameof(IsFillTool));
                OnPropertyChanged(nameof(IsSelectMoveTool));
                OnPropertyChanged(nameof(EditorHintText));
                OnPropertyChanged(nameof(EditorStateText));
                if (value != FaceEditorTool.SelectMove)
                {
                    ClearSelection();
                }
            }
        }
    }

    public bool IsDrawTool => SelectedTool == FaceEditorTool.Draw;

    public bool IsFillTool => SelectedTool == FaceEditorTool.Fill;

    public bool IsSelectMoveTool => SelectedTool == FaceEditorTool.SelectMove;

    public bool IsHorizontalSymmetryEnabled
    {
        get => isHorizontalSymmetryEnabled;
        set
        {
            if (SetField(ref isHorizontalSymmetryEnabled, value))
            {
                OnPropertyChanged(nameof(EditorHintText));
                OnPropertyChanged(nameof(EditorStateText));
            }
        }
    }

    public bool IsVerticalSymmetryEnabled
    {
        get => isVerticalSymmetryEnabled;
        set
        {
            if (SetField(ref isVerticalSymmetryEnabled, value))
            {
                OnPropertyChanged(nameof(EditorHintText));
                OnPropertyChanged(nameof(EditorStateText));
            }
        }
    }

    public FaceSelectionBounds? SelectionBounds
    {
        get => selectionBounds;
        private set
        {
            if (SetField(ref selectionBounds, value))
            {
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(SelectionStatusText));
                OnPropertyChanged(nameof(EditorHintText));
                OnPropertyChanged(nameof(EditorStateText));
            }
        }
    }

    public bool HasSelection => SelectionBounds is not null;

    public string SelectionStatusText => SelectionBounds is { } selection
        ? $"Selection columns {selection.Left + 1}-{selection.Right + 1}, rows {selection.Top + 1}-{selection.Bottom + 1}."
        : "No pixel selection.";

    public string EditorHintText => SelectedTool switch
    {
        FaceEditorTool.Draw when IsHorizontalSymmetryEnabled && IsVerticalSymmetryEnabled =>
            "Drag to draw in four-way horizontal and vertical symmetry.",
        FaceEditorTool.Draw when IsHorizontalSymmetryEnabled =>
            "Drag to draw with left/right symmetry.",
        FaceEditorTool.Draw when IsVerticalSymmetryEnabled =>
            "Drag to draw with top/bottom symmetry.",
        FaceEditorTool.Draw => "Drag to draw or erase.",
        FaceEditorTool.Fill => "Tap a connected region to fill it; enabled symmetry also fills mirrored regions.",
        _ when HasSelection => "Drag the selected connected region to move it.",
        _ => "Tap a lit region to select it, then drag to move it."
    };

    public string EditorStateText =>
        $"Active tool: {SelectedTool switch
        {
            FaceEditorTool.SelectMove => "Select and move",
            _ => SelectedTool.ToString()
        }}. Horizontal symmetry {(IsHorizontalSymmetryEnabled ? "on" : "off")}. " +
        $"Vertical symmetry {(IsVerticalSymmetryEnabled ? "on" : "off")}. {SelectionStatusText}";

    public bool CanUndo => undoHistory.Count > 0;

    public bool CanRedo => redoHistory.Count > 0;

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string StoreStatusText
    {
        get => storeStatusText;
        private set => SetField(ref storeStatusText, value);
    }

    public string LastPayloadHex
    {
        get => lastPayloadHex;
        private set => SetField(ref lastPayloadHex, value);
    }

    public string LastCommandText
    {
        get => lastCommandText;
        private set => SetField(ref lastCommandText, value);
    }

    public int FrameCount
    {
        get => frameCount;
        private set => SetField(ref frameCount, value);
    }

    public bool UseCompatibilityWriteOnly
    {
        get => useCompatibilityWriteOnly;
        set
        {
            if (SetField(ref useCompatibilityWriteOnly, value))
            {
                OnPropertyChanged(nameof(AcknowledgementModeText));
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool SupportsAcknowledgements
    {
        get => supportsAcknowledgements;
        private set
        {
            if (SetField(ref supportsAcknowledgements, value))
            {
                OnPropertyChanged(nameof(AcknowledgementModeText));
            }
        }
    }

    public FaceUploadTransportState TransportState
    {
        get => transportState;
        private set
        {
            if (SetField(ref transportState, value))
            {
                OnPropertyChanged(nameof(CanUseCompatibilityWriteOnly));
                OnPropertyChanged(nameof(AcknowledgementModeText));
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanUseCompatibilityWriteOnly =>
        TransportState is FaceUploadTransportState.Ready
            or FaceUploadTransportState.CompatibilityReady
            or FaceUploadTransportState.Simulated;

    public string AcknowledgementModeText
    {
        get
        {
            if (UseCompatibilityWriteOnly)
            {
                return "Write-only compatibility: uploads and plays without ACK confirmation.";
            }

            return SupportsAcknowledgements
                ? "ACK required: upload waits for mask confirmation."
                : "ACK unavailable: enable write-only compatibility to upload.";
        }
    }

    public string ActiveTransportText =>
        transport.IsSimulated
            ? $"{transport.TransportDisplayName} (simulated)"
            : $"{transport.TransportDisplayName} (real)";

    public bool IsEditingBuiltIn => CurrentPattern.IsBuiltIn;

    public string CurrentPatternSubtitle => $"{CurrentPattern.SourceLabel} / {SlotSummaryText}";

    public string DeleteButtonText => CurrentPattern.IsBuiltIn ? "Built-in" : "Delete Face";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        StoreStatusText = state.Status;
        ApplyState(state);
        if (Patterns.Count == 0)
        {
            ApplyPattern(FacePatternFactory.CreateBlank());
            return;
        }

        var selected = Patterns.FirstOrDefault(pattern => pattern.Id == CurrentPattern.Id)
            ?? Patterns.First();
        ApplyPattern(selected);
    }

    public void SelectColor(string colorName)
    {
        var color = ActiveColorOptions.FirstOrDefault(option =>
            string.Equals(option.Name, colorName, StringComparison.OrdinalIgnoreCase))
            ?? FaceColorOptions.FirstOrDefault(option =>
                string.Equals(option.Name, colorName, StringComparison.OrdinalIgnoreCase));
        if (color is not null)
        {
            SelectedColor = color;
        }
    }

    public void SelectColor(FaceColorOption color)
    {
        if (color is not null)
        {
            SelectedColor = color;
        }
    }

    public void SetCell(int column, int row)
    {
        var standalone = editTransactionStart is null;
        if (standalone)
        {
            BeginEditTransaction();
        }

        ApplyToolAt(column, row);
        if (standalone)
        {
            EndEditTransaction();
        }
    }

    public void BeginCanvasInteraction(int column, int row)
    {
        BeginEditTransaction();
        lastInteractionCell = (column, row);
        if (SelectedTool == FaceEditorTool.SelectMove)
        {
            if (!selectedCellIndices.Contains((row * FacePattern.Width) + column))
            {
                SelectConnectedRegion(column, row);
            }

            selectionDragStart = (column, row);
            selectionDragPattern = CurrentPattern.Normalize();
            selectionDragIndices = selectedCellIndices.ToArray();
            return;
        }

        ApplyToolAt(column, row);
    }

    public void ContinueCanvasInteraction(int column, int row)
    {
        if (lastInteractionCell == (column, row))
        {
            return;
        }

        lastInteractionCell = (column, row);
        if (SelectedTool == FaceEditorTool.Draw)
        {
            DrawAt(column, row);
        }
        else if (SelectedTool == FaceEditorTool.SelectMove)
        {
            MoveSelectionTo(column, row);
        }
    }

    public void EndCanvasInteraction()
    {
        selectionDragStart = null;
        selectionDragPattern = null;
        selectionDragIndices = [];
        lastInteractionCell = null;
        EndEditTransaction();
    }

    public void Undo()
    {
        if (undoHistory.Count == 0)
        {
            return;
        }

        PushBounded(redoHistory, CaptureSnapshot());
        ApplySnapshot(undoHistory.Pop());
        StatusText = "Undid the last face edit.";
        NotifyHistoryChanged();
    }

    public void Redo()
    {
        if (redoHistory.Count == 0)
        {
            return;
        }

        PushBounded(undoHistory, CaptureSnapshot());
        ApplySnapshot(redoHistory.Pop());
        StatusText = "Redid the face edit.";
        NotifyHistoryChanged();
    }

    public void ImportImage(FaceSampleImage image, FacePatternSource source, string displayName)
    {
        var name = string.IsNullOrWhiteSpace(displayName)
            ? source == FacePatternSource.CapturedPhoto ? "Camera Face" : "Imported Face"
            : displayName.Trim();
        var pattern = FaceImageImport.CreatePattern(image, name, source, SelectedSlot);
        ApplyPattern(pattern);
        StatusText = "Image converted to the native 46x58 mask canvas.";
    }

    private Task NewBlankAsync(CancellationToken cancellationToken)
    {
        ApplyPattern(FacePatternFactory.CreateBlank("Custom Face", GetNextCustomSlot()));
        StatusText = "Blank 46x58 mask canvas ready.";
        return Task.CompletedTask;
    }

    private void Clear()
    {
        ApplyImmediatePixelChange(
            Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray(),
            "Face cleared.");
    }

    private void MirrorHorizontally()
    {
        var normalized = CurrentPattern.Normalize();
        var pixels = normalized.Pixels.ToArray();
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width / 2; column++)
            {
                var left = (row * FacePattern.Width) + column;
                var right = (row * FacePattern.Width) + (FacePattern.Width - column - 1);
                pixels[right] = pixels[left];
            }
        }

        ApplyImmediatePixelChange(pixels, "Left half mirrored to the right.");
    }

    private void MirrorVertically()
    {
        var normalized = CurrentPattern.Normalize();
        var pixels = normalized.Pixels.ToArray();
        for (var row = 0; row < FacePattern.Height / 2; row++)
        {
            var mirrorRow = FacePattern.Height - row - 1;
            for (var column = 0; column < FacePattern.Width; column++)
            {
                pixels[(mirrorRow * FacePattern.Width) + column] =
                    pixels[(row * FacePattern.Width) + column];
            }
        }

        ApplyImmediatePixelChange(pixels, "Top half mirrored to the bottom.");
    }

    private async Task SavePaletteAsync(CancellationToken cancellationToken)
    {
        var colors = CurrentPattern.Normalize().Pixels
            .Where(pixel => pixel.IsLit)
            .Select(pixel => pixel.Color)
            .Append(SelectedColor.Color)
            .Distinct()
            .Take(FaceSavedPalette.MaxColors)
            .ToArray();
        var palette = new FaceSavedPalette
        {
            Id = SelectedPalette?.Id ?? $"palette-{Guid.NewGuid():N}",
            DisplayName = PaletteName,
            Colors = colors
        }.Normalize();
        var state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        state = state with
        {
            SavedPalettes = state.SavedPalettes
                .Where(item => !string.Equals(item.Id, palette.Id, StringComparison.Ordinal))
                .Append(palette)
                .ToArray()
        };
        await store.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        SelectedPalette = SavedPalettes.First(item => string.Equals(item.Id, palette.Id, StringComparison.Ordinal));
        PaletteStatusText = $"Saved {palette.DisplayName} with {palette.Colors.Length} color(s).";
    }

    private async Task DeletePaletteAsync(CancellationToken cancellationToken)
    {
        if (SelectedPalette is not { } palette)
        {
            return;
        }

        var state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        state = state with
        {
            SavedPalettes = state.SavedPalettes
                .Where(item => !string.Equals(item.Id, palette.Id, StringComparison.Ordinal))
                .ToArray()
        };
        await store.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        SelectedPalette = null;
        PaletteName = $"Palette {SavedPalettes.Count + 1}";
        PaletteStatusText = $"Deleted {palette.DisplayName}. Default drawing colors active.";
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        var pattern = BuildDraftPattern(forceNewId: CurrentPattern.IsBuiltIn);
        var state = await store.UpsertAsync(pattern, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        ApplyPattern(state.Normalize().Patterns.First(item => item.Id == pattern.Id));
        StoreStatusText = CurrentPattern.IsBuiltIn ? "Saved built-in status." : $"Saved {pattern.DisplayName}.";
    }

    private async Task SaveAsCopyAsync(CancellationToken cancellationToken)
    {
        var pattern = BuildDraftPattern(forceNewId: true) with
        {
            DisplayName = FaceName.EndsWith(" Copy", StringComparison.OrdinalIgnoreCase)
                ? FaceName
                : $"{FaceName} Copy"
        };
        var state = await store.UpsertAsync(pattern, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        ApplyPattern(state.Normalize().Patterns.First(item => item.Id == pattern.Id));
        StoreStatusText = $"Saved copy {pattern.DisplayName}.";
    }

    private async Task DeleteCurrentAsync(CancellationToken cancellationToken)
    {
        if (CurrentPattern.IsBuiltIn)
        {
            StoreStatusText = "Built-in pixel faces stay in the library.";
            return;
        }

        var deletedId = CurrentPattern.Id;
        var state = await store.DeleteAsync(deletedId, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        ApplyPattern(Patterns.FirstOrDefault() ?? FacePatternFactory.CreateBlank());
        StoreStatusText = "Face deleted.";
    }

    private async Task UploadAsync(CancellationToken cancellationToken)
    {
        if (!CanUpload())
        {
            StatusText = BuildCannotUploadStatus();
            return;
        }

        var pattern = BuildDraftPattern(forceNewId: false);
        var package = FaceUploadProtocol.CreatePackage(pattern, SelectedSlot);
        var deleteCommand = FaceUploadProtocol.BuildDeleteCommand([SelectedSlot]);
        LastCommandText = $"{deleteCommand.DisplayName}; {package.StartCommand.DisplayName}; {package.FinishCommand.DisplayName}; {package.PlayCommand.DisplayName}";
        LastPayloadHex = TruncateDiagnosticHex(Convert.ToHexString(package.Payload));
        FrameCount = package.Frames.Count;

        try
        {
            IsSending = true;
            await FaceUploadOperationLock.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                StatusText = $"Uploading {pattern.DisplayName} to slot {SelectedSlot}...";
                var options = UseCompatibilityWriteOnly || !SupportsAcknowledgements
                    ? FaceUploadOptions.WriteOnlyCompatibility
                    : FaceUploadOptions.RequireAcknowledgements;
                var slotState = (await store.LoadAsync(cancellationToken).ConfigureAwait(false))
                    .ClearSlotInstallation(SelectedSlot);
                await store.SaveAsync(slotState, cancellationToken).ConfigureAwait(false);
                var result = await transport.UploadAsync(package, options, cancellationToken).ConfigureAwait(false);
                StatusText = result.Message;
                FrameCount = result.FramesSent;

                var timestamp = DateTimeOffset.UtcNow;
                var status = result.Succeeded
                    ? $"Uploaded slot {SelectedSlot}; needs real-mask visual confirmation."
                    : result.Message;
                var state = await store.UpsertAsync(pattern with
                {
                    LastUploadedAt = result.Succeeded ? timestamp : pattern.LastUploadedAt,
                    LastUploadStatus = status
                }, cancellationToken).ConfigureAwait(false);
                state = result.Succeeded
                    ? state.MarkSlotInstalled(
                        SelectedSlot,
                        FaceContentFingerprint.Compute(pattern),
                        $"face:{pattern.Id}",
                        timestamp)
                    : state.ClearSlotInstallation(SelectedSlot);
                await store.SaveAsync(state, cancellationToken).ConfigureAwait(false);
                ApplyState(state);
                ApplyPattern(state.Normalize().Patterns.First(item => item.Id == pattern.Id));
            }
            finally
            {
                FaceUploadOperationLock.Gate.Release();
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Face upload cancelled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed: {GetShortErrorMessage(ex)}";
        }
        finally
        {
            IsSending = false;
        }
    }

    private FacePattern BuildDraftPattern(bool forceNewId)
    {
        var now = DateTimeOffset.UtcNow;
        var source = CurrentPattern.IsBuiltIn || forceNewId
            ? FacePatternSource.Custom
            : CurrentPattern.Source;
        var emotion = source == FacePatternSource.Custom && !CurrentPattern.IsBuiltIn
            ? CurrentPattern.Emotion
            : FaceEmotion.Custom;
        return CurrentPattern with
        {
            Id = forceNewId ? $"face-{Guid.NewGuid():N}" : CurrentPattern.Id,
            DisplayName = string.IsNullOrWhiteSpace(FaceName) ? "Custom Face" : FaceName.Trim(),
            Source = source,
            Emotion = emotion,
            PreferredSlot = SelectedSlot,
            IsFavorite = CurrentPattern.IsFavorite && !forceNewId,
            UpdatedAt = now
        };
    }

    private void ApplyState(FacePatternStoreState state)
    {
        var selectedPaletteId = SelectedPalette?.Id;
        storeState = state.Normalize();
        Patterns = storeState.Patterns;
        SavedPalettes = storeState.SavedPalettes;
        if (!string.IsNullOrWhiteSpace(selectedPaletteId))
        {
            SelectedPalette = SavedPalettes.FirstOrDefault(palette =>
                string.Equals(palette.Id, selectedPaletteId, StringComparison.Ordinal));
        }

        RefreshPatternCards();
    }

    private void RefreshPatternCards()
    {
        PatternCards = Patterns
            .Select(pattern => new FacePatternCard(
                pattern,
                pattern.Id == CurrentPattern.Id,
                new AsyncRelayCommand(_ =>
                {
                    ApplyPattern(pattern);
                    return Task.CompletedTask;
                }),
                new AsyncRelayCommand(cancellationToken =>
                {
                    ApplyPattern(pattern);
                    return UploadAsync(cancellationToken);
                }, CanUpload),
                new AsyncRelayCommand(cancellationToken =>
                {
                    ApplyPattern(pattern);
                    return DeleteCurrentAsync(cancellationToken);
                }, () => !pattern.IsBuiltIn)))
            .ToArray();
    }

    private void ApplyPattern(FacePattern pattern)
    {
        CurrentPattern = pattern.Normalize();
        FaceName = CurrentPattern.DisplayName;
        SelectedSlot = CurrentPattern.PreferredSlot;
        var firstLitColor = CurrentPattern.Pixels.FirstOrDefault(pixel => pixel.IsLit).Color;
        if (firstLitColor != default)
        {
            SelectedColor = FaceColorOptions.FirstOrDefault(option => option.Color == firstLitColor)
                ?? new FaceColorOption("Picked", firstLitColor);
        }

        RefreshPreview();
        ClearSelection();
        ResetHistory();
        RefreshPatternCards();
        SaveCommand.RaiseCanExecuteChanged();
        SaveAsCopyCommand.RaiseCanExecuteChanged();
        UploadCommand.RaiseCanExecuteChanged();
    }

    private void ApplyToolAt(int column, int row)
    {
        if (column is < 0 or >= FacePattern.Width || row is < 0 or >= FacePattern.Height)
        {
            return;
        }

        if (SelectedTool == FaceEditorTool.Fill)
        {
            FillAt(column, row);
        }
        else if (SelectedTool == FaceEditorTool.SelectMove)
        {
            SelectConnectedRegion(column, row);
        }
        else
        {
            DrawAt(column, row);
        }
    }

    private void DrawAt(int column, int row)
    {
        var pattern = CurrentPattern.Normalize();
        var pixels = pattern.Pixels.ToArray();
        var pixel = IsEraseMode ? FacePixel.Off : new FacePixel(true, SelectedColor.Color);
        foreach (var index in GetSymmetricIndices(column, row))
        {
            pixels[index] = pixel;
        }

        ReplaceCanvasPixels(pixels);
    }

    private void FillAt(int column, int row)
    {
        var pattern = CurrentPattern.Normalize();
        var pixels = pattern.Pixels.ToArray();
        var replacement = IsEraseMode ? FacePixel.Off : new FacePixel(true, SelectedColor.Color);
        foreach (var index in GetSymmetricIndices(column, row))
        {
            FloodFill(pixels, index % FacePattern.Width, index / FacePattern.Width, replacement);
        }

        if (ReplaceCanvasPixels(pixels))
        {
            StatusText = "Connected region filled.";
        }
    }

    private IEnumerable<int> GetSymmetricIndices(int column, int row)
    {
        var columns = IsHorizontalSymmetryEnabled
            ? new[] { column, FacePattern.Width - column - 1 }
            : [column];
        var rows = IsVerticalSymmetryEnabled
            ? new[] { row, FacePattern.Height - row - 1 }
            : [row];
        return rows
            .SelectMany(targetRow => columns.Select(targetColumn => (targetRow * FacePattern.Width) + targetColumn))
            .Distinct();
    }

    private static void FloodFill(FacePixel[] pixels, int column, int row, FacePixel replacement)
    {
        var target = pixels[(row * FacePattern.Width) + column].Normalize();
        replacement = replacement.Normalize();
        if (target == replacement)
        {
            return;
        }

        var queue = new Queue<(int Column, int Row)>();
        queue.Enqueue((column, row));
        while (queue.TryDequeue(out var cell))
        {
            if (cell.Column is < 0 or >= FacePattern.Width || cell.Row is < 0 or >= FacePattern.Height)
            {
                continue;
            }

            var index = (cell.Row * FacePattern.Width) + cell.Column;
            if (pixels[index].Normalize() != target)
            {
                continue;
            }

            pixels[index] = replacement;
            queue.Enqueue((cell.Column - 1, cell.Row));
            queue.Enqueue((cell.Column + 1, cell.Row));
            queue.Enqueue((cell.Column, cell.Row - 1));
            queue.Enqueue((cell.Column, cell.Row + 1));
        }
    }

    private void SelectConnectedRegion(int column, int row)
    {
        var pattern = CurrentPattern.Normalize();
        var start = pattern.GetPixel(column, row).Normalize();
        if (!start.IsLit)
        {
            ClearSelection();
            StatusText = "Tap a lit region to select it.";
            return;
        }

        var selected = new HashSet<int>();
        var queue = new Queue<(int Column, int Row)>();
        queue.Enqueue((column, row));
        while (queue.TryDequeue(out var cell))
        {
            if (cell.Column is < 0 or >= FacePattern.Width || cell.Row is < 0 or >= FacePattern.Height)
            {
                continue;
            }

            var index = (cell.Row * FacePattern.Width) + cell.Column;
            if (selected.Contains(index) || pattern.Pixels[index].Normalize() != start)
            {
                continue;
            }

            selected.Add(index);
            queue.Enqueue((cell.Column - 1, cell.Row));
            queue.Enqueue((cell.Column + 1, cell.Row));
            queue.Enqueue((cell.Column, cell.Row - 1));
            queue.Enqueue((cell.Column, cell.Row + 1));
        }

        selectedCellIndices = selected;
        UpdateSelectionBounds();
        StatusText = $"Selected {selected.Count} connected pixel(s).";
    }

    private void MoveSelectionTo(int column, int row)
    {
        if (selectionDragStart is not { } start || selectionDragPattern is null || selectionDragIndices.Length == 0)
        {
            return;
        }

        var minColumn = selectionDragIndices.Min(index => index % FacePattern.Width);
        var maxColumn = selectionDragIndices.Max(index => index % FacePattern.Width);
        var minRow = selectionDragIndices.Min(index => index / FacePattern.Width);
        var maxRow = selectionDragIndices.Max(index => index / FacePattern.Width);
        var deltaColumn = Math.Clamp(column - start.Column, -minColumn, FacePattern.Width - maxColumn - 1);
        var deltaRow = Math.Clamp(row - start.Row, -minRow, FacePattern.Height - maxRow - 1);
        var source = selectionDragPattern.Normalize();
        var pixels = source.Pixels.ToArray();
        foreach (var index in selectionDragIndices)
        {
            pixels[index] = FacePixel.Off;
        }

        selectedCellIndices = [];
        foreach (var index in selectionDragIndices)
        {
            var sourceColumn = index % FacePattern.Width;
            var sourceRow = index / FacePattern.Width;
            var destination = ((sourceRow + deltaRow) * FacePattern.Width) + sourceColumn + deltaColumn;
            pixels[destination] = source.Pixels[index];
            selectedCellIndices.Add(destination);
        }

        ReplaceCanvasPixels(pixels);
        UpdateSelectionBounds();
        StatusText = $"Selection moved {deltaColumn:+0;-0;0} columns, {deltaRow:+0;-0;0} rows.";
    }

    private void ClearSelection()
    {
        selectedCellIndices = [];
        SelectionBounds = null;
    }

    private void UpdateSelectionBounds()
    {
        if (selectedCellIndices.Count == 0)
        {
            SelectionBounds = null;
            return;
        }

        SelectionBounds = new FaceSelectionBounds(
            selectedCellIndices.Min(index => index % FacePattern.Width),
            selectedCellIndices.Min(index => index / FacePattern.Width),
            selectedCellIndices.Max(index => index % FacePattern.Width),
            selectedCellIndices.Max(index => index / FacePattern.Width));
    }

    private void ApplyImmediatePixelChange(FacePixel[] pixels, string status)
    {
        var before = CaptureSnapshot();
        if (!ReplaceCanvasPixels(pixels))
        {
            return;
        }

        PushBounded(undoHistory, before);
        redoHistory.Clear();
        StatusText = status;
        NotifyHistoryChanged();
    }

    private bool ReplaceCanvasPixels(FacePixel[] pixels)
    {
        var normalized = CurrentPattern.Normalize();
        var replacement = pixels.Select(pixel => pixel.Normalize()).ToArray();
        if (normalized.Pixels.SequenceEqual(replacement))
        {
            return false;
        }

        CurrentPattern = normalized with
        {
            Pixels = replacement,
            DisplayName = FaceName,
            Source = normalized.IsBuiltIn ? FacePatternSource.Custom : normalized.Source,
            Emotion = normalized.IsBuiltIn ? FaceEmotion.Custom : normalized.Emotion,
            PreferredSlot = SelectedSlot,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        RefreshPreview();
        return true;
    }

    private void BeginEditTransaction() => editTransactionStart ??= CaptureSnapshot();

    private void EndEditTransaction()
    {
        if (editTransactionStart is not { } before)
        {
            return;
        }

        editTransactionStart = null;
        if (!SnapshotsEqual(before, CaptureSnapshot()))
        {
            PushBounded(undoHistory, before);
            redoHistory.Clear();
            NotifyHistoryChanged();
        }
    }

    private EditorSnapshot CaptureSnapshot() => new(CurrentPattern, FaceName, SelectedSlot);

    private void ApplySnapshot(EditorSnapshot snapshot)
    {
        CurrentPattern = snapshot.Pattern;
        FaceName = snapshot.FaceName;
        SelectedSlot = snapshot.SelectedSlot;
        ClearSelection();
        RefreshPreview();
        RefreshPatternCards();
    }

    private static bool SnapshotsEqual(EditorSnapshot left, EditorSnapshot right) =>
        left.FaceName == right.FaceName &&
        left.SelectedSlot == right.SelectedSlot &&
        left.Pattern.Id == right.Pattern.Id &&
        left.Pattern.Source == right.Pattern.Source &&
        left.Pattern.Emotion == right.Pattern.Emotion &&
        left.Pattern.Pixels.SequenceEqual(right.Pattern.Pixels);

    private static void PushBounded(Stack<EditorSnapshot> stack, EditorSnapshot snapshot)
    {
        if (stack.Count >= MaxHistoryDepth)
        {
            var retained = stack.Reverse().Skip(1).ToArray();
            stack.Clear();
            foreach (var item in retained)
            {
                stack.Push(item);
            }
        }

        stack.Push(snapshot);
    }

    private void ResetHistory()
    {
        undoHistory.Clear();
        redoHistory.Clear();
        editTransactionStart = null;
        NotifyHistoryChanged();
    }

    private void NotifyHistoryChanged()
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
    }

    private void RefreshPreview()
    {
        var pattern = CurrentPattern.Normalize();
        var cells = new FacePreviewCell[FacePattern.PixelCount];
        var index = 0;
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width; column++)
            {
                var pixel = pattern.GetPixel(column, row);
                cells[index++] = new FacePreviewCell(column, row, pixel.IsLit, pixel.IsLit ? pixel.Color.Hex : OffColor);
            }
        }

        PreviewCells = cells;
    }

    private int GetNextCustomSlot()
    {
        if (Patterns.Count == 0)
        {
            return 7;
        }

        var state = storeState with { Patterns = Patterns };
        return state.NextCustomSlot(AppBuiltInAnimationCatalog.ReservedSlots);
    }

    private AsyncRelayCommand CreateToolCommand(FaceEditorTool tool) => new(_ =>
    {
        SelectedTool = tool;
        return Task.CompletedTask;
    });

    private static string GetColorName(FaceColor color, int index) =>
        FaceColorOption.Defaults.FirstOrDefault(option => option.Color == color)?.Name
        ?? $"Saved {index + 1}";

    private bool CanSave() => !string.IsNullOrWhiteSpace(FaceName);

    private bool CanUpload()
    {
        if (IsSending || !transport.IsReady || string.IsNullOrWhiteSpace(FaceName))
        {
            return false;
        }

        if (UseCompatibilityWriteOnly)
        {
            return CanUseCompatibilityWriteOnly;
        }

        return SupportsAcknowledgements
            && TransportState is FaceUploadTransportState.Ready or FaceUploadTransportState.Simulated;
    }

    private string BuildCannotUploadStatus()
    {
        if (transport.IsReady && !SupportsAcknowledgements && !UseCompatibilityWriteOnly)
        {
            return "ACK notifications are unavailable. Enable write-only compatibility to upload.";
        }

        return transport.StatusText;
    }

    private void OnTransportStateChanged(object? sender, FaceUploadTransportStateChangedEventArgs e)
    {
        TransportState = e.State;
        SupportsAcknowledgements = e.SupportsAcknowledgements;
        StatusText = e.Message;
        if (ShouldDefaultToCompatibilityMode(e.State, e.SupportsAcknowledgements))
        {
            UseCompatibilityWriteOnly = true;
        }

        OnPropertyChanged(nameof(ActiveTransportText));
    }

    private static bool ShouldDefaultToCompatibilityMode(
        FaceUploadTransportState state,
        bool supportsAcknowledgements) =>
        !supportsAcknowledgements && state == FaceUploadTransportState.CompatibilityReady;

    private static string TruncateDiagnosticHex(string value) =>
        value.Length <= MaxDiagnosticHexLength
            ? value
            : string.Concat(value.AsSpan(0, MaxDiagnosticHexLength), "...");

    private static string GetShortErrorMessage(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message)
            ? ex.GetType().Name
            : ex.Message;
        return message.Length <= 96 ? message : string.Concat(message.AsSpan(0, 96), "...");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private sealed record EditorSnapshot(FacePattern Pattern, string FaceName, int SelectedSlot);
}
