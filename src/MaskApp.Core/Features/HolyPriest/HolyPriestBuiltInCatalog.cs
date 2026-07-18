namespace MaskApp.Core.Features.HolyPriest;

public static class HolyPriestBuiltInCatalog
{
    public const int OriginalSlot = 15;
    public const int InvertedSlot = 16;
    public const int RedSlot = 17;
    public const int BlueSlot = 18;
    public const int GoldSlot = 19;
    public const int BlackoutSlot = 20;

    public const string OriginalFaceId = "built-in-face-holy-priest-original";
    public const string InvertedFaceId = "built-in-face-holy-priest-inverted";
    public const string RedFaceId = "built-in-face-holy-priest-red";
    public const string BlueFaceId = "built-in-face-holy-priest-blue";
    public const string GoldFaceId = "built-in-face-holy-priest-gold";

    public const string BlackWhiteAnimationId = "holy-priest-black-white-flash";
    public const string BlueRedBlackAnimationId = "holy-priest-blue-red-black";
    public const string FiveMaskAnimationId = "holy-priest-five-mask-cycle";
    public const string ColorPulseAnimationId = "holy-priest-color-pulse";

    private static readonly HashSet<string> FaceIds =
    [
        OriginalFaceId,
        InvertedFaceId,
        RedFaceId,
        BlueFaceId,
        GoldFaceId
    ];

    private static readonly HashSet<string> GalleryItemIds =
    [
        $"face:{OriginalFaceId}",
        $"face:{InvertedFaceId}",
        $"face:{RedFaceId}",
        $"face:{BlueFaceId}",
        $"face:{GoldFaceId}",
        $"app-animation:{BlackWhiteAnimationId}",
        $"app-animation:{BlueRedBlackAnimationId}",
        $"app-animation:{FiveMaskAnimationId}",
        $"app-animation:{ColorPulseAnimationId}"
    ];

    public static bool IsFaceId(string id) => FaceIds.Contains(id);

    public static bool IsGalleryItemId(string id) => GalleryItemIds.Contains(MigrateGalleryItemId(id));

    public static string MigratePersistedFaceId(string id) => id switch
    {
        "built-in-face-holy-priest-cross" => OriginalFaceId,
        "built-in-face-holy-priest-antihero" => InvertedFaceId,
        "built-in-face-holy-priest-bass-powah" => RedFaceId,
        "built-in-face-holy-priest-atlantis" => BlueFaceId,
        "built-in-face-holy-priest-no-balance" => GoldFaceId,
        _ => id
    };

    public static string MigrateGalleryItemId(string? id)
    {
        var normalized = id?.Trim() ?? string.Empty;
        return normalized switch
        {
            "face:built-in-face-holy-priest-cross" => $"face:{OriginalFaceId}",
            "face:built-in-face-holy-priest-antihero" => $"face:{InvertedFaceId}",
            "face:built-in-face-holy-priest-bass-powah" => $"face:{RedFaceId}",
            "face:built-in-face-holy-priest-atlantis" => $"face:{BlueFaceId}",
            "face:built-in-face-holy-priest-no-balance" => $"face:{GoldFaceId}",
            "face:built-in-face-holy-priest-retro-future" => $"face:{OriginalFaceId}",
            "app-animation:holy-priest-cross-pulse" => $"app-animation:{BlackWhiteAnimationId}",
            "app-animation:holy-priest-red-mass" => $"app-animation:{BlueRedBlackAnimationId}",
            "app-animation:holy-priest-antihero-scan" => $"app-animation:{FiveMaskAnimationId}",
            "app-animation:holy-priest-atlantis-signal" => $"app-animation:{ColorPulseAnimationId}",
            "app-animation:holy-priest-no-balance" => $"app-animation:{ColorPulseAnimationId}",
            "app-animation:holy-priest-ritual-inversion" => $"app-animation:{FiveMaskAnimationId}",
            _ => normalized
        };
    }
}
