namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryListRow
{
    private GalleryListRow(
        string title,
        string subtitle,
        GalleryItemCard? left,
        GalleryItemCard? right,
        bool isGroupHeader,
        bool isItemRow)
    {
        Title = title;
        Subtitle = subtitle;
        Left = left;
        Right = right;
        IsGroupHeader = isGroupHeader;
        IsItemRow = isItemRow;
        HasRight = right is not null;
    }

    public string Title { get; }

    public string Subtitle { get; }

    public GalleryItemCard? Left { get; }

    public GalleryItemCard? Right { get; }

    public bool IsGroupHeader { get; }

    public bool IsItemRow { get; }

    public bool HasRight { get; }

    public static GalleryListRow GroupHeader(string title, int count) =>
        new(title, $"{count} items", null, null, isGroupHeader: true, isItemRow: false);

    public static GalleryListRow ItemPair(GalleryItemCard left, GalleryItemCard? right) =>
        new(string.Empty, string.Empty, left, right, isGroupHeader: false, isItemRow: true);
}
