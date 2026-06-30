namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryItemOrder
{
    public string ItemId { get; init; } = string.Empty;

    public int SortIndex { get; init; }
}
