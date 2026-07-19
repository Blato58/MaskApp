using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Preflight;

public enum FestivalPreflightStatus
{
    ShowReady,
    Degraded,
    NotReady
}

public enum PreflightActionClassification
{
    Instant,
    UploadRequired,
    Prepared,
    Unverified
}

public enum PreflightIssueSeverity
{
    Warning,
    Blocking
}

public sealed record PreflightIssue(
    string Code,
    PreflightIssueSeverity Severity,
    string Message,
    string RecoveryAction,
    string? ItemId = null)
{
    public string SeverityText => Severity == PreflightIssueSeverity.Blocking ? "BLOCKING" : "WARNING";

    public string ColorHex => Severity == PreflightIssueSeverity.Blocking ? "#FF5C54" : "#FACC15";
}

public sealed record PreflightVerifiedCheck(string Title, string Detail);

public sealed record DiySlotRequirement(
    string RequirementId,
    string SourceId,
    string ContentFingerprint,
    int PreferredSlot);

public sealed record DiySlotAllocation(
    string RequirementId,
    string SourceId,
    string ContentFingerprint,
    int PreferredSlot,
    int AssignedSlot,
    bool IsPrepared,
    MaskPreparedSlotVerification? Verification,
    string ReplacedSourceId);

public sealed record DiySlotAllocationResult(
    bool Succeeded,
    IReadOnlyList<DiySlotAllocation> Allocations,
    IReadOnlyList<PreflightIssue> Issues);

public sealed record PreflightActionAssessment(
    string PageId,
    string PageTitle,
    string SlotId,
    string ItemId,
    string Title,
    PreflightActionClassification Classification,
    IReadOnlyList<DiySlotAllocation> DiySlots)
{
    public string ClassificationText => Classification switch
    {
        PreflightActionClassification.Instant => "INSTANT",
        PreflightActionClassification.UploadRequired => "UPLOAD REQUIRED",
        PreflightActionClassification.Prepared => "PREPARED",
        _ => "UNVERIFIED"
    };

    public string ColorHex => Classification switch
    {
        PreflightActionClassification.Instant => "#52E3FF",
        PreflightActionClassification.Prepared => "#22C55E",
        PreflightActionClassification.UploadRequired => "#FACC15",
        _ => "#FF8A65"
    };
}

public sealed record PreflightFlashSafetyResult(
    string ItemId,
    string Title,
    FlashSafetyAssessment Assessment,
    FlashSafetyDecision Decision);

public sealed record FestivalPreflightRequest
{
    public GalleryLayoutState Layout { get; init; } = new();

    public IReadOnlyList<GalleryItem> Catalog { get; init; } = [];

    public IReadOnlyList<string> SelectedPageIds { get; init; } = [];

    public MaskProfile? ActiveProfile { get; init; }

    public MaskBleSchedulerSnapshot? SchedulerSnapshot { get; init; }

    public BleConnectionState ConnectionState { get; init; } = BleConnectionState.Disconnected;

    public PreflightRuntimeSnapshot RuntimeSnapshot { get; init; } = new();

    public PreflightRuntimeRequirement RequiredRuntimePermissions { get; init; } =
        PreflightRuntimeRequirement.Bluetooth;

    public FlashSafetyAcknowledgementState FlashSafetyAcknowledgements { get; init; } = new();

    public DateTimeOffset EvaluatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record FestivalPreflightReport(
    FestivalPreflightStatus Status,
    string StatusText,
    IReadOnlyList<PreflightActionAssessment> Actions,
    IReadOnlyList<DiySlotAllocation> SlotAllocations,
    IReadOnlyList<PreflightIssue> Issues)
{
    public IReadOnlyList<PreflightFlashSafetyResult> FlashSafetyResults { get; init; } = [];

    public int InstantActionCount => Actions.Count(action =>
        action.Classification == PreflightActionClassification.Instant);

    public int PreparedActionCount => Actions.Count(action =>
        action.Classification == PreflightActionClassification.Prepared);

    public int UploadRequiredActionCount => Actions.Count(action =>
        action.Classification == PreflightActionClassification.UploadRequired);

    public int UnverifiedActionCount => Actions.Count(action =>
        action.Classification == PreflightActionClassification.Unverified);
}
