using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Preflight;

public sealed class FestivalPreflightAnalyzer
{
    private static readonly TimeSpan CapabilityFreshness = TimeSpan.FromHours(24);
    private readonly DiySlotAllocator slotAllocator;
    private readonly PerformanceAnimationBuilder animationBuilder;
    private readonly FlashSafetyAnalyzer flashSafetyAnalyzer;
    private readonly AnimationLoadAnalyzer animationLoadAnalyzer;
    private readonly SceneValidator sceneValidator;

    public FestivalPreflightAnalyzer(
        DiySlotAllocator? slotAllocator = null,
        PerformanceAnimationBuilder? animationBuilder = null,
        FlashSafetyAnalyzer? flashSafetyAnalyzer = null,
        AnimationLoadAnalyzer? animationLoadAnalyzer = null,
        SceneValidator? sceneValidator = null)
    {
        this.slotAllocator = slotAllocator ?? new DiySlotAllocator();
        this.animationBuilder = animationBuilder ?? new PerformanceAnimationBuilder();
        this.flashSafetyAnalyzer = flashSafetyAnalyzer ?? new FlashSafetyAnalyzer();
        this.animationLoadAnalyzer = animationLoadAnalyzer ?? new AnimationLoadAnalyzer();
        this.sceneValidator = sceneValidator ?? new SceneValidator();
    }

    public FestivalPreflightReport Analyze(FestivalPreflightRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var issues = new List<PreflightIssue>();
        var profile = request.ActiveProfile?.Normalize();
        var capabilities = profile?.Capabilities.Normalize();
        if (request.ConnectionState != BleConnectionState.Connected)
        {
            issues.Add(new PreflightIssue(
                "mask-disconnected",
                PreflightIssueSeverity.Blocking,
                "The show mask is not connected now.",
                "Open Device, connect the physical show mask, then rerun Preflight."));
        }

        AddProfileIssues(profile, capabilities, request.EvaluatedAt, issues);
        if (request.SchedulerSnapshot is { PendingOperationCount: > 0 } scheduler)
        {
            issues.Add(new PreflightIssue(
                "scheduler-busy",
                PreflightIssueSeverity.Warning,
                $"The mask scheduler still has {scheduler.PendingOperationCount} queued operation(s).",
                "Wait for the queue to drain or use Stop before starting show preparation."));
        }

        if (profile is not null && profile.AverageCommandLatencyMilliseconds is null)
        {
            issues.Add(new PreflightIssue(
                "latency-unmeasured",
                PreflightIssueSeverity.Warning,
                "No command-latency measurement is stored for the active mask.",
                "Treat rapid cue timing as unverified and rehearse it on the physical show mask."));
        }

        var pages = SelectPages(request.Layout.Normalize(), request.SelectedPageIds);
        var catalog = request.Catalog.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var drafts = new List<ActionDraft>();
        var requirements = new List<DiySlotRequirement>();
        foreach (var page in pages)
        {
            foreach (var pageItem in page.Items)
            {
                if (!catalog.TryGetValue(pageItem.GalleryItemId, out var item))
                {
                    issues.Add(new PreflightIssue(
                        "missing-gallery-item",
                        PreflightIssueSeverity.Blocking,
                        $"{page.Title} references missing content {pageItem.GalleryItemId}.",
                        "Edit the Page and replace or remove the missing tile.",
                        pageItem.GalleryItemId));
                    continue;
                }

                var draft = CreateDraft(
                    page,
                    pageItem,
                    item,
                    profile,
                    capabilities,
                    request.FlashSafetyAcknowledgements,
                    catalog,
                    issues);
                drafts.Add(draft);
                requirements.AddRange(draft.Requirements);
            }
        }

        if (drafts.Count == 0)
        {
            issues.Add(new PreflightIssue(
                "show-empty",
                PreflightIssueSeverity.Blocking,
                "The selected show scope contains no playable actions.",
                "Add content to a selected Page before running Preflight."));
        }

        var allocation = slotAllocator.Allocate(
            requirements,
            profile,
            capabilities?.DiySlotCapacity ?? 0);
        issues.AddRange(allocation.Issues);
        var allocationsByRequirement = allocation.Allocations.ToDictionary(
            item => item.RequirementId,
            StringComparer.Ordinal);
        var actions = drafts
            .Select(draft => CompleteAssessment(draft, allocationsByRequirement, issues))
            .ToArray();

        var status = issues.Any(issue => issue.Severity == PreflightIssueSeverity.Blocking)
            ? FestivalPreflightStatus.NotReady
            : issues.Count > 0
                ? FestivalPreflightStatus.Degraded
                : FestivalPreflightStatus.ShowReady;
        var statusText = status switch
        {
            FestivalPreflightStatus.ShowReady => "SHOW READY",
            FestivalPreflightStatus.Degraded => "DEGRADED",
            _ => "NOT READY"
        };
        return new FestivalPreflightReport(status, statusText, actions, allocation.Allocations, issues)
        {
            FlashSafetyResults = drafts
                .SelectMany(draft => draft.FlashSafety)
                .GroupBy(result => (result.ItemId, result.Assessment.RevisionHash))
                .Select(group => group.First())
                .ToArray()
        };
    }

    private static void AddProfileIssues(
        MaskProfile? profile,
        MaskCapabilitySnapshot? capabilities,
        DateTimeOffset evaluatedAt,
        ICollection<PreflightIssue> issues)
    {
        if (profile is null || capabilities is null)
        {
            issues.Add(new PreflightIssue(
                "mask-profile-missing",
                PreflightIssueSeverity.Blocking,
                "No physical mask profile is active.",
                "Scan and connect the mask that will be used for this show."));
            return;
        }

        if (!capabilities.CommandWriteAvailable)
        {
            issues.Add(new PreflightIssue(
                "command-write-unavailable",
                PreflightIssueSeverity.Blocking,
                "The command characteristic was not ready during the last capability observation.",
                "Reconnect the mask and confirm Device diagnostics report command writes ready."));
        }

        if (capabilities.AcknowledgementMode == MaskAcknowledgementMode.Unknown)
        {
            issues.Add(new PreflightIssue(
                "ack-mode-unknown",
                PreflightIssueSeverity.Blocking,
                "The mask ACK/write-only mode is unknown.",
                "Reconnect and complete capability discovery before preparing content."));
        }
        else if (capabilities.AcknowledgementMode == MaskAcknowledgementMode.WriteOnly)
        {
            issues.Add(new PreflightIssue(
                "write-only-mode",
                PreflightIssueSeverity.Warning,
                "This mask uses write-only compatibility mode, so uploads cannot be confirmed by ACK.",
                "Prepare and visually verify every required DIY slot on the physical mask."));
        }

        if (capabilities.ObservedAt == default
            || evaluatedAt - capabilities.ObservedAt > CapabilityFreshness)
        {
            issues.Add(new PreflightIssue(
                "capabilities-stale",
                PreflightIssueSeverity.Warning,
                "The mask capability observation is older than 24 hours.",
                "Reconnect the show mask to refresh capabilities."));
        }
    }

    private static IReadOnlyList<GalleryPageLayout> SelectPages(
        GalleryLayoutState layout,
        IReadOnlyList<string> selectedPageIds)
    {
        if (selectedPageIds.Count == 0)
        {
            return layout.Pages;
        }

        var selected = selectedPageIds.ToHashSet(StringComparer.Ordinal);
        return layout.Pages.Where(page => selected.Contains(page.PageId)).ToArray();
    }

    private ActionDraft CreateDraft(
        GalleryPageLayout page,
        GalleryPageItemLayout pageItem,
        GalleryItem item,
        MaskProfile? profile,
        MaskCapabilitySnapshot? capabilities,
        FlashSafetyAcknowledgementState safetyAcknowledgements,
        IReadOnlyDictionary<string, GalleryItem> catalog,
        ICollection<PreflightIssue> issues)
    {
        var classification = PreflightActionClassification.Instant;
        var requirements = new List<DiySlotRequirement>();
        var flashSafety = new List<PreflightFlashSafetyResult>();
        var hasLiveUpload = false;
        var hasUnverifiedDependency = false;

        switch (item.Type)
        {
            case GalleryItemType.TextPreset:
                classification = PreflightActionClassification.UploadRequired;
                hasLiveUpload = true;
                RequireTextCapability(item, capabilities, issues);
                break;
            case GalleryItemType.CustomStaticFace when item.FacePattern is not null:
            {
                classification = PreflightActionClassification.UploadRequired;
                RequireFaceCapability(item, capabilities, issues);
                var pattern = item.FacePattern.Normalize();
                requirements.Add(new DiySlotRequirement(
                    $"{pageItem.SlotId}:face",
                    item.Id,
                    FaceContentFingerprint.Compute(pattern),
                    pattern.PreferredSlot));
                break;
            }
            case GalleryItemType.AppBuiltInAnimation when item.AppAnimation is not null:
            case GalleryItemType.CustomAnimation when item.PerformanceAnimation is not null:
            {
                classification = PreflightActionClassification.UploadRequired;
                RequireFaceCapability(item, capabilities, issues);
                var performanceAnimation = item.PerformanceAnimation
                    ?? animationBuilder.FromAppBuiltIn(item.AppAnimation!.Normalize());
                requirements.AddRange(performanceAnimation.StoredFrames.Select((frame, index) =>
                    new DiySlotRequirement(
                        $"{pageItem.SlotId}:animation:{index}",
                        $"{item.Id}:frame:{index}",
                        frame.ContentFingerprint,
                        frame.Slot)));
                var safetyAssessment = flashSafetyAnalyzer.Analyze(performanceAnimation);
                var safetyDecision = flashSafetyAnalyzer.Decide(safetyAssessment, safetyAcknowledgements);
                var loadAssessment = animationLoadAnalyzer.Analyze(performanceAnimation);
                flashSafety.Add(new PreflightFlashSafetyResult(
                    item.Id,
                    item.Title,
                    safetyAssessment,
                    safetyDecision));
                if (safetyDecision.Status == FlashSafetyStatus.Blocked)
                {
                    issues.Add(new PreflightIssue(
                        "flash-safety-blocked",
                        PreflightIssueSeverity.Blocking,
                        $"{item.Title}: {safetyDecision.Message}",
                        "Open the exact animation revision, review the photosensitivity warning, and explicitly acknowledge or edit its timing.",
                        item.Id));
                }
                else if (safetyDecision.Status == FlashSafetyStatus.AcknowledgedOverride)
                {
                    issues.Add(new PreflightIssue(
                        "flash-safety-overridden",
                        PreflightIssueSeverity.Warning,
                        $"{item.Title} uses an explicit flash-risk override for revision {safetyAssessment.RevisionHash[..12]}.",
                        "Keep Blackout immediately available; revoke the acknowledgement to block this revision again.",
                        item.Id));
                }
                if (profile?.SustainableCadenceHz is null && capabilities is not null)
                {
                    issues.Add(new PreflightIssue(
                        "cadence-unmeasured",
                        PreflightIssueSeverity.Warning,
                        $"Sustainable animation cadence has not been recorded for {item.Title}.",
                        "Run a real-mask cadence check and save the result to this mask profile.",
                        item.Id));
                }
                else if (profile?.SustainableCadenceHz is double sustainableCadence
                    && loadAssessment.AverageCadenceHz > sustainableCadence)
                {
                    issues.Add(new PreflightIssue(
                        "cadence-exceeded",
                        PreflightIssueSeverity.Blocking,
                        $"{item.Title} requires {loadAssessment.AverageCadenceHz:0.0} frames/s, above this mask's measured {sustainableCadence:0.0} frames/s cadence.",
                        "Lower the animation BPM/frame rate or run and save a new real-mask cadence measurement.",
                        item.Id));
                }
                else if (profile?.SustainableCadenceHz is double measuredCadence
                    && loadAssessment.AverageCadenceHz > measuredCadence * 0.85)
                {
                    issues.Add(new PreflightIssue(
                        "cadence-near-limit",
                        PreflightIssueSeverity.Warning,
                        $"{item.Title} uses more than 85% of the active mask's measured command cadence.",
                        "Rehearse on the physical show mask and watch dropped-frame diagnostics.",
                        item.Id));
                }

                if (loadAssessment.HasHighSustainedLoad)
                {
                    issues.Add(new PreflightIssue(
                        "animation-high-load",
                        PreflightIssueSeverity.Warning,
                        $"{item.Title} combines rapid updates with a large bright area and may increase phone/mask power use.",
                        "Run a rehearsal-length battery/thermal check and reduce brightness or cadence if needed.",
                        item.Id));
                }

                break;
            }
            case GalleryItemType.BuiltInStaticImage:
            case GalleryItemType.BuiltInAnimation:
                classification = ClassifyBuiltIn(item, issues);
                hasUnverifiedDependency = classification == PreflightActionClassification.Unverified;
                break;
            case GalleryItemType.QuickAction:
                classification = ClassifyQuickAction(item, capabilities, issues);
                hasLiveUpload = classification == PreflightActionClassification.UploadRequired;
                hasUnverifiedDependency = classification == PreflightActionClassification.Unverified;
                break;
            case GalleryItemType.Scene when item.Scene is not null:
            {
                var validation = sceneValidator.Validate(item.Scene, catalog);
                foreach (var issue in validation.Issues)
                {
                    issues.Add(new PreflightIssue(
                        $"scene-{issue.Code}",
                        issue.Severity == SceneValidationSeverity.Blocking
                            ? PreflightIssueSeverity.Blocking
                            : PreflightIssueSeverity.Warning,
                        $"{item.Title}: {issue.Message}",
                        issue.RecoveryAction,
                        item.Id));
                }

                if (!validation.IsValid)
                {
                    classification = PreflightActionClassification.Unverified;
                    hasUnverifiedDependency = true;
                }

                foreach (var (step, index) in validation.ExpandedSteps.Select((step, index) => (step, index)))
                {
                    if (string.IsNullOrWhiteSpace(step.GalleryItemId)
                        || !catalog.TryGetValue(step.GalleryItemId, out var dependency)
                        || dependency.Type == GalleryItemType.Scene)
                    {
                        continue;
                    }

                    var nestedLayout = pageItem with
                    {
                        SlotId = $"{pageItem.SlotId}:scene:{index}:{step.Id}",
                        GalleryItemId = dependency.Id
                    };
                    var nested = CreateDraft(
                        page,
                        nestedLayout,
                        dependency,
                        profile,
                        capabilities,
                        safetyAcknowledgements,
                        catalog,
                        issues);
                    requirements.AddRange(nested.Requirements);
                    flashSafety.AddRange(nested.FlashSafety);
                    hasLiveUpload |= nested.HasLiveUpload;
                    hasUnverifiedDependency |= nested.HasUnverifiedDependency;
                }

                classification = hasUnverifiedDependency
                    ? PreflightActionClassification.Unverified
                    : hasLiveUpload || requirements.Count > 0
                        ? PreflightActionClassification.UploadRequired
                        : PreflightActionClassification.Instant;
                break;
            }
        }

        return new ActionDraft(
            page,
            pageItem,
            item,
            classification,
            requirements,
            flashSafety,
            hasLiveUpload,
            hasUnverifiedDependency);
    }

    private static PreflightActionClassification ClassifyBuiltIn(
        GalleryItem item,
        ICollection<PreflightIssue> issues)
    {
        var status = item.BuiltInAssetRecord?.Status ?? BuiltInAssetStatus.Untested;
        if (status is BuiltInAssetStatus.Working or BuiltInAssetStatus.Favorite)
        {
            return PreflightActionClassification.Instant;
        }

        var blocking = status == BuiltInAssetStatus.Bad;
        issues.Add(new PreflightIssue(
            blocking ? "built-in-bad" : "built-in-unverified",
            blocking ? PreflightIssueSeverity.Blocking : PreflightIssueSeverity.Warning,
            $"{item.Title} is marked {status} for this catalog.",
            blocking
                ? "Remove the tile or retest and explicitly mark a working command."
                : "Test this stock command on the show mask before relying on it.",
            item.Id));
        return PreflightActionClassification.Unverified;
    }

    private static PreflightActionClassification ClassifyQuickAction(
        GalleryItem item,
        MaskCapabilitySnapshot? capabilities,
        ICollection<PreflightIssue> issues)
    {
        if (item.QuickActionKind == QuickActionKind.Text)
        {
            RequireTextCapability(item, capabilities, issues);
            return PreflightActionClassification.UploadRequired;
        }

        if (item.QuickActionKind is QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation or QuickActionKind.Random)
        {
            issues.Add(new PreflightIssue(
                "quick-action-unverified",
                PreflightIssueSeverity.Warning,
                $"{item.Title} uses a stock/random command without a per-mask verification record.",
                "Trigger it on the show mask and record the working result before performance.",
                item.Id));
            return PreflightActionClassification.Unverified;
        }

        return PreflightActionClassification.Instant;
    }

    private static void RequireTextCapability(
        GalleryItem item,
        MaskCapabilitySnapshot? capabilities,
        ICollection<PreflightIssue> issues)
    {
        if (capabilities?.TextUploadAvailable == true)
        {
            return;
        }

        issues.Add(new PreflightIssue(
            "text-upload-unavailable",
            PreflightIssueSeverity.Blocking,
            $"{item.Title} needs text upload, but the capability is unavailable.",
            "Reconnect a compatible mask or remove the text action.",
            item.Id));
    }

    private static void RequireFaceCapability(
        GalleryItem item,
        MaskCapabilitySnapshot? capabilities,
        ICollection<PreflightIssue> issues)
    {
        if (capabilities?.FaceUploadAvailable == true)
        {
            return;
        }

        issues.Add(new PreflightIssue(
            "face-upload-unavailable",
            PreflightIssueSeverity.Blocking,
            $"{item.Title} needs DIY upload, but the capability is unavailable.",
            "Reconnect a compatible mask or remove the DIY action.",
            item.Id));
    }

    private static PreflightActionAssessment CompleteAssessment(
        ActionDraft draft,
        IReadOnlyDictionary<string, DiySlotAllocation> allocations,
        ICollection<PreflightIssue> issues)
    {
        var actionAllocations = draft.Requirements
            .Where(requirement => allocations.ContainsKey(requirement.RequirementId))
            .Select(requirement => allocations[requirement.RequirementId])
            .ToArray();
        var classification = draft.Classification;
        if (draft.HasUnverifiedDependency)
        {
            classification = PreflightActionClassification.Unverified;
        }
        else if (draft.HasLiveUpload)
        {
            classification = PreflightActionClassification.UploadRequired;
            issues.Add(new PreflightIssue(
                "content-upload-required",
                PreflightIssueSeverity.Warning,
                $"{draft.Item.Title} includes content that must be uploaded when triggered.",
                "Rehearse on the show mask and keep the connection stable during the cue.",
                draft.Item.Id));
        }
        else if (draft.Requirements.Count > 0 && actionAllocations.Length == draft.Requirements.Count)
        {
            if (actionAllocations.All(allocation => allocation.IsPrepared
                && allocation.Verification == MaskPreparedSlotVerification.Acknowledged))
            {
                classification = PreflightActionClassification.Prepared;
            }
            else if (actionAllocations.All(allocation => allocation.IsPrepared))
            {
                classification = PreflightActionClassification.Unverified;
                issues.Add(new PreflightIssue(
                    "prepared-content-unverified",
                    PreflightIssueSeverity.Warning,
                    $"{draft.Item.Title} is recorded in slots but lacks ACK-backed verification.",
                    "Visually verify or re-upload this content on the active mask.",
                    draft.Item.Id));
            }
            else
            {
                classification = PreflightActionClassification.UploadRequired;
                issues.Add(new PreflightIssue(
                    "content-upload-required",
                    PreflightIssueSeverity.Warning,
                    $"{draft.Item.Title} needs {actionAllocations.Count(allocation => !allocation.IsPrepared)} DIY upload(s).",
                    "Prepare this Page/show before locking Stage Mode.",
                    draft.Item.Id));
            }
        }
        else if (classification == PreflightActionClassification.UploadRequired)
        {
            issues.Add(new PreflightIssue(
                "content-upload-required",
                PreflightIssueSeverity.Warning,
                $"{draft.Item.Title} requires live upload traffic.",
                "Confirm the connection is stable and prepare persistent content where supported.",
                draft.Item.Id));
        }

        return new PreflightActionAssessment(
            draft.Page.PageId,
            draft.Page.Title,
            draft.PageItem.SlotId,
            draft.Item.Id,
            draft.Item.Title,
            classification,
            actionAllocations);
    }

    private sealed record ActionDraft(
        GalleryPageLayout Page,
        GalleryPageItemLayout PageItem,
        GalleryItem Item,
        PreflightActionClassification Classification,
        IReadOnlyList<DiySlotRequirement> Requirements,
        IReadOnlyList<PreflightFlashSafetyResult> FlashSafety,
        bool HasLiveUpload,
        bool HasUnverifiedDependency);
}
