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

public sealed class AnimationStudioViewModel : INotifyPropertyChanged
{
    private const string OffColor = "#05070D";

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
        var pixel = IsEraseMode ? FacePixel.Off : new FacePixel(true, SelectedColor.Color);
        ReplaceSelectedPattern(SelectedFrame.Pattern.WithPixel(column, row, pixel));
    }

    public void ClearSelectedFrame()
    {
        ReplaceSelectedPattern(SelectedFrame.Pattern with
        {
            Pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray()
        });
        StatusText = "Selected frame cleared.";
    }

    public void MirrorSelectedFrameHorizontally()
    {
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
        var frames = CurrentProject.Frames.ToArray();
        frames[SelectedFrameIndex] = SelectedFrame with { Duration = normalized };
        ReplaceFrames(frames, SelectedFrameIndex, $"Frame duration {normalized.TotalMilliseconds:0} ms.");
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
        RefreshDerivedState();
    }

    private void ReplaceSelectedPattern(FacePattern pattern)
    {
        var frames = CurrentProject.Frames.ToArray();
        frames[SelectedFrameIndex] = SelectedFrame with
        {
            Pattern = pattern.Normalize(),
        };
        ReplaceFrames(frames, SelectedFrameIndex, "Frame pixels updated.");
    }

    private void ReplaceFrames(
        IReadOnlyList<AnimationProjectFrame> frames,
        int selectedIndex,
        string message)
    {
        CurrentProject = CurrentProject with
        {
            Frames = frames,
            UpdatedAt = DateTimeOffset.UtcNow
        };
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
    }

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
}
