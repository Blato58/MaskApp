using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Profiles;

public sealed record MaskPreparedSlot
{
    public int Slot { get; init; }

    public string ContentFingerprint { get; init; } = string.Empty;

    public string SourceId { get; init; } = string.Empty;

    public DateTimeOffset InstalledAt { get; init; }

    public MaskPreparedSlotVerification Verification { get; init; } =
        MaskPreparedSlotVerification.WriteOnlyUnverified;

    public MaskPreparedSlot Normalize() =>
        this with
        {
            Slot = Math.Clamp(Slot, FacePattern.MinSlot, FacePattern.MaxSlot),
            ContentFingerprint = ContentFingerprint?.Trim() ?? string.Empty,
            SourceId = SourceId?.Trim() ?? string.Empty,
            InstalledAt = InstalledAt == default ? DateTimeOffset.UtcNow : InstalledAt
        };

    public FaceSlotInstallation ToFaceSlotInstallation() =>
        new()
        {
            Slot = Slot,
            ContentFingerprint = ContentFingerprint,
            SourceId = SourceId,
            InstalledAt = InstalledAt
        };

    public static MaskPreparedSlot FromLegacy(FaceSlotInstallation installation) =>
        new MaskPreparedSlot
        {
            Slot = installation.Slot,
            ContentFingerprint = installation.ContentFingerprint,
            SourceId = installation.SourceId,
            InstalledAt = installation.InstalledAt,
            Verification = MaskPreparedSlotVerification.UnverifiedLegacy
        }.Normalize();
}
