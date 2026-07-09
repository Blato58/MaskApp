namespace MaskApp.Core.Features.BuiltIns;

public static class BuiltInAssetRange
{
    public const int MinId = 0;
    public const int StaticImageSafeMaxId = 69;
    public const int AnimationSafeMaxId = 45;

    public static int GetSafeMaxId(BuiltInAssetType type) =>
        type == BuiltInAssetType.StaticImage ? StaticImageSafeMaxId : AnimationSafeMaxId;

    public static bool IsInSafeRange(BuiltInAssetType type, int id) =>
        BuiltInAssetCatalog.IsKnown(type, id);

    public static int Clamp(BuiltInAssetType type, int id) =>
        BuiltInAssetCatalog.ClampToKnownId(type, id);

    public static string ToHexId(int id) => $"0x{id:X2}";
}
