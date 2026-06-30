namespace MaskApp.Core.Features.Faces;

public static class FaceImageImport
{
    private const int LitThreshold = 40;

    public static FacePattern CreatePattern(
        FaceSampleImage source,
        string displayName,
        FacePatternSource patternSource,
        int preferredSlot)
    {
        source = source.Normalize();
        var pixels = TransformToFacePixels(source);
        return new FacePattern
        {
            Id = $"face-{Guid.NewGuid():N}",
            DisplayName = displayName,
            Emotion = FaceEmotion.Custom,
            Source = patternSource,
            PreferredSlot = preferredSlot,
            Pixels = pixels
        }.Normalize();
    }

    public static FacePixel[] TransformToFacePixels(FaceSampleImage source)
    {
        source = source.Normalize();
        var crop = CalculateCenterCrop(source.Width, source.Height);
        var target = new FacePixel[FacePattern.PixelCount];
        var index = 0;

        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width; column++)
            {
                var sourceX = crop.Left + (int)Math.Clamp(
                    Math.Floor((column + 0.5) * crop.Width / FacePattern.Width),
                    0,
                    crop.Width - 1);
                var sourceY = crop.Top + (int)Math.Clamp(
                    Math.Floor((row + 0.5) * crop.Height / FacePattern.Height),
                    0,
                    crop.Height - 1);
                var sample = source.GetPixel(sourceX, sourceY);
                var lit = sample.IsVisible && sample.Luminance >= LitThreshold;
                target[index++] = lit ? new FacePixel(true, sample.ToFaceColor()) : FacePixel.Off;
            }
        }

        return target;
    }

    private static CropRect CalculateCenterCrop(int width, int height)
    {
        const double targetAspect = FacePattern.Width / (double)FacePattern.Height;
        var sourceAspect = width / (double)height;

        if (sourceAspect > targetAspect)
        {
            var cropWidth = Math.Max(1, (int)Math.Round(height * targetAspect));
            return new CropRect((width - cropWidth) / 2, 0, cropWidth, height);
        }

        var cropHeight = Math.Max(1, (int)Math.Round(width / targetAspect));
        return new CropRect(0, (height - cropHeight) / 2, width, cropHeight);
    }

    private readonly record struct CropRect(int Left, int Top, int Width, int Height);
}
