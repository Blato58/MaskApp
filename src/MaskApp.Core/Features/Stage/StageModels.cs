using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Preflight;

namespace MaskApp.Core.Features.Stage;

public enum StageLayoutMode
{
    Giant,
    Grid2x2,
    Dense
}

public sealed record StageTile(
    string TileId,
    string Label,
    string Subtitle,
    string ColorHex,
    GalleryItemType ItemType,
    bool IsPrepared,
    bool IsHoldAction,
    string ReadinessText)
{
    public string ReadinessBadgeText => IsPrepared ? "READY" : "UNPREPARED";

    public string ReadinessColorHex => IsPrepared ? "#30D158" : "#FFD60A";

    public string ActionHint => IsHoldAction
        ? $"Hold to play {Label}; release to restore the previous look"
        : $"Show {Label}";
}

public sealed record StageShowSnapshot(
    string PageId,
    string PageTitle,
    string PageColorHex,
    int PageIndex,
    int PageCount,
    IReadOnlyList<StageTile> Tiles,
    string PositionLabel = "Page",
    string NextCueLabel = "")
{
    public string PagePositionText => PageCount == 0
        ? "No prepared Pages"
        : $"{PositionLabel} {PageIndex + 1} of {PageCount}";
}

public sealed record StageReadinessSnapshot(
    FestivalPreflightStatus Status,
    string StatusText,
    string Summary,
    int BlockingIssueCount,
    int WarningIssueCount)
{
    public static StageReadinessSnapshot NotReady(string summary) =>
        new(FestivalPreflightStatus.NotReady, "NOT READY", summary, 1, 0);
}

public interface IStageShowSource
{
    Task<StageShowSnapshot> InitializeAsync(CancellationToken cancellationToken = default);

    Task<StageShowSnapshot> SelectPageAsync(int pageIndex, CancellationToken cancellationToken = default);

    Task<GalleryActionResult> TriggerAsync(string tileId, CancellationToken cancellationToken = default);

    void StartObservingTransportState();

    void StopObservingTransportState();
}

public interface IStageReadinessProvider
{
    Task<StageReadinessSnapshot> EvaluateAsync(CancellationToken cancellationToken = default);
}

public interface IStageDeviceFeedback
{
    void Success();

    void Failure();

    void Warning();
}

public interface IStageDisplayControl
{
    void SetKeepAwake(bool enabled);
}
