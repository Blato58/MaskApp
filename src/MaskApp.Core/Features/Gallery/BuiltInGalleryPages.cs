using MaskApp.Core.Features.HolyPriest;

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
                CreateItem("holy-priest-face-original", $"face:{HolyPriestBuiltInCatalog.OriginalFaceId}", "Original", "face", "#FFFFFF", 0),
                CreateItem("holy-priest-face-inverted", $"face:{HolyPriestBuiltInCatalog.InvertedFaceId}", "Inverted", "face", "#FFFFFF", 1),
                CreateItem("holy-priest-face-red", $"face:{HolyPriestBuiltInCatalog.RedFaceId}", "Red", "face", "#FF0000", 2),
                CreateItem("holy-priest-face-blue", $"face:{HolyPriestBuiltInCatalog.BlueFaceId}", "Blue", "face", "#0000FF", 3),
                CreateItem("holy-priest-face-gold", $"face:{HolyPriestBuiltInCatalog.GoldFaceId}", "Gold", "face", "#FFFF00", 4),
                CreateItem("holy-priest-anim-flash", $"app-animation:{HolyPriestBuiltInCatalog.BlackWhiteAnimationId}", "Black / White Flash", "anim", "#FFFFFF", 5),
                CreateItem("holy-priest-anim-color-drop", $"app-animation:{HolyPriestBuiltInCatalog.BlueRedBlackAnimationId}", "Blue → Red → Black", "anim", "#0A84FF", 6),
                CreateItem("holy-priest-anim-five-mask", $"app-animation:{HolyPriestBuiltInCatalog.FiveMaskAnimationId}", "Five Mask Cycle", "anim", "#FFD60A", 7),
                CreateItem("holy-priest-anim-color-pulse", $"app-animation:{HolyPriestBuiltInCatalog.ColorPulseAnimationId}", "Color Pulse", "anim", "#FF3B30", 8)
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
