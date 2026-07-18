namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryLayoutState
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public GalleryOrderState Order { get; init; } = new();

    public IReadOnlyList<GalleryPageLayout> Pages { get; init; } =
    [
        new GalleryPageLayout
        {
            Title = "Live",
            ColorHex = "#52E3FF",
            SortIndex = 0
        },
        new GalleryPageLayout
        {
            Title = "RAVE",
            ColorHex = "#FACC15",
            SortIndex = 1
        }
    ];

    public string Status { get; init; } = "Ready.";

    public bool UsedFallback { get; init; }

    public GalleryLayoutState Normalize()
    {
        var pages = Pages is null || Pages.Count == 0 ? new GalleryLayoutState().Pages : Pages;
        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            Order = Order ?? new GalleryOrderState(),
            Pages = pages
                .Select((page, index) => page.Normalize(index))
                .OrderBy(page => page.SortIndex)
                .ThenBy(page => page.Title, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Status = string.IsNullOrWhiteSpace(Status) ? "Ready." : Status.Trim()
        };
    }
}
