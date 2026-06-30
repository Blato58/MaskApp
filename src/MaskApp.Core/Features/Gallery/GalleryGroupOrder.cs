namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryGroupOrder
{
    public string GroupKey { get; init; } = string.Empty;

    public int SortIndex { get; init; }
}
