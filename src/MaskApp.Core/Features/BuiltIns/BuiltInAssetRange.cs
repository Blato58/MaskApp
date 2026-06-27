namespace MaskApp.Core.Features.BuiltIns;

public static class BuiltInAssetRange
{
    public const int MinId = 0;
    public const int StaticImageSafeMaxId = 0x69;
    public const int AnimationSafeMaxId = 0x45;

    public static int GetSafeMaxId(BuiltInAssetType type) =>
        type == BuiltInAssetType.StaticImage ? StaticImageSafeMaxId : AnimationSafeMaxId;

    public static bool IsInSafeRange(BuiltInAssetType type, int id) =>
        id is >= MinId && id <= GetSafeMaxId(type);

    public static int Clamp(BuiltInAssetType type, int id) =>
        Math.Clamp(id, MinId, GetSafeMaxId(type));

    public static string ToHexId(int id) => $"0x{id:X2}";
}
