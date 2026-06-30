namespace MaskApp.Core.Features.Faces;

public sealed record FaceColorOption(string Name, FaceColor Color)
{
    public string Hex => Color.Hex;

    public static IReadOnlyList<FaceColorOption> Defaults { get; } =
    [
        new("Yellow", new FaceColor(0xFA, 0xCC, 0x15)),
        new("White", new FaceColor(0xFF, 0xFF, 0xFF)),
        new("Cyan", new FaceColor(0x52, 0xE3, 0xFF)),
        new("Pink", new FaceColor(0xF4, 0x72, 0xB6)),
        new("Green", new FaceColor(0x22, 0xC5, 0x5E)),
        new("Red", new FaceColor(0xEF, 0x44, 0x44)),
        new("Purple", new FaceColor(0xA8, 0x55, 0xF7))
    ];
}
