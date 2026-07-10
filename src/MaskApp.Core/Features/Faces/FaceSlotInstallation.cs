namespace MaskApp.Core.Features.Faces;

public sealed record FaceSlotInstallation
{
    public int Slot { get; init; }

    public string ContentFingerprint { get; init; } = string.Empty;

    public string SourceId { get; init; } = string.Empty;

    public DateTimeOffset InstalledAt { get; init; }

    public FaceSlotInstallation Normalize() =>
        this with
        {
            Slot = Math.Clamp(Slot, FacePattern.MinSlot, FacePattern.MaxSlot),
            ContentFingerprint = ContentFingerprint?.Trim() ?? string.Empty,
            SourceId = SourceId?.Trim() ?? string.Empty,
            InstalledAt = InstalledAt == default ? DateTimeOffset.UtcNow : InstalledAt
        };
}
