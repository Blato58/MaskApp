namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryLayoutState
{
    public const int CurrentSchemaVersion = 3;

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
            .ToList();
        if (SchemaVersion < CurrentSchemaVersion && !pages.Any(BuiltInGalleryPages.IsHolyPriestPage))
        {
            var sortIndex = pages.Count == 0 ? 0 : pages.Max(page => page.SortIndex) + 1;
            pages.Add(BuiltInGalleryPages.CreateHolyPriestPage(sortIndex).Normalize(sortIndex));
        }

        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            Order = Order ?? new GalleryOrderState(),
            Pages = pages
                .OrderBy(page => page.SortIndex)
                .ThenBy(page => page.Title, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Status = string.IsNullOrWhiteSpace(Status) ? "Ready." : Status.Trim()
        };
    }
}
