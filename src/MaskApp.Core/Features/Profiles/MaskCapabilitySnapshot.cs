namespace MaskApp.Core.Features.Profiles;

public sealed record MaskCapabilitySnapshot
{
    public bool CommandWriteAvailable { get; init; }

    public bool TextUploadAvailable { get; init; }

    public bool FaceUploadAvailable { get; init; }

    public bool AudioVisualizationWriteAvailable { get; init; }

    public MaskAcknowledgementMode AcknowledgementMode { get; init; }

    public int DiySlotCapacity { get; init; } = 20;

    public string FirmwareRevision { get; init; } = string.Empty;

    public string TransportName { get; init; } = string.Empty;

    public DateTimeOffset ObservedAt { get; init; }

    public string Fingerprint => string.Join(
        '|',
        CommandWriteAvailable,
        TextUploadAvailable,
        FaceUploadAvailable,
        AcknowledgementMode,
        DiySlotCapacity,
        FirmwareRevision.Trim(),
        TransportName.Trim());

    public MaskCapabilitySnapshot Normalize() =>
        this with
        {
            DiySlotCapacity = Math.Clamp(DiySlotCapacity, 0, 20),
            FirmwareRevision = FirmwareRevision?.Trim() ?? string.Empty,
            TransportName = TransportName?.Trim() ?? string.Empty,
            ObservedAt = ObservedAt == default ? DateTimeOffset.UtcNow : ObservedAt
        };
}
