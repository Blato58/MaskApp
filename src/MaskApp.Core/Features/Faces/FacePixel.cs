namespace MaskApp.Core.Features.Faces;

public readonly record struct FacePixel(bool IsLit, FaceColor Color)
{
    public static FacePixel Off { get; } = new(false, FaceColor.Black);

    public FacePixel Normalize() => IsLit ? this : Off;
}
