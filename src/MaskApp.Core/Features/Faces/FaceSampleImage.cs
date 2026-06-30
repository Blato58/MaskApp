namespace MaskApp.Core.Features.Faces;

public sealed record FaceSampleImage(int Width, int Height, FaceSamplePixel[] Pixels)
{
    public FaceSampleImage Normalize()
    {
        if (Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Width), "Image width must be positive.");
        }

        if (Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Height), "Image height must be positive.");
        }

        if (Pixels.Length != Width * Height)
        {
            throw new ArgumentException("Pixel count must match image dimensions.", nameof(Pixels));
        }

        return this;
    }

    public FaceSamplePixel GetPixel(int x, int y)
    {
        var clampedX = Math.Clamp(x, 0, Width - 1);
        var clampedY = Math.Clamp(y, 0, Height - 1);
        return Pixels[(clampedY * Width) + clampedX];
    }
}
