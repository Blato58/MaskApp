namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryOrderState
{
    public IReadOnlyList<GalleryItemOrder> ItemOrders { get; init; } = [];

    public IReadOnlyList<GalleryGroupOrder> GroupOrders { get; init; } = [];

    public int GetItemSortIndex(string itemId, int fallback) =>
        ItemOrders.FirstOrDefault(order => order.ItemId == itemId)?.SortIndex ?? fallback;

    public int GetGroupSortIndex(string groupKey, int fallback) =>
        GroupOrders.FirstOrDefault(order => order.GroupKey == groupKey)?.SortIndex ?? fallback;

    public GalleryOrderState WithItemSortIndex(string itemId, int sortIndex)
    {
        var orders = ItemOrders
            .Where(order => !string.Equals(order.ItemId, itemId, StringComparison.Ordinal))
            .Append(new GalleryItemOrder { ItemId = itemId, SortIndex = sortIndex })
            .OrderBy(order => order.SortIndex)
            .ThenBy(order => order.ItemId, StringComparer.Ordinal)
            .ToArray();
        return this with { ItemOrders = orders };
    }

    public GalleryOrderState WithGroupSortIndex(string groupKey, int sortIndex)
    {
        var orders = GroupOrders
            .Where(order => !string.Equals(order.GroupKey, groupKey, StringComparison.Ordinal))
            .Append(new GalleryGroupOrder { GroupKey = groupKey, SortIndex = sortIndex })
            .OrderBy(order => order.SortIndex)
            .ThenBy(order => order.GroupKey, StringComparer.Ordinal)
            .ToArray();
        return this with { GroupOrders = orders };
    }
}
