namespace MaskApp.Core.Features.Faces;

public readonly record struct FaceColor(byte Red, byte Green, byte Blue)
{
    public static FaceColor Black { get; } = new(0, 0, 0);

    public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";
}
