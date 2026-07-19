using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Preflight;

public sealed class FestivalPreflightViewModel : INotifyPropertyChanged
{
    private readonly ITextPresetStore textPresetStore;
    private readonly IBuiltInAssetArchiveStore builtInArchiveStore;
    private readonly IFacePatternStore facePatternStore;
    private readonly IGalleryLayoutStore galleryLayoutStore;
    private readonly MaskProfileSession profileSession;
    private readonly MaskBleScheduler scheduler;
    private readonly QuickActionCatalog quickActionCatalog;
    private readonly FestivalPreflightAnalyzer analyzer;
    private readonly FestivalShowPreparationService preparationService;
    private readonly IFlashSafetyAcknowledgementStore flashSafetyAcknowledgementStore;
    private readonly FlashSafetyAcknowledgementService flashSafetyAcknowledgementService;
    private readonly IAnimationProjectStore animationProjectStore;
    private readonly ISceneShowStore sceneShowStore;
    private readonly IBleDeviceConnection deviceConnection;
    private readonly PreflightStatusSession? statusSession;
    private PreflightPageOption? selectedPage;
    private bool isBusy;
    private string statusText = "NOT READY";
    private string statusColorHex = "#FF5C54";
    private string scopeText = "Whole show";
    private string summaryText = "Run Preflight after selecting the mask used for this show.";
    private string activeMaskText = "No active mask profile";
    private DateTimeOffset? lastRunAt;
    private FestivalPreflightReport? currentReport;
    private IReadOnlyList<GalleryItem> currentCatalog = [];
    private bool lastRunSelectedOnly;
    private bool lastRunActiveSetlist;
    private SceneShowState sceneState = new();
    private string preparationStatusText = "Run Preflight before preparation.";
    private MaskProfile? analyzedProfile;
    private BleConnectionState analyzedConnectionState = BleConnectionState.Disconnected;

    public FestivalPreflightViewModel(
        ITextPresetStore textPresetStore,
        IBuiltInAssetArchiveStore builtInArchiveStore,
        IFacePatternStore facePatternStore,
        IGalleryLayoutStore galleryLayoutStore,
        MaskProfileSession profileSession,
        MaskBleScheduler scheduler,
        QuickActionCatalog quickActionCatalog,
        FestivalPreflightAnalyzer analyzer,
        FestivalShowPreparationService preparationService,
        IFlashSafetyAcknowledgementStore flashSafetyAcknowledgementStore,
        FlashSafetyAcknowledgementService flashSafetyAcknowledgementService,
        IBleDeviceConnection deviceConnection,
        IAnimationProjectStore? animationProjectStore = null,
        ISceneShowStore? sceneShowStore = null,
        PreflightStatusSession? statusSession = null)
    {
        this.textPresetStore = textPresetStore;
        this.builtInArchiveStore = builtInArchiveStore;
        this.facePatternStore = facePatternStore;
        this.galleryLayoutStore = galleryLayoutStore;
        this.profileSession = profileSession;
        this.scheduler = scheduler;
        this.quickActionCatalog = quickActionCatalog;
        this.analyzer = analyzer;
        this.preparationService = preparationService;
        this.flashSafetyAcknowledgementStore = flashSafetyAcknowledgementStore;
        this.flashSafetyAcknowledgementService = flashSafetyAcknowledgementService;
        this.deviceConnection = deviceConnection;
        this.animationProjectStore = animationProjectStore ?? new InMemoryAnimationProjectStore();
        this.sceneShowStore = sceneShowStore ?? new InMemorySceneShowStore();
        this.statusSession = statusSession;

        RunSelectedPageCommand = new AsyncRelayCommand(
            cancellationToken => AnalyzeAsync(selectedOnly: true, cancellationToken),
            () => !IsBusy && SelectedPage is not null);
        RunWholeShowCommand = new AsyncRelayCommand(
            cancellationToken => AnalyzeAsync(selectedOnly: false, cancellationToken),
            () => !IsBusy);
        RunActiveSetlistCommand = new AsyncRelayCommand(
            cancellationToken => AnalyzeAsync(selectedOnly: false, cancellationToken, activeSetlist: true),
            () => !IsBusy && HasActiveSetlist);
        PrepareDiyContentCommand = new AsyncRelayCommand(
            PrepareDiyContentAsync,
            () => !IsBusy
                && currentReport is not null
                && currentReport.Status != FestivalPreflightStatus.NotReady
                && currentReport.SlotAllocations.Any(allocation => !allocation.IsPrepared));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PreflightPageOption> Pages { get; } = [];

    public ObservableCollection<PreflightIssue> Issues { get; } = [];

    public ObservableCollection<PreflightIssue> Blockers { get; } = [];

    public ObservableCollection<PreflightIssue> Warnings { get; } = [];

    public ObservableCollection<PreflightVerifiedCheck> VerifiedChecks { get; } = [];

    public ObservableCollection<PreflightActionAssessment> Actions { get; } = [];

    public bool HasNoIssues => Issues.Count == 0;

    public bool HasIssues => Issues.Count > 0;

    public bool HasBlockers => Blockers.Count > 0;

    public bool HasWarnings => Warnings.Count > 0;

    public bool HasVerifiedChecks => VerifiedChecks.Count > 0;

    public int BlockingIssueCount => Issues.Count(issue => issue.Severity == PreflightIssueSeverity.Blocking);

    public int WarningIssueCount => Issues.Count(issue => issue.Severity == PreflightIssueSeverity.Warning);

    public string InstantMetricText => (currentReport?.InstantActionCount ?? 0).ToString();

    public string PreparedMetricText => (currentReport?.PreparedActionCount ?? 0).ToString();

    public string UploadMetricText => (currentReport?.UploadRequiredActionCount ?? 0).ToString();

    public string UnverifiedMetricText => (currentReport?.UnverifiedActionCount ?? 0).ToString();

    public string SlotUsageMetricText => currentReport is null || analyzedProfile is null
        ? "Unavailable"
        : $"{currentReport.SlotAllocations.Select(allocation => allocation.AssignedSlot).Distinct().Count()} / {analyzedProfile.Capabilities.DiySlotCapacity}";

    public string LatencyMetricText => analyzedProfile?.AverageCommandLatencyMilliseconds is { } latency
        ? $"{latency:0.#} ms measured"
        : "Unavailable (not measured)";

    public string AcknowledgementMetricText => analyzedProfile is null
        ? "Unavailable"
        : analyzedProfile.Capabilities.AcknowledgementMode.ToString();

    public string ConnectionMetricText => analyzedConnectionState == BleConnectionState.Connected
        ? "Connected now"
        : analyzedConnectionState.ToString();

    public string SafetyMetricText => currentReport is null ? "Not checked" : FlashSafetySummary;

    public string ReadinessIcon => currentReport?.Status switch
    {
        FestivalPreflightStatus.ShowReady => "✓",
        FestivalPreflightStatus.Degraded => "!",
        _ => "×"
    };

    public bool HasBlockedFlashSafety => currentReport?.FlashSafetyResults.Any(result =>
        result.Decision.Status == FlashSafetyStatus.Blocked) == true;

    public bool HasAcknowledgedFlashSafety => currentReport?.FlashSafetyResults.Any(result =>
        result.Decision.Status == FlashSafetyStatus.AcknowledgedOverride) == true;

    public string FlashSafetySummary
    {
        get
        {
            var results = currentReport?.FlashSafetyResults ?? [];
            if (results.Count == 0)
            {
                return "No timed DIY animations in this scope.";
            }

            var blocked = results.Count(result => result.Decision.Status == FlashSafetyStatus.Blocked);
            var overridden = results.Count(result => result.Decision.Status == FlashSafetyStatus.AcknowledgedOverride);
            var safe = results.Count - blocked - overridden;
            return $"{safe} safe · {blocked} blocked · {overridden} explicit override(s)";
        }
    }

    public AsyncRelayCommand RunSelectedPageCommand { get; }

    public AsyncRelayCommand RunWholeShowCommand { get; }

    public AsyncRelayCommand RunActiveSetlistCommand { get; }

    public AsyncRelayCommand PrepareDiyContentCommand { get; }

    public PreflightPageOption? SelectedPage
    {
        get => selectedPage;
        set
        {
            if (SetField(ref selectedPage, value))
            {
                RunSelectedPageCommand.RaiseCanExecuteChanged();
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
                RunSelectedPageCommand.RaiseCanExecuteChanged();
                RunWholeShowCommand.RaiseCanExecuteChanged();
                RunActiveSetlistCommand.RaiseCanExecuteChanged();
                PrepareDiyContentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string StatusColorHex
    {
        get => statusColorHex;
        private set => SetField(ref statusColorHex, value);
    }

    public string ScopeText
    {
        get => scopeText;
        private set => SetField(ref scopeText, value);
    }

    public string SummaryText
    {
        get => summaryText;
        private set => SetField(ref summaryText, value);
    }

    public string ActiveMaskText
    {
        get => activeMaskText;
        private set => SetField(ref activeMaskText, value);
    }

    public string PreparationStatusText
    {
        get => preparationStatusText;
        private set => SetField(ref preparationStatusText, value);
    }

    public FestivalPreflightReport? CurrentReport => currentReport;

    public bool CanEnterStage => currentReport?.Status is FestivalPreflightStatus.ShowReady or FestivalPreflightStatus.Degraded;

    public bool HasActiveSetlist => !string.IsNullOrWhiteSpace(sceneState.ActiveSetlistId)
        && sceneState.Setlists.Any(setlist => setlist.Id == sceneState.ActiveSetlistId);

    public string ActiveSetlistText => HasActiveSetlist
        ? $"Check active setlist: {sceneState.Setlists.First(setlist => setlist.Id == sceneState.ActiveSetlistId).DisplayName}"
        : "No active setlist; Stage uses Pages";

    public string LastRunText => LastRunAt is null
        ? "Not run yet"
        : $"Checked {LastRunAt.Value.LocalDateTime:g}";

    public DateTimeOffset? LastRunAt
    {
        get => lastRunAt;
        private set
        {
            if (SetField(ref lastRunAt, value))
            {
                OnPropertyChanged(nameof(LastRunText));
            }
        }
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        InitializeAsync("whole-show", null, cancellationToken);

    public async Task InitializeAsync(
        string scope,
        string? sourceId,
        CancellationToken cancellationToken = default)
    {
        var layout = (await galleryLayoutStore.LoadAsync(cancellationToken)).Normalize();
        sceneState = (await sceneShowStore.LoadAsync(cancellationToken)).Normalize();
        OnPropertyChanged(nameof(HasActiveSetlist));
        OnPropertyChanged(nameof(ActiveSetlistText));
        RunActiveSetlistCommand.RaiseCanExecuteChanged();
        Pages.Clear();
        foreach (var page in layout.Pages)
        {
            Pages.Add(new PreflightPageOption(page.PageId, page.Title));
        }

        SelectedPage = !string.IsNullOrWhiteSpace(sourceId)
            ? Pages.FirstOrDefault(page => page.PageId == sourceId) ?? SelectedPage
            : SelectedPage;
        SelectedPage ??= Pages.FirstOrDefault();
        var normalizedScope = scope?.Trim().ToLowerInvariant();
        await AnalyzeAsync(
            selectedOnly: normalizedScope == "live-deck",
            cancellationToken,
            activeSetlist: normalizedScope == "active-show");
    }

    public async Task InitializeForStageAsync(CancellationToken cancellationToken = default)
    {
        sceneState = (await sceneShowStore.LoadAsync(cancellationToken)).Normalize();
        await AnalyzeAsync(
            selectedOnly: false,
            cancellationToken,
            activeSetlist: HasActiveSetlist);
    }

    private async Task AnalyzeAsync(
        bool selectedOnly,
        CancellationToken cancellationToken,
        bool activeSetlist = false)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var textState = await textPresetStore.LoadAsync(cancellationToken);
            var builtIns = await builtInArchiveStore.LoadAsync(cancellationToken);
            var faceState = await facePatternStore.LoadAsync(cancellationToken);
            var layout = (await galleryLayoutStore.LoadAsync(cancellationToken)).Normalize();
            var profile = await profileSession.GetActiveProfileAsync(cancellationToken);
            var safetyAcknowledgements = await flashSafetyAcknowledgementStore.LoadAsync(cancellationToken);
            var animationState = await animationProjectStore.LoadAsync(cancellationToken);
            sceneState = (await sceneShowStore.LoadAsync(cancellationToken)).Normalize();
            var catalog = new GalleryCatalogBuilder(quickActionCatalog).Build(
                textState,
                builtIns,
                faceState,
                layout.Order,
                animationState,
                sceneState);
            var analysisLayout = activeSetlist
                ? CreateActiveSetlistLayout(sceneState)
                : layout;
            var selectedPageIds = !activeSetlist && selectedOnly && SelectedPage is not null
                ? new[] { SelectedPage.PageId }
                : [];
            var report = analyzer.Analyze(new FestivalPreflightRequest
            {
                Layout = analysisLayout,
                Catalog = catalog,
                SelectedPageIds = selectedPageIds,
                ActiveProfile = profile,
                SchedulerSnapshot = scheduler.GetSnapshot(),
                ConnectionState = deviceConnection.State,
                FlashSafetyAcknowledgements = safetyAcknowledgements,
                EvaluatedAt = DateTimeOffset.UtcNow
            });
            currentCatalog = catalog;
            currentReport = report;
            analyzedProfile = profile;
            analyzedConnectionState = deviceConnection.State;
            lastRunSelectedOnly = selectedOnly;
            lastRunActiveSetlist = activeSetlist;
            ApplyReport(report);
            statusSession?.Update(new PreflightStatusSnapshot(
                report.Status,
                report.StatusText,
                SummaryText,
                DateTimeOffset.UtcNow));
            ScopeText = activeSetlist && HasActiveSetlist
                ? $"Setlist · {sceneState.Setlists.First(setlist => setlist.Id == sceneState.ActiveSetlistId).DisplayName}"
                : selectedOnly && SelectedPage is not null
                ? SelectedPage.DisplayName
                : $"Whole show · {layout.Pages.Count} Deck(s)";
            OnPropertyChanged(nameof(HasActiveSetlist));
            OnPropertyChanged(nameof(ActiveSetlistText));
            RunActiveSetlistCommand.RaiseCanExecuteChanged();
            ActiveMaskText = profile is null
                ? "No active mask profile"
                : $"{profile.DisplayName} · {profile.Capabilities.AcknowledgementMode} · {profile.PreparedSlots.Count} prepared slot(s)";
            LastRunAt = DateTimeOffset.UtcNow;
            PrepareDiyContentCommand.RaiseCanExecuteChanged();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            StatusText = "NOT READY";
            StatusColorHex = "#FF5C54";
            SummaryText = $"Preflight could not load show data: {exception.Message}";
            Issues.Clear();
            Blockers.Clear();
            Warnings.Clear();
            VerifiedChecks.Clear();
            var issue = new PreflightIssue(
                "preflight-load-failed",
                PreflightIssueSeverity.Blocking,
                SummaryText,
                "Retry. If the error persists, export diagnostics before resetting only the affected store.");
            Issues.Add(issue);
            Blockers.Add(issue);
            OnPropertyChanged(nameof(HasNoIssues));
            OnPropertyChanged(nameof(HasIssues));
            OnPropertyChanged(nameof(HasBlockers));
            OnPropertyChanged(nameof(HasWarnings));
            OnPropertyChanged(nameof(HasVerifiedChecks));
            Actions.Clear();
            currentReport = null;
            currentCatalog = [];
            analyzedProfile = null;
            analyzedConnectionState = deviceConnection.State;
            NotifyReportStateChanged();
            statusSession?.Update(new PreflightStatusSnapshot(
                FestivalPreflightStatus.NotReady,
                "NOT READY",
                SummaryText,
                DateTimeOffset.UtcNow));
            PrepareDiyContentCommand.RaiseCanExecuteChanged();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PrepareDiyContentAsync(CancellationToken cancellationToken)
    {
        if (IsBusy || currentReport is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await preparationService.PrepareAsync(
                currentReport,
                currentCatalog,
                cancellationToken);
            PreparationStatusText = result.Message;
        }
        finally
        {
            IsBusy = false;
        }

        await AnalyzeAsync(lastRunSelectedOnly, cancellationToken, lastRunActiveSetlist);
    }

    public async Task AcknowledgeBlockedFlashRiskAsync(CancellationToken cancellationToken = default)
    {
        var blocked = (currentReport?.FlashSafetyResults ?? [])
            .Where(result => result.Decision.Status == FlashSafetyStatus.Blocked)
            .GroupBy(result => (result.Assessment.ContentId, result.Assessment.RevisionHash))
            .Select(group => group.First().Assessment)
            .ToArray();
        if (blocked.Length == 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            foreach (var assessment in blocked)
            {
                await flashSafetyAcknowledgementService
                    .AcknowledgeAsync(assessment, cancellationToken);
            }

            PreparationStatusText = $"Recorded explicit flash-risk acknowledgement for {blocked.Length} exact revision(s).";
        }
        finally
        {
            IsBusy = false;
        }

        await AnalyzeAsync(lastRunSelectedOnly, cancellationToken, lastRunActiveSetlist);
    }

    public async Task RevokeFlashRiskOverridesAsync(CancellationToken cancellationToken = default)
    {
        var contentIds = (currentReport?.FlashSafetyResults ?? [])
            .Where(result => result.Decision.Status == FlashSafetyStatus.AcknowledgedOverride)
            .Select(result => result.Assessment.ContentId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (contentIds.Length == 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            foreach (var contentId in contentIds)
            {
                await flashSafetyAcknowledgementService.RevokeAsync(contentId, cancellationToken);
            }

            PreparationStatusText = $"Revoked {contentIds.Length} flash-risk override(s); those revisions are blocked again.";
        }
        finally
        {
            IsBusy = false;
        }

        await AnalyzeAsync(lastRunSelectedOnly, cancellationToken, lastRunActiveSetlist);
    }

    private static GalleryLayoutState CreateActiveSetlistLayout(SceneShowState state)
    {
        var setlist = state.Setlists.FirstOrDefault(item => item.Id == state.ActiveSetlistId);
        if (setlist is null)
        {
            return new GalleryLayoutState
            {
                Pages =
                [
                    new GalleryPageLayout
                    {
                        PageId = "active-setlist",
                        Title = "No active setlist",
                        Items = []
                    }
                ]
            };
        }

        return new GalleryLayoutState
        {
            Pages =
            [
                new GalleryPageLayout
                {
                    PageId = $"setlist:{setlist.Id}",
                    Title = setlist.DisplayName,
                    ColorHex = "#A78BFA",
                    Items = setlist.Cues.Select((cue, index) => new GalleryPageItemLayout
                    {
                        SlotId = $"setlist:{setlist.Id}:cue:{cue.Id}",
                        GalleryItemId = $"scene:{cue.SceneId}",
                        Label = cue.Label,
                        IconKey = "lucide:clapperboard",
                        ColorHex = "#A78BFA",
                        SortIndex = index
                    }).ToArray()
                }
            ]
        };
    }

    private void ApplyReport(FestivalPreflightReport report)
    {
        StatusText = report.StatusText;
        StatusColorHex = report.Status switch
        {
            FestivalPreflightStatus.ShowReady => "#22C55E",
            FestivalPreflightStatus.Degraded => "#FACC15",
            _ => "#FF5C54"
        };
        SummaryText = $"{report.Actions.Count} action(s) · {report.InstantActionCount} instant · "
            + $"{report.PreparedActionCount} prepared · {report.UploadRequiredActionCount} upload · "
            + $"{report.UnverifiedActionCount} unverified";

        Issues.Clear();
        Blockers.Clear();
        Warnings.Clear();
        foreach (var issue in report.Issues)
        {
            Issues.Add(issue);
            if (issue.Severity == PreflightIssueSeverity.Blocking)
            {
                Blockers.Add(issue);
            }
            else
            {
                Warnings.Add(issue);
            }
        }
        OnPropertyChanged(nameof(HasNoIssues));
        OnPropertyChanged(nameof(HasIssues));
        OnPropertyChanged(nameof(HasBlockers));
        OnPropertyChanged(nameof(HasWarnings));
        OnPropertyChanged(nameof(BlockingIssueCount));
        OnPropertyChanged(nameof(WarningIssueCount));

        Actions.Clear();
        foreach (var action in report.Actions)
        {
            Actions.Add(action);
        }

        NotifyReportStateChanged();

        VerifiedChecks.Clear();
        if (analyzedConnectionState == BleConnectionState.Connected)
        {
            VerifiedChecks.Add(new PreflightVerifiedCheck(
                "Mask connected",
                "The active physical mask is connected for this check."));
        }

        if (analyzedProfile?.Capabilities.CommandWriteAvailable == true)
        {
            VerifiedChecks.Add(new PreflightVerifiedCheck(
                "Command write ready",
                string.IsNullOrWhiteSpace(analyzedProfile.Capabilities.TransportName)
                    ? "The command characteristic was observed ready."
                    : $"Observed using {analyzedProfile.Capabilities.TransportName}."));
        }

        if (report.SlotAllocations.Count == 0 || report.SlotAllocations.All(allocation => allocation.IsPrepared))
        {
            VerifiedChecks.Add(new PreflightVerifiedCheck(
                "DIY preparation resolved",
                "No required DIY upload remains in this checked scope."));
        }

        if (report.FlashSafetyResults.All(result => result.Decision.Status == FlashSafetyStatus.Safe))
        {
            VerifiedChecks.Add(new PreflightVerifiedCheck(
                "Safety gate clear",
                "No checked animation revision is blocked by flash analysis."));
        }

        OnPropertyChanged(nameof(HasVerifiedChecks));
    }

    private void NotifyReportStateChanged()
    {
        OnPropertyChanged(nameof(HasBlockedFlashSafety));
        OnPropertyChanged(nameof(HasAcknowledgedFlashSafety));
        OnPropertyChanged(nameof(FlashSafetySummary));
        OnPropertyChanged(nameof(InstantMetricText));
        OnPropertyChanged(nameof(PreparedMetricText));
        OnPropertyChanged(nameof(UploadMetricText));
        OnPropertyChanged(nameof(UnverifiedMetricText));
        OnPropertyChanged(nameof(ReadinessIcon));
        OnPropertyChanged(nameof(CanEnterStage));
        OnPropertyChanged(nameof(SlotUsageMetricText));
        OnPropertyChanged(nameof(LatencyMetricText));
        OnPropertyChanged(nameof(AcknowledgementMetricText));
        OnPropertyChanged(nameof(ConnectionMetricText));
        OnPropertyChanged(nameof(SafetyMetricText));
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
