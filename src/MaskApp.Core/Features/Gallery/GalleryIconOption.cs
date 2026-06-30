namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryIconOption(string IconKey, string Label, string ColorHex)
{
    public static IReadOnlyList<GalleryIconOption> Defaults { get; } =
    [
        new("txt", "TXT", "#52E3FF"),
        new("face", "FACE", "#A78BFA"),
        new("anim", "ANIM", "#FF3D8B"),
        new("rave", "RAVE", "#FACC15"),
        new("fav", "FAV", "#22C55E"),
        new("safe", "SAFE", "#FFFFFF"),
        new("pack", "PACK", "#F472B6")
    ];
}
