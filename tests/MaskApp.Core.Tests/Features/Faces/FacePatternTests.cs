using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class FacePatternTests
{
    [Fact]
    public void Normalize_MigratesLegacyCanvasWithoutStretchingItsAspectRatio()
    {
        var legacyPixels = Enumerable.Repeat(FacePixel.Off, 36 * 12).ToArray();
        legacyPixels[0] = new FacePixel(true, new FaceColor(0x11, 0x22, 0x33));
        legacyPixels[^1] = new FacePixel(true, new FaceColor(0xFA, 0xCC, 0x15));
        var legacy = new FacePattern
        {
            Id = "legacy-face",
            Pixels = legacyPixels
        };

        var migrated = legacy.Normalize();

        Assert.Equal(46, FacePattern.Width);
        Assert.Equal(58, FacePattern.Height);
        Assert.Equal(46 * 58, migrated.Pixels.Length);
        Assert.Equal(new FaceColor(0x11, 0x22, 0x33), migrated.GetPixel(0, 21).Color);
        Assert.Equal(new FaceColor(0xFA, 0xCC, 0x15), migrated.GetPixel(45, 35).Color);
        Assert.All(Enumerable.Range(0, 21), row =>
            Assert.DoesNotContain(Enumerable.Range(0, FacePattern.Width), column => migrated.GetPixel(column, row).IsLit));
        Assert.All(Enumerable.Range(36, FacePattern.Height - 36), row =>
            Assert.DoesNotContain(Enumerable.Range(0, FacePattern.Width), column => migrated.GetPixel(column, row).IsLit));
    }

    [Fact]
    public void Normalize_LeavesNativeCanvasCoordinatesUnchanged()
    {
        var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        pixels[(3 * FacePattern.Width) + 2] = new FacePixel(true, new FaceColor(1, 2, 3));

        var normalized = new FacePattern { Id = "native-face", Pixels = pixels }.Normalize();

        Assert.True(normalized.GetPixel(2, 3).IsLit);
        Assert.Equal(new FaceColor(1, 2, 3), normalized.GetPixel(2, 3).Color);
    }
}
