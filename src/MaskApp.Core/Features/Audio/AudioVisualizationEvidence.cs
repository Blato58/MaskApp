namespace MaskApp.Core.Features.Audio;

public enum AudioVisualizationEvidenceStatus
{
    Unknown,
    PendingPhysicalConfirmation,
    Passed,
    Failed,
    Unsupported
}

public sealed record AudioVisualizationEvidence
{
    public const int CurrentProtocolVersion = 1;

    public int ProtocolVersion { get; init; } = CurrentProtocolVersion;

    public AudioVisualizationEvidenceStatus Status { get; init; }

    public AudioVisualizationFraming Framing { get; init; } = AudioVisualizationFraming.LegacyAndroidLength;

    public AudioVisualizationPackingMode PackingMode { get; init; } = AudioVisualizationPackingMode.PaletteA;

    public bool CharacteristicObserved { get; init; }

    public bool IsSimulated { get; init; }

    public int PacketsAttempted { get; init; }

    public int PacketsSent { get; init; }

    public int FailedWrites { get; init; }

    public double RequestedCadenceHz { get; init; }

    public double? ObservedWriteCadenceHz { get; init; }

    public DateTimeOffset TestedAt { get; init; }

    public string StatusText { get; init; } = "Audio visualization has not been tested on this mask.";

    public bool EnablesLiveMicrophone =>
        Status == AudioVisualizationEvidenceStatus.Passed
        && CharacteristicObserved
        && !IsSimulated
        && PacketsAttempted > 0
        && PacketsSent == PacketsAttempted
        && FailedWrites == 0;

    public AudioVisualizationEvidence Normalize() => this with
    {
        ProtocolVersion = CurrentProtocolVersion,
        PacketsAttempted = Math.Max(0, PacketsAttempted),
        PacketsSent = Math.Clamp(PacketsSent, 0, Math.Max(0, PacketsAttempted)),
        FailedWrites = Math.Clamp(FailedWrites, 0, Math.Max(0, PacketsAttempted)),
        RequestedCadenceHz = NormalizeCadence(RequestedCadenceHz) ?? 8,
        ObservedWriteCadenceHz = NormalizeCadence(ObservedWriteCadenceHz),
        TestedAt = TestedAt == default && Status != AudioVisualizationEvidenceStatus.Unknown
            ? DateTimeOffset.UtcNow
            : TestedAt,
        StatusText = string.IsNullOrWhiteSpace(StatusText)
            ? "Audio visualization evidence has no status detail."
            : StatusText.Trim()
    };

    private static double? NormalizeCadence(double? cadence) =>
        cadence is > 0 and <= 60 && double.IsFinite(cadence.Value) ? cadence : null;
}
