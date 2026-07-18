namespace MaskApp.Core.Features.Animations;

public enum FlashSafetyStatus
{
    Safe,
    Blocked,
    AcknowledgedOverride
}

public sealed record FlashTransitionFinding(
    int FromFrameIndex,
    int ToFrameIndex,
    TimeSpan OccursAt,
    double AffectedAreaRatio,
    double AverageBrightnessDelta,
    double ContrastRatio);

public sealed record FlashSafetyAssessment
{
    public string ContentId { get; init; } = string.Empty;

    public string RevisionHash { get; init; } = string.Empty;

    public int MaximumFlashesPerSecond { get; init; }

    public int AnalyzedTransitionCount { get; init; }

    public double MaximumAffectedAreaRatio { get; init; }

    public double MaximumBrightnessDelta { get; init; }

    public double MaximumContrastRatio { get; init; }

    public IReadOnlyList<FlashTransitionFinding> FlashTransitions { get; init; } = [];

    public bool IsSafeByDefault => MaximumFlashesPerSecond <= FlashSafetyAnalyzer.MaximumDefaultFlashesPerSecond;
}

public sealed record FlashSafetyAcknowledgement
{
    public string ContentId { get; init; } = string.Empty;

    public string RevisionHash { get; init; } = string.Empty;

    public DateTimeOffset AcknowledgedAt { get; init; }

    public string Warning { get; init; } = string.Empty;

    public FlashSafetyAcknowledgement Normalize() => this with
    {
        ContentId = ContentId?.Trim() ?? string.Empty,
        RevisionHash = RevisionHash?.Trim().ToLowerInvariant() ?? string.Empty,
        AcknowledgedAt = AcknowledgedAt == default ? DateTimeOffset.UtcNow : AcknowledgedAt,
        Warning = Warning?.Trim() ?? string.Empty
    };
}

public sealed record FlashSafetyAcknowledgementState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public IReadOnlyList<FlashSafetyAcknowledgement> Acknowledgements { get; init; } = [];

    public bool UsedFallback { get; init; }

    public string Status { get; init; } = string.Empty;

    public FlashSafetyAcknowledgementState Normalize() => this with
    {
        SchemaVersion = CurrentSchemaVersion,
        Acknowledgements = (Acknowledgements ?? [])
            .Select(item => item.Normalize())
            .Where(item => !string.IsNullOrWhiteSpace(item.ContentId)
                && !string.IsNullOrWhiteSpace(item.RevisionHash))
            .GroupBy(item => item.ContentId, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(item => item.AcknowledgedAt).First())
            .OrderBy(item => item.ContentId, StringComparer.Ordinal)
            .ToArray(),
        Status = Status?.Trim() ?? string.Empty
    };
}

public sealed record FlashSafetyDecision(
    FlashSafetyStatus Status,
    string Message,
    FlashSafetyAcknowledgement? Acknowledgement)
{
    public bool CanPlay => Status != FlashSafetyStatus.Blocked;
}
