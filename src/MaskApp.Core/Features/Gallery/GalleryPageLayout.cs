namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryPageLayout
{
    public string PageId { get; init; } = Guid.NewGuid().ToString("N");

    public string Title { get; init; } = "Live";

    public string ColorHex { get; init; } = "#52E3FF";

    public int SortIndex { get; init; }

    public IReadOnlyList<GalleryPageItemLayout> Items { get; init; } = [];

    public GalleryPageLayout Normalize(int fallbackSortIndex) =>
        this with
        {
            PageId = string.IsNullOrWhiteSpace(PageId) ? Guid.NewGuid().ToString("N") : PageId,
            Title = string.IsNullOrWhiteSpace(Title) ? "Live" : Title.Trim(),
            ColorHex = string.IsNullOrWhiteSpace(ColorHex) ? "#52E3FF" : ColorHex.Trim(),
            SortIndex = SortIndex < 0 ? fallbackSortIndex : SortIndex,
            Items = Items
                .Select((item, index) => item.Normalize(index))
                .Where(item => !string.IsNullOrWhiteSpace(item.GalleryItemId))
                .OrderBy(item => item.SortIndex)
                .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };
}
