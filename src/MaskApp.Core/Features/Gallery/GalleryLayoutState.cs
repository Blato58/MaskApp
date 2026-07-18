using MaskApp.Core.Features.HolyPriest;

namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryLayoutState
{
    public const int CurrentSchemaVersion = 4;
    private const int HolyPriestPageIntroducedSchemaVersion = 3;
    private const int HolyPriestCatalogRefreshSchemaVersion = 4;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public GalleryOrderState Order { get; init; } = new();

    public IReadOnlyList<GalleryPageLayout> Pages { get; init; } = BuiltInGalleryPages.CreateDefaults();

    public string Status { get; init; } = "Ready.";

    public bool UsedFallback { get; init; }

    public GalleryLayoutState Normalize()
    {
        var pages = (Pages is null || Pages.Count == 0
                ? BuiltInGalleryPages.CreateDefaults()
                : Pages)
            .Select((page, index) => page.Normalize(index))
            .Select(MigrateLegacyReferences)
            .ToList();
        if (SchemaVersion < HolyPriestPageIntroducedSchemaVersion && !pages.Any(BuiltInGalleryPages.IsHolyPriestPage))
        {
            var sortIndex = pages.Count == 0 ? 0 : pages.Max(page => page.SortIndex) + 1;
            pages.Add(BuiltInGalleryPages.CreateHolyPriestPage(sortIndex).Normalize(sortIndex));
        }

        if (SchemaVersion < HolyPriestCatalogRefreshSchemaVersion)
        {
            pages = pages
                .Select(page => string.Equals(page.PageId, BuiltInGalleryPages.HolyPriestPageId, StringComparison.Ordinal)
                    ? RefreshHolyPriestPage(page)
                    : page)
                .ToList();
        }

        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            Order = MigrateLegacyReferences(Order ?? new GalleryOrderState()),
            Pages = pages
                .OrderBy(page => page.SortIndex)
                .ThenBy(page => page.Title, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Status = string.IsNullOrWhiteSpace(Status) ? "Ready." : Status.Trim()
        };
    }

    private static GalleryPageLayout MigrateLegacyReferences(GalleryPageLayout page) =>
        (page with
        {
            Items = page.Items
                .Select(item => item with
                {
                    GalleryItemId = HolyPriestBuiltInCatalog.MigrateGalleryItemId(item.GalleryItemId)
                })
                .GroupBy(item => item.GalleryItemId, StringComparer.Ordinal)
                .Select(group => group.OrderBy(item => item.SortIndex).First())
                .ToArray()
        }).Normalize(page.SortIndex);

    private static GalleryPageLayout RefreshHolyPriestPage(GalleryPageLayout page)
    {
        var canonical = BuiltInGalleryPages.CreateHolyPriestPage(page.SortIndex);
        var extras = page.Items
            .Where(item => !HolyPriestBuiltInCatalog.IsGalleryItemId(item.GalleryItemId))
            .Select((item, index) => item with { SortIndex = canonical.Items.Count + index });
        return (page with { Items = canonical.Items.Concat(extras).ToArray() }).Normalize(page.SortIndex);
    }

    private static GalleryOrderState MigrateLegacyReferences(GalleryOrderState order) =>
        order with
        {
            ItemOrders = (order.ItemOrders ?? [])
                .Select(item => item with
                {
                    ItemId = HolyPriestBuiltInCatalog.MigrateGalleryItemId(item.ItemId)
                })
                .GroupBy(item => item.ItemId, StringComparer.Ordinal)
                .Select(group => group.OrderBy(item => item.SortIndex).First())
                .OrderBy(item => item.SortIndex)
                .ThenBy(item => item.ItemId, StringComparer.Ordinal)
                .ToArray()
        };
}
