namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryPageItemLayout
{
    public string SlotId { get; init; } = Guid.NewGuid().ToString("N");

    public string GalleryItemId { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string IconKey { get; init; } = "face";

    public string ColorHex { get; init; } = "#A78BFA";

    public int SortIndex { get; init; }

    public GalleryPageItemLayout Normalize(int fallbackSortIndex) =>
        this with
        {
            SlotId = string.IsNullOrWhiteSpace(SlotId) ? Guid.NewGuid().ToString("N") : SlotId,
            Label = Label.Trim(),
            IconKey = string.IsNullOrWhiteSpace(IconKey) ? "face" : IconKey.Trim(),
            ColorHex = string.IsNullOrWhiteSpace(ColorHex) ? "#A78BFA" : ColorHex.Trim(),
            SortIndex = SortIndex < 0 ? fallbackSortIndex : SortIndex
        };
}
