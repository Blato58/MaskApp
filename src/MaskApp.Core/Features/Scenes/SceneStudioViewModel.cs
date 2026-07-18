using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Gallery;

namespace MaskApp.Core.Features.Scenes;

public sealed record SceneStepRow(
    int Index,
    PerformanceSceneStep Step,
    string Title,
    string Summary,
    bool IsSelected)
{
    public int DisplayIndex => Index + 1;
}

public sealed record SetlistCueRow(
    int Index,
    PerformanceSetlistCue Cue,
    string SceneName,
    bool IsSelected)
{
    public int DisplayIndex => Index + 1;
}

public sealed class SceneStudioViewModel : INotifyPropertyChanged
{
    private readonly ISceneShowStore store;
    private readonly ISceneCatalogSource catalogSource;
    private readonly SceneValidator validator;
    private readonly SceneExecutionEngine executionEngine;
    private readonly SetlistCoordinator setlistCoordinator;
    private SceneShowState state = new();
    private IReadOnlyList<GalleryItem> catalog = [];
    private PerformanceScene currentScene = PerformanceScene.CreateBlank();
    private PerformanceSetlist currentSetlist = PerformanceSetlist.CreateBlank();
    private IReadOnlyList<PerformanceScene> scenes = [];
    private IReadOnlyList<PerformanceSetlist> setlists = [];
    private IReadOnlyList<SceneStepRow> stepRows = [];
    private IReadOnlyList<SetlistCueRow> cueRows = [];
    private IReadOnlyList<GalleryItem> contentOptions = [];
    private IReadOnlyList<SceneStepRow> repeatTargets = [];
    private IReadOnlyList<SceneValidationIssue> validationIssues = [];
    private int selectedStepIndex;
    private int selectedCueIndex;
    private SceneStepKind newStepKind = SceneStepKind.Face;
    private GalleryItem? selectedContentItem;
    private SceneStepRow? selectedRepeatTarget;
    private PerformanceScene? selectedCueScene;
    private int stepValue = 60;
    private double stepDurationMilliseconds = 1000;
    private int repeatCount = 2;
    private bool isBusy;
    private string statusText = "Scene Studio ready.";
    private string setlistPositionText = "No active setlist.";

    public SceneStudioViewModel(
        ISceneShowStore store,
        ISceneCatalogSource catalogSource,
        SceneValidator validator,
        SceneExecutionEngine executionEngine,
        SetlistCoordinator setlistCoordinator)
    {
        this.store = store;
        this.catalogSource = catalogSource;
        this.validator = validator;
        this.executionEngine = executionEngine;
        this.setlistCoordinator = setlistCoordinator;

        NewSceneCommand = new AsyncRelayCommand(NewSceneAsync, () => !IsBusy, SetCommandError);
        SaveSceneCommand = new AsyncRelayCommand(SaveSceneAsync, () => !IsBusy && IsSceneValid, SetCommandError);
        DuplicateSceneCommand = new AsyncRelayCommand(DuplicateSceneAsync, () => !IsBusy, SetCommandError);
        DeleteSceneCommand = new AsyncRelayCommand(DeleteSceneAsync, () => !IsBusy, SetCommandError);
        RehearseSceneCommand = new AsyncRelayCommand(RehearseSceneAsync, () => !IsBusy && IsSceneValid, SetCommandError);
        StopCommand = new AsyncRelayCommand(StopAsync, () => !IsBusy, SetCommandError);
        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync, () => !IsBusy, SetCommandError);
        NewSetlistCommand = new AsyncRelayCommand(NewSetlistAsync, () => !IsBusy, SetCommandError);
        SaveSetlistCommand = new AsyncRelayCommand(SaveSetlistAsync, () => !IsBusy, SetCommandError);
        DuplicateSetlistCommand = new AsyncRelayCommand(DuplicateSetlistAsync, () => !IsBusy, SetCommandError);
        DeleteSetlistCommand = new AsyncRelayCommand(DeleteSetlistAsync, () => !IsBusy, SetCommandError);
        ActivateSetlistCommand = new AsyncRelayCommand(ActivateSetlistAsync, () => !IsBusy, SetCommandError);
        UsePagesCommand = new AsyncRelayCommand(UsePagesAsync, () => !IsBusy, SetCommandError);
        PreviousCueCommand = new AsyncRelayCommand(PreviousCueAsync, () => !IsBusy, SetCommandError);
        NextCueCommand = new AsyncRelayCommand(NextCueAsync, () => !IsBusy, SetCommandError);
        TriggerCueCommand = new AsyncRelayCommand(TriggerCueAsync, () => !IsBusy, SetCommandError);
        RefreshDerivedState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<SceneStepKind> StepKinds { get; } = Enum.GetValues<SceneStepKind>();

    public IReadOnlyList<SceneFailurePolicy> FailurePolicies { get; } = Enum.GetValues<SceneFailurePolicy>();

    public AsyncRelayCommand NewSceneCommand { get; }
    public AsyncRelayCommand SaveSceneCommand { get; }
    public AsyncRelayCommand DuplicateSceneCommand { get; }
    public AsyncRelayCommand DeleteSceneCommand { get; }
    public AsyncRelayCommand RehearseSceneCommand { get; }
    public AsyncRelayCommand StopCommand { get; }
    public AsyncRelayCommand BlackoutCommand { get; }
    public AsyncRelayCommand NewSetlistCommand { get; }
    public AsyncRelayCommand SaveSetlistCommand { get; }
    public AsyncRelayCommand DuplicateSetlistCommand { get; }
    public AsyncRelayCommand DeleteSetlistCommand { get; }
    public AsyncRelayCommand ActivateSetlistCommand { get; }
    public AsyncRelayCommand UsePagesCommand { get; }
    public AsyncRelayCommand PreviousCueCommand { get; }
    public AsyncRelayCommand NextCueCommand { get; }
    public AsyncRelayCommand TriggerCueCommand { get; }

    public IReadOnlyList<PerformanceScene> Scenes
    {
        get => scenes;
        private set => SetField(ref scenes, value);
    }

    public IReadOnlyList<PerformanceSetlist> Setlists
    {
        get => setlists;
        private set => SetField(ref setlists, value);
    }

    public PerformanceScene CurrentScene
    {
        get => currentScene;
        private set
        {
            if (SetField(ref currentScene, value))
            {
                OnPropertyChanged(nameof(SceneName));
                OnPropertyChanged(nameof(SceneColorHex));
                OnPropertyChanged(nameof(FailurePolicy));
                RefreshDerivedState();
            }
        }
    }

    public string SceneName
    {
        get => CurrentScene.DisplayName;
        set => UpdateScene(CurrentScene with { DisplayName = value ?? string.Empty });
    }

    public string SceneColorHex
    {
        get => CurrentScene.ColorHex;
        set => UpdateScene(CurrentScene with { ColorHex = value ?? string.Empty });
    }

    public SceneFailurePolicy FailurePolicy
    {
        get => CurrentScene.FailurePolicy;
        set => UpdateScene(CurrentScene with { FailurePolicy = value });
    }

    public IReadOnlyList<SceneStepRow> StepRows
    {
        get => stepRows;
        private set => SetField(ref stepRows, value);
    }

    public IReadOnlyList<SceneStepRow> RepeatTargets
    {
        get => repeatTargets;
        private set => SetField(ref repeatTargets, value);
    }

    public int SelectedStepIndex
    {
        get => selectedStepIndex;
        set
        {
            var normalized = Math.Clamp(value, 0, Math.Max(0, CurrentScene.Steps.Count - 1));
            if (SetField(ref selectedStepIndex, normalized))
            {
                RefreshStepRows();
            }
        }
    }

    public SceneStepKind NewStepKind
    {
        get => newStepKind;
        set
        {
            if (SetField(ref newStepKind, value))
            {
                RefreshContentOptions();
                OnPropertyChanged(nameof(NeedsContentItem));
                OnPropertyChanged(nameof(NeedsValue));
                OnPropertyChanged(nameof(NeedsDuration));
                OnPropertyChanged(nameof(NeedsRepeat));
            }
        }
    }

    public IReadOnlyList<GalleryItem> ContentOptions
    {
        get => contentOptions;
        private set => SetField(ref contentOptions, value);
    }

    public GalleryItem? SelectedContentItem
    {
        get => selectedContentItem;
        set => SetField(ref selectedContentItem, value);
    }

    public SceneStepRow? SelectedRepeatTarget
    {
        get => selectedRepeatTarget;
        set => SetField(ref selectedRepeatTarget, value);
    }

    public bool NeedsContentItem => NewStepKind is SceneStepKind.Face or SceneStepKind.Text or SceneStepKind.Animation;

    public bool NeedsValue => NewStepKind is SceneStepKind.Brightness or SceneStepKind.AnimationSpeed;

    public bool NeedsDuration => NewStepKind == SceneStepKind.Wait;

    public bool NeedsRepeat => NewStepKind == SceneStepKind.Repeat;

    public int StepValue
    {
        get => stepValue;
        set => SetField(ref stepValue, Math.Clamp(value, 1, 100));
    }

    public double StepDurationMilliseconds
    {
        get => stepDurationMilliseconds;
        set => SetField(
            ref stepDurationMilliseconds,
            Math.Clamp(value, PerformanceScene.MinWaitDuration.TotalMilliseconds, PerformanceScene.MaxWaitDuration.TotalMilliseconds));
    }

    public int RepeatCount
    {
        get => repeatCount;
        set => SetField(ref repeatCount, Math.Clamp(value, 2, PerformanceScene.MaxRepeatCount));
    }

    public IReadOnlyList<SceneValidationIssue> ValidationIssues
    {
        get => validationIssues;
        private set => SetField(ref validationIssues, value);
    }

    public bool IsSceneValid => ValidationIssues.All(issue => issue.Severity != SceneValidationSeverity.Blocking);

    public string ValidationSummary => IsSceneValid
        ? $"VALID · {StepRows.Count} source / {validator.Validate(CurrentScene, CatalogById()).ExpandedStepCount} executed steps"
        : $"BLOCKED · {ValidationIssues.Count(issue => issue.Severity == SceneValidationSeverity.Blocking)} issue(s)";

    public PerformanceSetlist CurrentSetlist
    {
        get => currentSetlist;
        private set
        {
            if (SetField(ref currentSetlist, value))
            {
                OnPropertyChanged(nameof(SetlistName));
                RefreshCueRows();
            }
        }
    }

    public string SetlistName
    {
        get => CurrentSetlist.DisplayName;
        set => CurrentSetlist = CurrentSetlist with { DisplayName = value ?? string.Empty, UpdatedAt = DateTimeOffset.UtcNow };
    }

    public IReadOnlyList<SetlistCueRow> CueRows
    {
        get => cueRows;
        private set => SetField(ref cueRows, value);
    }

    public int SelectedCueIndex
    {
        get => selectedCueIndex;
        set
        {
            var normalized = Math.Clamp(value, 0, Math.Max(0, CurrentSetlist.Cues.Count - 1));
            if (SetField(ref selectedCueIndex, normalized))
            {
                RefreshCueRows();
            }
        }
    }

    public PerformanceScene? SelectedCueScene
    {
        get => selectedCueScene;
        set => SetField(ref selectedCueScene, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                RefreshCommands();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string SetlistPositionText
    {
        get => setlistPositionText;
        private set => SetField(ref setlistPositionText, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        try
        {
            await ReloadAsync(CurrentScene.Id, CurrentSetlist.Id, cancellationToken).ConfigureAwait(false);
            var snapshot = await setlistCoordinator.InitializeAsync(cancellationToken).ConfigureAwait(false);
            ApplySetlistPosition(snapshot);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SelectScene(string sceneId)
    {
        var scene = Scenes.FirstOrDefault(item => item.Id == sceneId);
        if (scene is not null)
        {
            SelectedStepIndex = 0;
            CurrentScene = scene;
            StatusText = $"Editing {scene.DisplayName}.";
        }
    }

    public void SelectSetlist(string setlistId)
    {
        var setlist = Setlists.FirstOrDefault(item => item.Id == setlistId);
        if (setlist is not null)
        {
            SelectedCueIndex = 0;
            CurrentSetlist = setlist;
            StatusText = $"Editing {setlist.DisplayName}.";
        }
    }

    public bool AddStep()
    {
        if (CurrentScene.Steps.Count >= PerformanceScene.MaxSteps)
        {
            StatusText = $"A Scene is limited to {PerformanceScene.MaxSteps} source steps.";
            return false;
        }

        if (NeedsContentItem && SelectedContentItem is null)
        {
            StatusText = $"Choose a compatible Library item for the {NewStepKind} step.";
            return false;
        }

        if (NeedsRepeat && SelectedRepeatTarget is null)
        {
            StatusText = "Choose the first earlier step for this finite Repeat.";
            return false;
        }

        var step = new PerformanceSceneStep
        {
            Id = $"step-{Guid.NewGuid():N}",
            Kind = NewStepKind,
            GalleryItemId = SelectedContentItem?.Id ?? string.Empty,
            Value = StepValue,
            Duration = TimeSpan.FromMilliseconds(StepDurationMilliseconds),
            RepeatFromStepId = SelectedRepeatTarget?.Step.Id ?? string.Empty,
            RepeatCount = RepeatCount
        };
        var insertIndex = Math.Min(SelectedStepIndex + 1, CurrentScene.Steps.Count);
        var steps = CurrentScene.Steps.ToList();
        steps.Insert(insertIndex, step);
        UpdateScene(CurrentScene with { Steps = steps });
        SelectedStepIndex = insertIndex;
        StatusText = $"Added {NewStepKind}. Save after validation passes.";
        return true;
    }

    public bool DuplicateSelectedStep()
    {
        if (CurrentScene.Steps.Count >= PerformanceScene.MaxSteps || CurrentScene.Steps.Count == 0)
        {
            return false;
        }

        var source = CurrentScene.Steps[SelectedStepIndex];
        var copy = source with { Id = $"step-{Guid.NewGuid():N}" };
        var steps = CurrentScene.Steps.ToList();
        steps.Insert(SelectedStepIndex + 1, copy);
        UpdateScene(CurrentScene with { Steps = steps });
        SelectedStepIndex++;
        StatusText = "Step duplicated.";
        return true;
    }

    public bool DeleteSelectedStep()
    {
        if (CurrentScene.Steps.Count <= 1)
        {
            StatusText = "A Scene must keep at least one step.";
            return false;
        }

        var removedId = CurrentScene.Steps[SelectedStepIndex].Id;
        if (CurrentScene.Steps.Any(step => step.Kind == SceneStepKind.Repeat && step.RepeatFromStepId == removedId))
        {
            StatusText = "Move or delete the Repeat that targets this step first.";
            return false;
        }

        var steps = CurrentScene.Steps.Where((_, index) => index != SelectedStepIndex).ToArray();
        UpdateScene(CurrentScene with { Steps = steps });
        SelectedStepIndex = Math.Min(SelectedStepIndex, steps.Length - 1);
        StatusText = "Step deleted.";
        return true;
    }

    public bool MoveSelectedStep(int delta) => MoveStep(SelectedStepIndex, SelectedStepIndex + delta);

    public bool MoveStep(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= CurrentScene.Steps.Count
            || toIndex < 0 || toIndex >= CurrentScene.Steps.Count
            || fromIndex == toIndex)
        {
            return false;
        }

        var steps = CurrentScene.Steps.ToList();
        var step = steps[fromIndex];
        steps.RemoveAt(fromIndex);
        steps.Insert(toIndex, step);
        SelectedStepIndex = toIndex;
        UpdateScene(CurrentScene with { Steps = steps });
        StatusText = "Step order updated.";
        return true;
    }

    public bool AddCue()
    {
        if (SelectedCueScene is null)
        {
            StatusText = "Choose a Scene for the setlist cue.";
            return false;
        }

        if (CurrentSetlist.Cues.Count >= PerformanceSetlist.MaxCues)
        {
            StatusText = $"A setlist is limited to {PerformanceSetlist.MaxCues} cues.";
            return false;
        }

        var cue = new PerformanceSetlistCue
        {
            Id = $"cue-{Guid.NewGuid():N}",
            Label = SelectedCueScene.DisplayName,
            SceneId = SelectedCueScene.Id
        };
        var cues = CurrentSetlist.Cues.ToList();
        var insertIndex = cues.Count == 0 ? 0 : Math.Min(SelectedCueIndex + 1, cues.Count);
        cues.Insert(insertIndex, cue);
        CurrentSetlist = CurrentSetlist with { Cues = cues, UpdatedAt = DateTimeOffset.UtcNow };
        SelectedCueIndex = insertIndex;
        StatusText = "Cue added.";
        return true;
    }

    public bool DuplicateSelectedCue()
    {
        if (CurrentSetlist.Cues.Count == 0 || CurrentSetlist.Cues.Count >= PerformanceSetlist.MaxCues)
        {
            return false;
        }

        var copy = CurrentSetlist.Cues[SelectedCueIndex] with { Id = $"cue-{Guid.NewGuid():N}" };
        var cues = CurrentSetlist.Cues.ToList();
        cues.Insert(SelectedCueIndex + 1, copy);
        CurrentSetlist = CurrentSetlist with { Cues = cues, UpdatedAt = DateTimeOffset.UtcNow };
        SelectedCueIndex++;
        StatusText = "Cue duplicated.";
        return true;
    }

    public bool DeleteSelectedCue()
    {
        if (CurrentSetlist.Cues.Count == 0)
        {
            return false;
        }

        var cues = CurrentSetlist.Cues.Where((_, index) => index != SelectedCueIndex).ToArray();
        CurrentSetlist = CurrentSetlist with { Cues = cues, UpdatedAt = DateTimeOffset.UtcNow };
        SelectedCueIndex = Math.Clamp(SelectedCueIndex, 0, Math.Max(0, cues.Length - 1));
        StatusText = "Cue deleted.";
        return true;
    }

    public bool MoveSelectedCue(int delta) => MoveCue(SelectedCueIndex, SelectedCueIndex + delta);

    public bool MoveCue(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= CurrentSetlist.Cues.Count
            || toIndex < 0 || toIndex >= CurrentSetlist.Cues.Count
            || fromIndex == toIndex)
        {
            return false;
        }

        var cues = CurrentSetlist.Cues.ToList();
        var cue = cues[fromIndex];
        cues.RemoveAt(fromIndex);
        cues.Insert(toIndex, cue);
        SelectedCueIndex = toIndex;
        CurrentSetlist = CurrentSetlist with { Cues = cues, UpdatedAt = DateTimeOffset.UtcNow };
        StatusText = "Cue order updated.";
        return true;
    }

    private Task NewSceneAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SelectedStepIndex = 0;
        CurrentScene = PerformanceScene.CreateBlank();
        StatusText = "New unsaved Scene.";
        return Task.CompletedTask;
    }

    private async Task SaveSceneAsync(CancellationToken cancellationToken)
    {
        var normalized = CurrentScene.Normalize() with { UpdatedAt = DateTimeOffset.UtcNow };
        var validation = validator.Validate(normalized, CatalogById());
        if (!validation.IsValid)
        {
            StatusText = "Resolve every blocking Scene validation issue before saving.";
            RefreshValidation();
            return;
        }

        var latest = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        EnsureWritable(latest);
        var updated = latest.Scenes.Where(scene => scene.Id != normalized.Id).Append(normalized).ToArray();
        await store.SaveAsync(latest with { Scenes = updated }, cancellationToken).ConfigureAwait(false);
        await ReloadAsync(normalized.Id, CurrentSetlist.Id, cancellationToken).ConfigureAwait(false);
        StatusText = $"Saved {normalized.DisplayName}. It is available in Library and Pages.";
    }

    private async Task DuplicateSceneAsync(CancellationToken cancellationToken)
    {
        var source = CurrentScene.Normalize();
        if (!validator.Validate(source, CatalogById()).IsValid)
        {
            StatusText = "Resolve Scene validation before duplicating it.";
            return;
        }

        var idMap = source.Steps.ToDictionary(step => step.Id, _ => $"step-{Guid.NewGuid():N}", StringComparer.Ordinal);
        var copy = source with
        {
            Id = $"scene-{Guid.NewGuid():N}",
            DisplayName = $"{source.DisplayName} Copy",
            Steps = source.Steps.Select(step => step with
            {
                Id = idMap[step.Id],
                RepeatFromStepId = string.IsNullOrWhiteSpace(step.RepeatFromStepId)
                    ? string.Empty
                    : idMap[step.RepeatFromStepId]
            }).ToArray(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        CurrentScene = copy;
        await SaveSceneAsync(cancellationToken).ConfigureAwait(false);
        StatusText = $"Duplicated as {copy.DisplayName}.";
    }

    private async Task DeleteSceneAsync(CancellationToken cancellationToken)
    {
        var latest = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        EnsureWritable(latest);
        if (latest.Setlists.Any(setlist => setlist.Cues.Any(cue => cue.SceneId == CurrentScene.Id)))
        {
            StatusText = "This Scene is used by a setlist. Remove those cues before deleting it.";
            return;
        }

        var remaining = latest.Scenes.Where(scene => scene.Id != CurrentScene.Id).ToArray();
        await store.SaveAsync(latest with { Scenes = remaining }, cancellationToken).ConfigureAwait(false);
        CurrentScene = remaining.FirstOrDefault() ?? PerformanceScene.CreateBlank();
        await ReloadAsync(CurrentScene.Id, CurrentSetlist.Id, cancellationToken).ConfigureAwait(false);
        StatusText = "Scene deleted.";
    }

    private async Task RehearseSceneAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            var result = await executionEngine.ExecuteAsync(CurrentScene, cancellationToken).ConfigureAwait(false);
            StatusText = result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StopAsync(CancellationToken cancellationToken)
    {
        var result = await executionEngine.StopAsync(cancellationToken).ConfigureAwait(false);
        StatusText = result.Message;
    }

    private async Task BlackoutAsync(CancellationToken cancellationToken)
    {
        var result = await executionEngine.BlackoutAsync(cancellationToken).ConfigureAwait(false);
        StatusText = result.Message;
    }

    private Task NewSetlistAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SelectedCueIndex = 0;
        CurrentSetlist = PerformanceSetlist.CreateBlank();
        StatusText = "New unsaved setlist.";
        return Task.CompletedTask;
    }

    private async Task SaveSetlistAsync(CancellationToken cancellationToken)
    {
        var normalized = CurrentSetlist.Normalize() with { UpdatedAt = DateTimeOffset.UtcNow };
        var latest = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        EnsureWritable(latest);
        var sceneMap = latest.Scenes.ToDictionary(scene => scene.Id, StringComparer.Ordinal);
        var issues = validator.ValidateSetlist(normalized, sceneMap);
        if (issues.Any(issue => issue.Severity == SceneValidationSeverity.Blocking))
        {
            StatusText = string.Join(" ", issues.Select(issue => issue.Message));
            return;
        }

        var updated = latest.Setlists.Where(setlist => setlist.Id != normalized.Id).Append(normalized).ToArray();
        await store.SaveAsync(latest with { Setlists = updated }, cancellationToken).ConfigureAwait(false);
        await ReloadAsync(CurrentScene.Id, normalized.Id, cancellationToken).ConfigureAwait(false);
        StatusText = $"Saved setlist {normalized.DisplayName}.";
    }

    private async Task DuplicateSetlistAsync(CancellationToken cancellationToken)
    {
        var source = CurrentSetlist.Normalize();
        var copy = source with
        {
            Id = $"setlist-{Guid.NewGuid():N}",
            DisplayName = $"{source.DisplayName} Copy",
            Cues = source.Cues.Select(cue => cue with { Id = $"cue-{Guid.NewGuid():N}" }).ToArray(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        CurrentSetlist = copy;
        await SaveSetlistAsync(cancellationToken).ConfigureAwait(false);
        StatusText = $"Duplicated as {copy.DisplayName}.";
    }

    private async Task DeleteSetlistAsync(CancellationToken cancellationToken)
    {
        var latest = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        EnsureWritable(latest);
        var remaining = latest.Setlists.Where(setlist => setlist.Id != CurrentSetlist.Id).ToArray();
        var activeId = latest.ActiveSetlistId == CurrentSetlist.Id ? string.Empty : latest.ActiveSetlistId;
        await store.SaveAsync(latest with
        {
            Setlists = remaining,
            ActiveSetlistId = activeId,
            Positions = latest.Positions.Where(position => position.SetlistId != CurrentSetlist.Id).ToArray()
        }, cancellationToken).ConfigureAwait(false);
        CurrentSetlist = remaining.FirstOrDefault() ?? PerformanceSetlist.CreateBlank();
        await ReloadAsync(CurrentScene.Id, CurrentSetlist.Id, cancellationToken).ConfigureAwait(false);
        StatusText = "Setlist deleted.";
    }

    private async Task ActivateSetlistAsync(CancellationToken cancellationToken)
    {
        await SaveSetlistAsync(cancellationToken).ConfigureAwait(false);
        var snapshot = await setlistCoordinator.ActivateAsync(CurrentSetlist.Id, cancellationToken).ConfigureAwait(false);
        ApplySetlistPosition(snapshot);
        StatusText = $"{snapshot.SetlistName} is now the Stage source.";
    }

    private async Task UsePagesAsync(CancellationToken cancellationToken)
    {
        var snapshot = await setlistCoordinator.UsePagesAsync(cancellationToken).ConfigureAwait(false);
        ApplySetlistPosition(snapshot);
        await ReloadAsync(CurrentScene.Id, CurrentSetlist.Id, cancellationToken).ConfigureAwait(false);
        StatusText = "Stage now uses prepared Pages.";
    }

    private async Task PreviousCueAsync(CancellationToken cancellationToken)
    {
        var snapshot = await EnsureCurrentSetlistActiveAsync(cancellationToken).ConfigureAwait(false);
        if (snapshot.HasSetlist)
        {
            snapshot = await setlistCoordinator.PreviousAsync(cancellationToken).ConfigureAwait(false);
        }

        ApplySetlistPosition(snapshot);
    }

    private async Task NextCueAsync(CancellationToken cancellationToken)
    {
        var snapshot = await EnsureCurrentSetlistActiveAsync(cancellationToken).ConfigureAwait(false);
        if (snapshot.HasSetlist)
        {
            snapshot = await setlistCoordinator.NextAsync(cancellationToken).ConfigureAwait(false);
        }

        ApplySetlistPosition(snapshot);
    }

    private async Task TriggerCueAsync(CancellationToken cancellationToken)
    {
        var snapshot = await EnsureCurrentSetlistActiveAsync(cancellationToken).ConfigureAwait(false);
        if (!snapshot.HasSetlist)
        {
            StatusText = "Save and activate a setlist before triggering its cue.";
            return;
        }

        var result = await setlistCoordinator.TriggerCurrentAsync(cancellationToken).ConfigureAwait(false);
        StatusText = result.Message;
    }

    private async Task<SetlistSnapshot> EnsureCurrentSetlistActiveAsync(CancellationToken cancellationToken)
    {
        await SaveSetlistAsync(cancellationToken).ConfigureAwait(false);
        var snapshot = await setlistCoordinator.InitializeAsync(cancellationToken).ConfigureAwait(false);
        return snapshot.SetlistId == CurrentSetlist.Id
            ? snapshot
            : await setlistCoordinator.ActivateAsync(CurrentSetlist.Id, cancellationToken).ConfigureAwait(false);
    }

    private async Task ReloadAsync(
        string selectedSceneId,
        string selectedSetlistId,
        CancellationToken cancellationToken)
    {
        state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        catalog = await catalogSource.LoadAsync(cancellationToken).ConfigureAwait(false);
        Scenes = state.Scenes;
        Setlists = state.Setlists;
        CurrentScene = Scenes.FirstOrDefault(scene => scene.Id == selectedSceneId)
            ?? Scenes.FirstOrDefault()
            ?? PerformanceScene.CreateBlank();
        CurrentSetlist = Setlists.FirstOrDefault(setlist => setlist.Id == selectedSetlistId)
            ?? Setlists.FirstOrDefault()
            ?? PerformanceSetlist.CreateBlank();
        SelectedCueScene = Scenes.FirstOrDefault();
        RefreshDerivedState();
    }

    private void UpdateScene(PerformanceScene value)
    {
        CurrentScene = value with { UpdatedAt = DateTimeOffset.UtcNow };
    }

    private void RefreshDerivedState()
    {
        RefreshStepRows();
        RefreshContentOptions();
        RefreshCueRows();
        RefreshValidation();
        RefreshCommands();
    }

    private void RefreshStepRows()
    {
        StepRows = CurrentScene.Steps.Select((step, index) => new SceneStepRow(
            index,
            step,
            $"{index + 1}. {step.Kind}",
            DescribeStep(step),
            index == SelectedStepIndex)).ToArray();
        RepeatTargets = StepRows.Where(row => row.Step.Kind != SceneStepKind.Repeat).ToArray();
        if (SelectedRepeatTarget is null || RepeatTargets.All(row => row.Step.Id != SelectedRepeatTarget.Step.Id))
        {
            SelectedRepeatTarget = RepeatTargets.FirstOrDefault();
        }
    }

    private void RefreshContentOptions()
    {
        ContentOptions = catalog.Where(item => NewStepKind switch
        {
            SceneStepKind.Face => item.Type is GalleryItemType.CustomStaticFace or GalleryItemType.BuiltInStaticImage,
            SceneStepKind.Text => item.Type == GalleryItemType.TextPreset,
            SceneStepKind.Animation => item.Type is GalleryItemType.BuiltInAnimation
                or GalleryItemType.AppBuiltInAnimation
                or GalleryItemType.CustomAnimation,
            _ => false
        }).ToArray();
        if (SelectedContentItem is null || ContentOptions.All(item => item.Id != SelectedContentItem.Id))
        {
            SelectedContentItem = ContentOptions.FirstOrDefault();
        }
    }

    private void RefreshCueRows()
    {
        var scenesById = Scenes.ToDictionary(scene => scene.Id, StringComparer.Ordinal);
        CueRows = CurrentSetlist.Cues.Select((cue, index) => new SetlistCueRow(
            index,
            cue,
            scenesById.GetValueOrDefault(cue.SceneId)?.DisplayName ?? $"Missing Scene {cue.SceneId}",
            index == SelectedCueIndex)).ToArray();
    }

    private void RefreshValidation()
    {
        ValidationIssues = validator.Validate(CurrentScene, CatalogById()).Issues;
        OnPropertyChanged(nameof(IsSceneValid));
        OnPropertyChanged(nameof(ValidationSummary));
    }

    private IReadOnlyDictionary<string, GalleryItem> CatalogById() =>
        catalog.Where(item => item.Type != GalleryItemType.Scene)
            .ToDictionary(item => item.Id, StringComparer.Ordinal);

    private void ApplySetlistPosition(SetlistSnapshot snapshot)
    {
        SetlistPositionText = snapshot.HasSetlist
            ? $"{snapshot.SetlistName} · {snapshot.PositionText} · {snapshot.CurrentCue?.Label ?? "no current cue"}"
            : "Pages are the active Stage source.";
    }

    private void RefreshCommands()
    {
        NewSceneCommand.RaiseCanExecuteChanged();
        SaveSceneCommand.RaiseCanExecuteChanged();
        DuplicateSceneCommand.RaiseCanExecuteChanged();
        DeleteSceneCommand.RaiseCanExecuteChanged();
        RehearseSceneCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
        BlackoutCommand.RaiseCanExecuteChanged();
        NewSetlistCommand.RaiseCanExecuteChanged();
        SaveSetlistCommand.RaiseCanExecuteChanged();
        DuplicateSetlistCommand.RaiseCanExecuteChanged();
        DeleteSetlistCommand.RaiseCanExecuteChanged();
        ActivateSetlistCommand.RaiseCanExecuteChanged();
        UsePagesCommand.RaiseCanExecuteChanged();
        PreviousCueCommand.RaiseCanExecuteChanged();
        NextCueCommand.RaiseCanExecuteChanged();
        TriggerCueCommand.RaiseCanExecuteChanged();
    }

    private void SetCommandError(Exception exception) =>
        StatusText = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;

    private static string DescribeStep(PerformanceSceneStep step) => step.Kind switch
    {
        SceneStepKind.Brightness or SceneStepKind.AnimationSpeed => $"{step.Value}%",
        SceneStepKind.Face or SceneStepKind.Text or SceneStepKind.Animation => step.GalleryItemId,
        SceneStepKind.Wait => $"{step.Duration.TotalMilliseconds:0} ms",
        SceneStepKind.Repeat => $"from {step.RepeatFromStepId} × {step.RepeatCount}",
        SceneStepKind.RestorePrevious => "Restore the prior visual from this execution",
        SceneStepKind.Stop => "Cancel later visual work and keep the stable look",
        SceneStepKind.Blackout => "Emergency blackout and terminate this Scene",
        _ => step.Kind.ToString()
    };

    private static void EnsureWritable(SceneShowState value)
    {
        if (value.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable Scene/setlist data cannot be overwritten.");
        }
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
