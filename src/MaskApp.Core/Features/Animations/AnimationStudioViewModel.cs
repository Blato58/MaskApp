using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public sealed record AnimationTimelineFrame(
    int Index,
    string FrameId,
    string Label,
    string DurationText,
    FacePattern Pattern,
    bool IsSelected);

public enum AnimationEditorTool
{
    Draw,
    Fill,
    SelectMove
}

public sealed record AnimationSelectionBounds(int Left, int Top, int Right, int Bottom);

public sealed class AnimationStudioViewModel : INotifyPropertyChanged
{
    private const string OffColor = "#05070D";
    private const int MaxHistoryDepth = 50;

    private readonly IAnimationProjectStore store;
    private readonly AnimationProjectCompiler compiler;
    private readonly DiySlotPlaybackCoordinator playbackCoordinator;
    private readonly FlashSafetyAnalyzer flashSafetyAnalyzer;
    private readonly IFlashSafetyAcknowledgementStore acknowledgementStore;
    private readonly FlashSafetyAcknowledgementService acknowledgementService;
    private readonly AnimationMediaImportService mediaImportService;
    private readonly TapTempoTracker tapTempoTracker = new();
    private AnimationProjectStoreState storeState = new();
    private AnimationProject currentProject = AnimationProject.CreateBlank();
    private IReadOnlyList<AnimationProject> projects = [];
    private IReadOnlyList<AnimationTimelineFrame> timelineFrames = [];
    private IReadOnlyList<FacePreviewCell> previewCells = [];
    private int selectedFrameIndex;
    private FaceColorOption selectedColor = FaceColorOption.Defaults[0];
    private bool isEraseMode;
    private bool onionSkinEnabled;
    private bool isBusy;
    private string statusText = "Animation Studio ready.";
    private string storeStatusText = "Project library ready.";
    private string budgetText = "1/20 unique DIY slots";
    private string safetyStatusText = "Safety analysis pending.";
    private FlashSafetyStatus safetyStatus = FlashSafetyStatus.Safe;
    private AnimationCompilationResult compilation = new();
    private FlashSafetyAssessment? safetyAssessment;
    private FlashSafetyAcknowledgementState acknowledgementState = new();
    private AnimationResizeMode importResizeMode = AnimationResizeMode.Crop;
    private AnimationPaletteMode importPaletteMode = AnimationPaletteMode.FullColor;
    private AnimationDitherMode importDitherMode;
    private double importHorizontalPosition;
    private double importVerticalPosition;
    private double importSampleMilliseconds = 100;
    private readonly Stack<EditorSnapshot> undoHistory = new();
    private readonly Stack<EditorSnapshot> redoHistory = new();
    private EditorSnapshot? editTransactionStart;
    private AnimationEditorTool selectedTool;
    private bool isSymmetryEnabled;
    private bool guidesEnabled = true;
    private bool isDirty;
    private HashSet<int> selectedCellIndices = [];
    private AnimationSelectionBounds? selectionBounds;
    private (int Column, int Row)? selectionDragStart;
    private FacePattern? selectionDragPattern;
    private int[] selectionDragIndices = [];
    private (int Column, int Row)? lastInteractionCell;

    public AnimationStudioViewModel(
        IAnimationProjectStore store,
        AnimationProjectCompiler compiler,
        DiySlotPlaybackCoordinator playbackCoordinator,
        FlashSafetyAnalyzer flashSafetyAnalyzer,
        IFlashSafetyAcknowledgementStore acknowledgementStore,
        FlashSafetyAcknowledgementService acknowledgementService,
        AnimationMediaImportService mediaImportService)
    {
        this.store = store;
        this.compiler = compiler;
        this.playbackCoordinator = playbackCoordinator;
        this.flashSafetyAnalyzer = flashSafetyAnalyzer;
        this.acknowledgementStore = acknowledgementStore;
        this.acknowledgementService = acknowledgementService;
        this.mediaImportService = mediaImportService;

        NewCommand = new AsyncRelayCommand(NewAsync, () => !IsBusy, SetCommandError);
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave, SetCommandError);
        DeleteProjectCommand = new AsyncRelayCommand(DeleteProjectAsync, () => !IsBusy, SetCommandError);
        PrepareCommand = new AsyncRelayCommand(PrepareAsync, CanUseCompiledAnimation, SetCommandError);
        PreviewCommand = new AsyncRelayCommand(PreviewAsync, CanUseCompiledAnimation, SetCommandError);
        StopCommand = new AsyncRelayCommand(StopAsync, () => !IsBusy, SetCommandError);
        AcknowledgeSafetyCommand = new AsyncRelayCommand(AcknowledgeSafetyAsync, CanAcknowledgeSafety, SetCommandError);
        RevokeSafetyCommand = new AsyncRelayCommand(RevokeSafetyAsync, CanRevokeSafety, SetCommandError);
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
        SelectDrawToolCommand = CreateToolCommand(AnimationEditorTool.Draw);
        SelectFillToolCommand = CreateToolCommand(AnimationEditorTool.Fill);
        SelectMoveToolCommand = CreateToolCommand(AnimationEditorTool.SelectMove);
        ApplyProject(currentProject);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<FaceColorOption> FaceColorOptions => FaceColorOption.Defaults;

    public IReadOnlyList<AnimationLoopMode> LoopModes { get; } = Enum.GetValues<AnimationLoopMode>();

    public IReadOnlyList<AnimationResizeMode> ResizeModes { get; } = Enum.GetValues<AnimationResizeMode>();

    public IReadOnlyList<AnimationPaletteMode> PaletteModes { get; } = Enum.GetValues<AnimationPaletteMode>();

    public IReadOnlyList<AnimationDitherMode> DitherModes { get; } = Enum.GetValues<AnimationDitherMode>();

    public AsyncRelayCommand NewCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand DeleteProjectCommand { get; }
    public AsyncRelayCommand PrepareCommand { get; }
    public AsyncRelayCommand PreviewCommand { get; }
    public AsyncRelayCommand StopCommand { get; }
    public AsyncRelayCommand AcknowledgeSafetyCommand { get; }
    public AsyncRelayCommand RevokeSafetyCommand { get; }
    public AsyncRelayCommand UndoCommand { get; }
    public AsyncRelayCommand RedoCommand { get; }
    public AsyncRelayCommand SelectDrawToolCommand { get; }
    public AsyncRelayCommand SelectFillToolCommand { get; }
    public AsyncRelayCommand SelectMoveToolCommand { get; }

    public IReadOnlyList<AnimationProject> Projects
    {
        get => projects;
        private set => SetField(ref projects, value);
    }

    public AnimationProject CurrentProject
    {
        get => currentProject;
        private set
        {
            if (SetField(ref currentProject, value))
            {
                OnPropertyChanged(nameof(ProjectName));
                OnPropertyChanged(nameof(LoopMode));
                OnPropertyChanged(nameof(FiniteLoopCount));
                OnPropertyChanged(nameof(Bpm));
                OnPropertyChanged(nameof(IsFiniteLoop));
                OnPropertyChanged(nameof(SelectedFrame));
                OnPropertyChanged(nameof(SelectedFrameDurationMilliseconds));
                OnPropertyChanged(nameof(OnionSkinPattern));
            }
        }
    }

    public string ProjectName
    {
        get => CurrentProject.DisplayName;
        set
        {
            var normalized = value ?? string.Empty;
            if (!string.Equals(CurrentProject.DisplayName, normalized, StringComparison.Ordinal))
            {
                CurrentProject = CurrentProject with { DisplayName = normalized, UpdatedAt = DateTimeOffset.UtcNow };
                IsDirty = true;
                RefreshDerivedState();
            }
        }
    }

    public AnimationLoopMode LoopMode
    {
        get => CurrentProject.LoopMode;
        set
        {
            if (CurrentProject.LoopMode != value)
            {
                CurrentProject = CurrentProject with { LoopMode = value, UpdatedAt = DateTimeOffset.UtcNow };
                IsDirty = true;
                OnPropertyChanged(nameof(IsFiniteLoop));
                RefreshDerivedState();
            }
        }
    }

    public bool IsFiniteLoop => LoopMode == AnimationLoopMode.Finite;

    public int FiniteLoopCount
    {
        get => CurrentProject.FiniteLoopCount;
        set
        {
            var normalized = Math.Clamp(value, 1, PerformanceAnimation.MaxFiniteLoops);
            if (CurrentProject.FiniteLoopCount != normalized)
            {
                CurrentProject = CurrentProject with { FiniteLoopCount = normalized, UpdatedAt = DateTimeOffset.UtcNow };
                IsDirty = true;
                RefreshDerivedState();
            }
        }
    }

    public double Bpm
    {
        get => CurrentProject.Bpm ?? PerformanceAnimationBuilder.DefaultBpm;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return;
            }

            var normalized = Math.Clamp(value, 30, 300);
            if (Math.Abs(Bpm - normalized) > 0.001)
            {
                var ratio = Bpm / normalized;
                var frames = CurrentProject.Frames
                    .Select(frame => frame with
                    {
                        Duration = ClampFrameDuration(TimeSpan.FromTicks(
                            (long)Math.Round(frame.Duration.Ticks * ratio)))
                    })
                    .ToArray();
                CurrentProject = CurrentProject with
                {
                    Bpm = normalized,
                    Frames = frames,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                IsDirty = true;
                RefreshDerivedState();
            }
        }
    }

    public IReadOnlyList<AnimationTimelineFrame> TimelineFrames
    {
        get => timelineFrames;
        private set => SetField(ref timelineFrames, value);
    }

    public int SelectedFrameIndex
    {
        get => selectedFrameIndex;
        private set
        {
            var normalized = Math.Clamp(value, 0, Math.Max(0, CurrentProject.Frames.Count - 1));
            if (SetField(ref selectedFrameIndex, normalized))
            {
                ClearSelection();
                OnPropertyChanged(nameof(SelectedFrame));
                OnPropertyChanged(nameof(SelectedFrameDurationMilliseconds));
                OnPropertyChanged(nameof(OnionSkinPattern));
                RefreshPreview();
                RefreshTimeline();
            }
        }
    }

    public AnimationProjectFrame SelectedFrame => CurrentProject.Frames[SelectedFrameIndex];

    public double SelectedFrameDurationMilliseconds
    {
        get => SelectedFrame.Duration.TotalMilliseconds;
        set => SetSelectedFrameDuration(TimeSpan.FromMilliseconds(value));
    }

    public FacePattern? OnionSkinPattern => OnionSkinEnabled && SelectedFrameIndex > 0
        ? CurrentProject.Frames[SelectedFrameIndex - 1].Pattern
        : null;

    public IReadOnlyList<FacePreviewCell> PreviewCells
    {
        get => previewCells;
        private set => SetField(ref previewCells, value);
    }

    public FaceColorOption SelectedColor
    {
        get => selectedColor;
        set => SetField(ref selectedColor, value ?? FaceColorOption.Defaults[0]);
    }

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

    public AnimationEditorTool SelectedTool
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
                if (value != AnimationEditorTool.SelectMove)
                {
                    ClearSelection();
                }
            }
        }
    }

    public bool IsDrawTool => SelectedTool == AnimationEditorTool.Draw;

    public bool IsFillTool => SelectedTool == AnimationEditorTool.Fill;

    public bool IsSelectMoveTool => SelectedTool == AnimationEditorTool.SelectMove;

    public bool IsSymmetryEnabled
    {
        get => isSymmetryEnabled;
        set
        {
            if (SetField(ref isSymmetryEnabled, value))
            {
                OnPropertyChanged(nameof(EditorHintText));
            }
        }
    }

    public bool GuidesEnabled
    {
        get => guidesEnabled;
        set => SetField(ref guidesEnabled, value);
    }

    public AnimationSelectionBounds? SelectionBounds
    {
        get => selectionBounds;
        private set
        {
            if (SetField(ref selectionBounds, value))
            {
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(EditorHintText));
            }
        }
    }

    public bool HasSelection => SelectionBounds is not null;

    public string EditorHintText => SelectedTool switch
    {
        AnimationEditorTool.Draw => IsSymmetryEnabled
            ? "Draw mirrors across the center line. Guides never become pixels."
            : "Drag to draw or erase. Guides never become pixels.",
        AnimationEditorTool.Fill => "Tap a connected region to fill it.",
        _ => HasSelection
            ? "Drag the selected connected region to move it."
            : "Tap a lit region to select it, then drag to move."
    };

    public bool IsDirty
    {
        get => isDirty;
        private set
        {
            if (SetField(ref isDirty, value))
            {
                OnPropertyChanged(nameof(SaveStateText));
            }
        }
    }

    public string SaveStateText => IsDirty ? "Unsaved changes" : "Saved state";

    public bool CanUndo => undoHistory.Count > 0;

    public bool CanRedo => redoHistory.Count > 0;

    public bool OnionSkinEnabled
    {
        get => onionSkinEnabled;
        set
        {
            if (SetField(ref onionSkinEnabled, value))
            {
                OnPropertyChanged(nameof(OnionSkinPattern));
                RefreshPreview();
            }
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                RaiseCommandStates();
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

    public string BudgetText
    {
        get => budgetText;
        private set => SetField(ref budgetText, value);
    }

    public string SafetyStatusText
    {
        get => safetyStatusText;
        private set => SetField(ref safetyStatusText, value);
    }

    public FlashSafetyStatus SafetyStatus
    {
        get => safetyStatus;
        private set
        {
            if (SetField(ref safetyStatus, value))
            {
                OnPropertyChanged(nameof(IsSafetyBlocked));
                OnPropertyChanged(nameof(HasSafetyOverride));
                RaiseCommandStates();
            }
        }
    }

    public bool IsSafetyBlocked => SafetyStatus == FlashSafetyStatus.Blocked;

    public bool HasSafetyOverride => SafetyStatus == FlashSafetyStatus.AcknowledgedOverride;

    public AnimationResizeMode ImportResizeMode
    {
        get => importResizeMode;
        set => SetField(ref importResizeMode, value);
    }

    public AnimationPaletteMode ImportPaletteMode
    {
        get => importPaletteMode;
        set => SetField(ref importPaletteMode, value);
    }

    public AnimationDitherMode ImportDitherMode
    {
        get => importDitherMode;
        set => SetField(ref importDitherMode, value);
    }

    public double ImportHorizontalPosition
    {
        get => importHorizontalPosition;
        set => SetField(ref importHorizontalPosition, Math.Clamp(value, -1, 1));
    }

    public double ImportVerticalPosition
    {
        get => importVerticalPosition;
        set => SetField(ref importVerticalPosition, Math.Clamp(value, -1, 1));
    }

    public double ImportSampleMilliseconds
    {
        get => importSampleMilliseconds;
        set => SetField(ref importSampleMilliseconds, Math.Clamp(value, 20, 2000));
    }

    public AnimationMediaConversionOptions BuildImportOptions() => new()
    {
        ResizeMode = ImportResizeMode,
        PaletteMode = ImportPaletteMode,
        DitherMode = ImportDitherMode,
        HorizontalPosition = ImportHorizontalPosition,
        VerticalPosition = ImportVerticalPosition
    };

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        storeState = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        acknowledgementState = (await acknowledgementStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        Projects = storeState.Projects;
        StoreStatusText = storeState.Status;
        ApplyProject(Projects.FirstOrDefault() ?? AnimationProject.CreateBlank());
    }

    public void SelectProject(string projectId)
    {
        var project = Projects.FirstOrDefault(item => string.Equals(item.Id, projectId, StringComparison.Ordinal));
        if (project is not null)
        {
            ApplyProject(project);
            StatusText = $"Editing {project.DisplayName}.";
        }
    }

    public void SelectFrame(int index) => SelectedFrameIndex = index;

    public void SelectColor(string colorName)
    {
        var color = FaceColorOptions.FirstOrDefault(option =>
            string.Equals(option.Name, colorName, StringComparison.OrdinalIgnoreCase));
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
        if (SelectedTool == AnimationEditorTool.SelectMove)
        {
            if (!selectedCellIndices.Contains((row * FacePattern.Width) + column))
            {
                SelectConnectedRegion(column, row);
            }

            selectionDragStart = (column, row);
            selectionDragPattern = SelectedFrame.Pattern.Normalize();
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
        if (SelectedTool == AnimationEditorTool.Draw)
        {
            DrawAt(column, row);
        }
        else if (SelectedTool == AnimationEditorTool.SelectMove)
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

    public void ClearSelectedFrame()
    {
        RecordUndoSnapshot();
        ReplaceSelectedPattern(SelectedFrame.Pattern with
        {
            Pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray()
        });
        StatusText = "Selected frame cleared.";
    }

    public void MirrorSelectedFrameHorizontally()
    {
        RecordUndoSnapshot();
        var pattern = SelectedFrame.Pattern.Normalize();
        var pixels = pattern.Pixels.ToArray();
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width / 2; column++)
            {
                pixels[(row * FacePattern.Width) + FacePattern.Width - column - 1] =
                    pixels[(row * FacePattern.Width) + column];
            }
        }

        ReplaceSelectedPattern(pattern with { Pixels = pixels });
        StatusText = "Left half mirrored to the right.";
    }

    public void DuplicateSelectedFrame()
    {
        if (CurrentProject.Frames.Count >= AnimationProject.MaxSourceFrames)
        {
            StatusText = $"Frame limit reached ({AnimationProject.MaxSourceFrames}).";
            return;
        }

        RecordUndoSnapshot();
        var frames = CurrentProject.Frames.ToList();
        frames.Insert(SelectedFrameIndex + 1, SelectedFrame with { Id = $"frame-{Guid.NewGuid():N}" });
        ReplaceFrames(frames, SelectedFrameIndex + 1, "Frame duplicated.");
    }

    public void InsertBlankFrame()
    {
        if (CurrentProject.Frames.Count >= AnimationProject.MaxSourceFrames)
        {
            StatusText = $"Frame limit reached ({AnimationProject.MaxSourceFrames}).";
            return;
        }

        RecordUndoSnapshot();
        var frames = CurrentProject.Frames.ToList();
        frames.Insert(SelectedFrameIndex + 1, new AnimationProjectFrame
        {
            Id = $"frame-{Guid.NewGuid():N}",
            Pattern = FacePatternFactory.CreateBlank("Animation frame", FacePattern.MinSlot),
            Duration = SelectedFrame.Duration
        });
        ReplaceFrames(frames, SelectedFrameIndex + 1, "Blank frame inserted.");
    }

    public void DeleteSelectedFrame()
    {
        if (CurrentProject.Frames.Count == 1)
        {
            StatusText = "An animation must keep at least one frame; clear it instead.";
            return;
        }

        RecordUndoSnapshot();
        var frames = CurrentProject.Frames.ToList();
        frames.RemoveAt(SelectedFrameIndex);
        ReplaceFrames(frames, Math.Min(SelectedFrameIndex, frames.Count - 1), "Frame deleted.");
    }

    public void MoveSelectedFrame(int offset) => MoveFrame(SelectedFrameIndex, SelectedFrameIndex + offset);

    public void MoveFrame(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= CurrentProject.Frames.Count ||
            toIndex < 0 || toIndex >= CurrentProject.Frames.Count || fromIndex == toIndex)
        {
            return;
        }

        RecordUndoSnapshot();
        var frames = CurrentProject.Frames.ToList();
        var frame = frames[fromIndex];
        frames.RemoveAt(fromIndex);
        frames.Insert(toIndex, frame);
        ReplaceFrames(frames, toIndex, $"Frame moved to position {toIndex + 1}.");
    }

    public void SetSelectedFrameDuration(TimeSpan duration)
    {
        var normalized = duration < PerformanceAnimation.MinFrameDuration
            ? PerformanceAnimation.MinFrameDuration
            : duration > PerformanceAnimation.MaxFrameDuration
                ? PerformanceAnimation.MaxFrameDuration
                : duration;
        RecordUndoSnapshot();
        var frames = CurrentProject.Frames.ToArray();
        frames[SelectedFrameIndex] = SelectedFrame with { Duration = normalized };
        ReplaceFrames(frames, SelectedFrameIndex, $"Frame duration {normalized.TotalMilliseconds:0} ms.");
    }

    public void Undo()
    {
        if (undoHistory.Count == 0)
        {
            return;
        }

        PushBounded(redoHistory, CaptureSnapshot());
        ApplySnapshot(undoHistory.Pop());
        StatusText = "Undid the last editor change.";
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
        StatusText = "Redid the editor change.";
        NotifyHistoryChanged();
    }

    private void ApplyToolAt(int column, int row)
    {
        if (column is < 0 or >= FacePattern.Width || row is < 0 or >= FacePattern.Height)
        {
            return;
        }

        if (SelectedTool == AnimationEditorTool.Fill)
        {
            FillAt(column, row);
        }
        else if (SelectedTool == AnimationEditorTool.SelectMove)
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
        var pattern = SelectedFrame.Pattern.Normalize();
        var pixels = pattern.Pixels.ToArray();
        var pixel = IsEraseMode ? FacePixel.Off : new FacePixel(true, SelectedColor.Color);
        pixels[(row * FacePattern.Width) + column] = pixel;
        if (IsSymmetryEnabled)
        {
            var mirrorColumn = FacePattern.Width - column - 1;
            pixels[(row * FacePattern.Width) + mirrorColumn] = pixel;
        }

        ReplaceSelectedPattern(pattern with { Pixels = pixels });
    }

    private void FillAt(int column, int row)
    {
        var pattern = SelectedFrame.Pattern.Normalize();
        var replacement = IsEraseMode ? FacePixel.Off : new FacePixel(true, SelectedColor.Color);
        var pixels = pattern.Pixels.ToArray();
        FloodFill(pixels, column, row, replacement);
        if (IsSymmetryEnabled)
        {
            FloodFill(pixels, FacePattern.Width - column - 1, row, replacement);
        }

        ReplaceSelectedPattern(pattern with { Pixels = pixels });
        StatusText = "Connected region filled.";
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
        var pattern = SelectedFrame.Pattern.Normalize();
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

        ReplaceSelectedPattern(source with { Pixels = pixels });
        UpdateSelectionBounds();
        StatusText = $"Selection moved {deltaColumn:+0;-0;0} columns, {deltaRow:+0;-0;0} rows.";
    }

    private void ClearSelection()
    {
        selectedCellIndices = [];
        SelectionBounds = null;
        OnPropertyChanged(nameof(EditorHintText));
    }

    private void UpdateSelectionBounds()
    {
        if (selectedCellIndices.Count == 0)
        {
            SelectionBounds = null;
            return;
        }

        SelectionBounds = new AnimationSelectionBounds(
            selectedCellIndices.Min(index => index % FacePattern.Width),
            selectedCellIndices.Min(index => index / FacePattern.Width),
            selectedCellIndices.Max(index => index % FacePattern.Width),
            selectedCellIndices.Max(index => index / FacePattern.Width));
        OnPropertyChanged(nameof(EditorHintText));
    }

    private void BeginEditTransaction() => editTransactionStart ??= CaptureSnapshot();

    private void EndEditTransaction()
    {
        if (editTransactionStart is not { } before)
        {
            return;
        }

        editTransactionStart = null;
        if (!ReferenceEquals(before.Project, CurrentProject))
        {
            PushBounded(undoHistory, before);
            redoHistory.Clear();
            IsDirty = true;
            NotifyHistoryChanged();
        }
    }

    private void RecordUndoSnapshot()
    {
        if (editTransactionStart is not null)
        {
            return;
        }

        PushBounded(undoHistory, CaptureSnapshot());
        redoHistory.Clear();
        NotifyHistoryChanged();
    }

    private EditorSnapshot CaptureSnapshot() => new(CurrentProject, SelectedFrameIndex);

    private void ApplySnapshot(EditorSnapshot snapshot)
    {
        CurrentProject = snapshot.Project;
        selectedFrameIndex = Math.Clamp(snapshot.SelectedFrameIndex, 0, CurrentProject.Frames.Count - 1);
        OnPropertyChanged(nameof(SelectedFrameIndex));
        OnPropertyChanged(nameof(SelectedFrame));
        OnPropertyChanged(nameof(SelectedFrameDurationMilliseconds));
        ClearSelection();
        IsDirty = true;
        RefreshDerivedState();
    }

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

    private void NotifyHistoryChanged()
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
    }

    public double? AddTap(TimeSpan monotonicTimestamp)
    {
        var bpm = tapTempoTracker.AddTap(monotonicTimestamp);
        if (bpm is not null)
        {
            Bpm = bpm.Value;
            StatusText = $"Tap tempo: {bpm.Value:0.0} BPM.";
        }
        else
        {
            StatusText = "Tap again to measure tempo.";
        }

        return bpm;
    }

    public async Task<AnimationMediaImportResult> ImportMediaAsync(
        Stream stream,
        string displayName,
        AnimationMediaKind kind,
        AnimationMediaConversionOptions options,
        TimeSpan sampleInterval,
        CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        try
        {
            StatusText = "Decoding bounded media...";
            var result = await mediaImportService.ImportAsync(
                stream,
                displayName,
                kind,
                options,
                sampleInterval,
                cancellationToken).ConfigureAwait(false);
            if (result.Succeeded && result.Project is not null)
            {
                ApplyProject(result.Project);
                IsDirty = true;
            }

            StatusText = result.Message;
            return result;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task NewAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ApplyProject(AnimationProject.CreateBlank());
        IsDirty = true;
        StatusText = "New unsaved animation ready.";
        return Task.CompletedTask;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (!compilation.Succeeded)
        {
            StatusText = compilation.Message;
            return;
        }

        if (storeState.UsedFallback)
        {
            StatusText = "Unreadable project data cannot be overwritten. Recover or reset the exact animation-project store first.";
            return;
        }

        IsBusy = true;
        try
        {
            var project = CurrentProject.Normalize() with { UpdatedAt = DateTimeOffset.UtcNow };
            storeState = storeState with
            {
                Projects = storeState.Projects
                    .Where(item => !string.Equals(item.Id, project.Id, StringComparison.Ordinal))
                    .Append(project)
                    .ToArray()
            };
            await store.SaveAsync(storeState, cancellationToken).ConfigureAwait(false);
            storeState = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            Projects = storeState.Projects;
            ApplyProject(Projects.First(item => item.Id == project.Id));
            IsDirty = false;
            StoreStatusText = $"Saved {project.DisplayName}.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteProjectAsync(CancellationToken cancellationToken)
    {
        if (!Projects.Any(project => project.Id == CurrentProject.Id))
        {
            StatusText = "This draft has not been saved.";
            return;
        }

        if (storeState.UsedFallback)
        {
            StatusText = "Unreadable project data cannot be overwritten.";
            return;
        }

        IsBusy = true;
        try
        {
            storeState = storeState with
            {
                Projects = storeState.Projects.Where(project => project.Id != CurrentProject.Id).ToArray()
            };
            await store.SaveAsync(storeState, cancellationToken).ConfigureAwait(false);
            Projects = storeState.Normalize().Projects;
            ApplyProject(Projects.FirstOrDefault() ?? AnimationProject.CreateBlank());
            StoreStatusText = "Animation project deleted.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PrepareAsync(CancellationToken cancellationToken)
    {
        if (compilation.Animation is null)
        {
            StatusText = compilation.Message;
            return;
        }

        IsBusy = true;
        try
        {
            var result = await playbackCoordinator.PrepareAnimationAsync(compilation.Animation, cancellationToken)
                .ConfigureAwait(false);
            StatusText = result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PreviewAsync(CancellationToken cancellationToken)
    {
        if (compilation.Animation is null)
        {
            StatusText = compilation.Message;
            return;
        }

        IsBusy = true;
        try
        {
            var result = await playbackCoordinator.PlayAnimationAsync(compilation.Animation, cancellationToken)
                .ConfigureAwait(false);
            StatusText = result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StopAsync(CancellationToken cancellationToken)
    {
        await playbackCoordinator.StopAnimationAsync(cancellationToken).ConfigureAwait(false);
        StatusText = "Animation stopped; previous stable look restored when available.";
    }

    private async Task AcknowledgeSafetyAsync(CancellationToken cancellationToken)
    {
        if (safetyAssessment is null || safetyAssessment.IsSafeByDefault)
        {
            return;
        }

        await acknowledgementService.AcknowledgeAsync(safetyAssessment, cancellationToken).ConfigureAwait(false);
        acknowledgementState = (await acknowledgementStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        RefreshSafety();
        StatusText = "Flash-risk override recorded for this exact animation revision. Change any frame or timing to invalidate it.";
    }

    private async Task RevokeSafetyAsync(CancellationToken cancellationToken)
    {
        await acknowledgementService.RevokeAsync(CurrentProject.Id, cancellationToken).ConfigureAwait(false);
        acknowledgementState = (await acknowledgementStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        RefreshSafety();
        StatusText = "Flash-risk override revoked.";
    }

    private void ApplyProject(AnimationProject project)
    {
        CurrentProject = project.Normalize();
        selectedFrameIndex = 0;
        OnPropertyChanged(nameof(SelectedFrameIndex));
        OnPropertyChanged(nameof(SelectedFrame));
        OnPropertyChanged(nameof(SelectedFrameDurationMilliseconds));
        OnPropertyChanged(nameof(OnionSkinPattern));
        undoHistory.Clear();
        redoHistory.Clear();
        editTransactionStart = null;
        ClearSelection();
        IsDirty = false;
        NotifyHistoryChanged();
        RefreshDerivedState();
    }

    private void ReplaceSelectedPattern(FacePattern pattern)
    {
        var frames = CurrentProject.Frames.ToArray();
        frames[SelectedFrameIndex] = SelectedFrame with
        {
            Pattern = pattern.Normalize(),
        };
        ReplaceFrames(frames, SelectedFrameIndex, "Frame pixels updated.", preserveSelection: true);
    }

    private void ReplaceFrames(
        IReadOnlyList<AnimationProjectFrame> frames,
        int selectedIndex,
        string message,
        bool preserveSelection = false)
    {
        CurrentProject = CurrentProject with
        {
            Frames = frames,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        IsDirty = true;
        if (!preserveSelection)
        {
            ClearSelection();
        }
        selectedFrameIndex = Math.Clamp(selectedIndex, 0, frames.Count - 1);
        OnPropertyChanged(nameof(SelectedFrameIndex));
        OnPropertyChanged(nameof(SelectedFrame));
        OnPropertyChanged(nameof(SelectedFrameDurationMilliseconds));
        OnPropertyChanged(nameof(OnionSkinPattern));
        StatusText = message;
        RefreshDerivedState();
    }

    private void RefreshDerivedState()
    {
        compilation = compiler.Compile(CurrentProject);
        BudgetText = compilation.Succeeded
            ? $"{compilation.UniqueFrameCount}/{compilation.SlotBudget} unique DIY slots · {compilation.SourceFrameCount} timeline frames"
            : compilation.Message;
        RefreshSafety();
        RefreshTimeline();
        RefreshPreview();
        RaiseCommandStates();
    }

    private void RefreshSafety()
    {
        if (!compilation.Succeeded || compilation.Animation is null)
        {
            safetyAssessment = null;
            SafetyStatus = FlashSafetyStatus.Blocked;
            SafetyStatusText = "Safety analysis unavailable until the slot budget and frame data are valid.";
            return;
        }

        safetyAssessment = flashSafetyAnalyzer.Analyze(compilation.Animation);
        var decision = flashSafetyAnalyzer.Decide(safetyAssessment, acknowledgementState);
        SafetyStatus = decision.Status;
        SafetyStatusText = decision.Message;
    }

    private void RefreshTimeline()
    {
        TimelineFrames = CurrentProject.Frames
            .Select((frame, index) => new AnimationTimelineFrame(
                index,
                frame.Id,
                $"Frame {index + 1}",
                $"{frame.Duration.TotalMilliseconds:0} ms",
                frame.Pattern,
                index == SelectedFrameIndex))
            .ToArray();
    }

    private void RefreshPreview()
    {
        var pattern = SelectedFrame.Pattern.Normalize();
        PreviewCells = Enumerable.Range(0, FacePattern.PixelCount)
            .Select(index =>
            {
                var pixel = pattern.Pixels[index];
                return new FacePreviewCell(
                    index % FacePattern.Width,
                    index / FacePattern.Width,
                    pixel.IsLit,
                    pixel.IsLit ? pixel.Color.Hex : OffColor);
            })
            .ToArray();
        OnPropertyChanged(nameof(OnionSkinPattern));
    }

    private bool CanSave() => !IsBusy && compilation.Succeeded && !storeState.UsedFallback;

    private bool CanUseCompiledAnimation() => !IsBusy && compilation.Succeeded;

    private bool CanAcknowledgeSafety() => !IsBusy && IsSafetyBlocked && safetyAssessment is not null;

    private bool CanRevokeSafety() => !IsBusy && HasSafetyOverride;

    private void RaiseCommandStates()
    {
        NewCommand.RaiseCanExecuteChanged();
        SaveCommand.RaiseCanExecuteChanged();
        DeleteProjectCommand.RaiseCanExecuteChanged();
        PrepareCommand.RaiseCanExecuteChanged();
        PreviewCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
        AcknowledgeSafetyCommand.RaiseCanExecuteChanged();
        RevokeSafetyCommand.RaiseCanExecuteChanged();
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
    }

    private AsyncRelayCommand CreateToolCommand(AnimationEditorTool tool) => new(_ =>
    {
        SelectedTool = tool;
        return Task.CompletedTask;
    });

    private void SetCommandError(Exception exception) =>
        StatusText = $"Failed: {ShortMessage(exception)}";

    private static TimeSpan ClampFrameDuration(TimeSpan duration) =>
        duration < PerformanceAnimation.MinFrameDuration
            ? PerformanceAnimation.MinFrameDuration
            : duration > PerformanceAnimation.MaxFrameDuration
                ? PerformanceAnimation.MaxFrameDuration
                : duration;

    private static string ShortMessage(Exception exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;
        return message.Length <= 160 ? message : string.Concat(message.AsSpan(0, 160), "...");
    }

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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed record EditorSnapshot(AnimationProject Project, int SelectedFrameIndex);
}
