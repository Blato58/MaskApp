using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.HolyPriest;

namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryPageItemLayout
{
    public string SlotId { get; init; } = Guid.NewGuid().ToString("N");

    public string GalleryItemId { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string IconKey { get; init; } = "face";

    public string ColorHex { get; init; } = "#A78BFA";

    public int SortIndex { get; init; }

    public int? FastMaskSlot { get; init; }

    public string FastContentFingerprint { get; init; } = string.Empty;

    public DateTimeOffset? FastPreparedAt { get; init; }

    public GalleryPageItemLayout Normalize(int fallbackSortIndex)
    {
        var originalGalleryItemId = GalleryItemId?.Trim() ?? string.Empty;
        var galleryItemId = HolyPriestBuiltInCatalog.MigrateGalleryItemId(originalGalleryItemId);
        var migrated = !string.Equals(originalGalleryItemId, galleryItemId, StringComparison.Ordinal);
        return this with
        {
            SlotId = string.IsNullOrWhiteSpace(SlotId) ? Guid.NewGuid().ToString("N") : SlotId,
            GalleryItemId = galleryItemId,
            Label = Label.Trim(),
            IconKey = string.IsNullOrWhiteSpace(IconKey) ? "face" : IconKey.Trim(),
            ColorHex = string.IsNullOrWhiteSpace(ColorHex) ? "#A78BFA" : ColorHex.Trim(),
            SortIndex = SortIndex < 0 ? fallbackSortIndex : SortIndex,
            FastMaskSlot = !migrated && FastMaskSlot is >= FacePattern.MinSlot and <= FacePattern.MaxSlot
                ? FastMaskSlot
                : null,
            FastContentFingerprint = migrated ? string.Empty : FastContentFingerprint?.Trim() ?? string.Empty,
            FastPreparedAt = migrated ? null : FastPreparedAt
        };
    }
}
