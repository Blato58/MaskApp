namespace MaskApp.Core.Features.Faces;

public readonly record struct FaceSamplePixel(byte Red, byte Green, byte Blue, byte Alpha = 255)
{
    public bool IsVisible => Alpha >= 24;

    public int Luminance => (Red * 299 + Green * 587 + Blue * 114) / 1000;

    public FaceColor ToFaceColor() => new(Red, Green, Blue);
}
