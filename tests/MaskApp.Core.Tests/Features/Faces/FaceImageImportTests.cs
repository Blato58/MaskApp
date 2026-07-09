using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class FaceImageImportTests
{
    [Fact]
    public void TransformToFacePixels_ClampsToNativeCanvasAndHandlesTransparencyAndDarkPixels()
    {
        var pixels = Enumerable.Range(0, FacePattern.PixelCount)
            .Select(index => new FaceSamplePixel(255, 255, 255, 255))
            .ToArray();
        pixels[0] = new FaceSamplePixel(255, 255, 255, 0);
        pixels[1] = new FaceSamplePixel(8, 8, 8, 255);
        pixels[2] = new FaceSamplePixel(255, 0, 0, 255);
        var image = new FaceSampleImage(FacePattern.Width, FacePattern.Height, pixels);

        var transformed = FaceImageImport.TransformToFacePixels(image);

        Assert.Equal(FacePattern.PixelCount, transformed.Length);
        Assert.False(transformed[0].IsLit);
        Assert.False(transformed[1].IsLit);
        Assert.True(transformed[2].IsLit);
        Assert.Equal(new FaceColor(255, 0, 0), transformed[2].Color);
    }

    [Fact]
    public void CreatePattern_UsesSourceNameSlotAndCustomEmotion()
    {
        var pixels = Enumerable.Repeat(new FaceSamplePixel(255, 255, 255, 255), FacePattern.PixelCount).ToArray();
        var image = new FaceSampleImage(FacePattern.Width, FacePattern.Height, pixels);

        var pattern = FaceImageImport.CreatePattern(image, "Imported", FacePatternSource.ImportedPhoto, 11);

        Assert.Equal("Imported", pattern.DisplayName);
        Assert.Equal(FacePatternSource.ImportedPhoto, pattern.Source);
        Assert.Equal(FaceEmotion.Custom, pattern.Emotion);
        Assert.Equal(11, pattern.PreferredSlot);
        Assert.Equal(FacePattern.PixelCount, pattern.Pixels.Length);
    }
}
