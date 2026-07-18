namespace MaskApp.Core.Features.Gallery;

public static class BuiltInGalleryPages
{
    public const string HolyPriestPageId = "built-in-page-holy-priest";

    public static IReadOnlyList<GalleryPageLayout> CreateDefaults() =>
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
        },
        CreateHolyPriestPage(2)
    ];

    public static GalleryPageLayout CreateHolyPriestPage(int sortIndex) =>
        new()
        {
            PageId = HolyPriestPageId,
            Title = "Holy Priest",
            ColorHex = "#FF3B30",
            SortIndex = sortIndex,
            Items =
            [
                CreateItem("holy-priest-anim-flash", "app-animation:holy-priest-cross-pulse", "Black / White Flash", "anim", "#FFFFFF", 0),
                CreateItem("holy-priest-anim-red-mass", "app-animation:holy-priest-red-mass", "Red Mass", "anim", "#FF3B30", 1),
                CreateItem("holy-priest-anim-antihero", "app-animation:holy-priest-antihero-scan", "Antihero Scan", "anim", "#52E3FF", 2),
                CreateItem("holy-priest-anim-atlantis", "app-animation:holy-priest-atlantis-signal", "Atlantis Signal", "anim", "#0A84FF", 3),
                CreateItem("holy-priest-anim-no-balance", "app-animation:holy-priest-no-balance", "No Balance", "anim", "#BF5AF2", 4),
                CreateItem("holy-priest-anim-inversion", "app-animation:holy-priest-ritual-inversion", "Ritual Inversion", "anim", "#FF9F0A", 5),
                CreateItem("holy-priest-face-cross", "face:built-in-face-holy-priest-cross", "Iconic Cross", "face", "#FFFFFF", 6),
                CreateItem("holy-priest-face-antihero", "face:built-in-face-holy-priest-antihero", "Masked Antihero", "face", "#52E3FF", 7),
                CreateItem("holy-priest-face-bass", "face:built-in-face-holy-priest-bass-powah", "Bass Pistons", "face", "#FF3B30", 8),
                CreateItem("holy-priest-face-atlantis", "face:built-in-face-holy-priest-atlantis", "Atlantis Sonar", "face", "#0A84FF", 9),
                CreateItem("holy-priest-face-balance", "face:built-in-face-holy-priest-no-balance", "No Balance Face", "face", "#BF5AF2", 10),
                CreateItem("holy-priest-face-retro", "face:built-in-face-holy-priest-retro-future", "90s → Future", "face", "#FF2D55", 11)
            ]
        };

    public static bool IsHolyPriestPage(GalleryPageLayout page) =>
        string.Equals(page.PageId, HolyPriestPageId, StringComparison.Ordinal) ||
        string.Equals(page.Title.Trim(), "Holy Priest", StringComparison.OrdinalIgnoreCase);

    private static GalleryPageItemLayout CreateItem(
        string slotId,
        string galleryItemId,
        string label,
        string iconKey,
        string colorHex,
        int sortIndex) =>
        new()
        {
            SlotId = slotId,
            GalleryItemId = galleryItemId,
            Label = label,
            IconKey = iconKey,
            ColorHex = colorHex,
            SortIndex = sortIndex
        };
}
