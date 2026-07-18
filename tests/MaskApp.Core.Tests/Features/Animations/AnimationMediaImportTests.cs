using System.Text;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class AnimationMediaImportTests
{
    [Fact]
    public async Task Import_RejectsOversizedInputBeforeDecoderRuns()
    {
        var decoder = new FakeDecoder();
        var limits = new AnimationMediaImportLimits
        {
            MaxBytes = 32,
            MaxDimension = 64,
            MaxFrames = 10,
            MaxTotalPixels = 10_000,
            MaxDuration = TimeSpan.FromSeconds(2)
        };
        var service = new AnimationMediaImportService(decoder, limits);
        var bytes = GifHeader().Concat(new byte[40]).ToArray();

        var result = await service.ImportAsync(
            new MemoryStream(bytes),
            "Too large",
            AnimationMediaKind.Gif);

        Assert.False(result.Succeeded);
        Assert.False(decoder.WasCalled);
        Assert.Contains("limit", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Import_RejectsMismatchedSignatureBeforeDecoderRuns()
    {
        var decoder = new FakeDecoder();
        var service = new AnimationMediaImportService(decoder);

        var result = await service.ImportAsync(
            new MemoryStream(Encoding.ASCII.GetBytes("not-a-gif")),
            "Spoofed",
            AnimationMediaKind.Gif);

        Assert.False(result.Succeeded);
        Assert.False(decoder.WasCalled);
        Assert.Contains("not a GIF", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Import_RejectsHostileDecodedDimensionsAndPixelWork()
    {
        var image = SolidImage(65, 1, 255);
        var decoder = new FakeDecoder(AnimationMediaDecodeResult.Success(
            [new AnimationDecodedFrame(image, TimeSpan.FromMilliseconds(100))],
            TimeSpan.FromMilliseconds(100)));
        var limits = new AnimationMediaImportLimits
        {
            MaxBytes = 1024,
            MaxDimension = 64,
            MaxFrames = 10,
            MaxTotalPixels = 10_000,
            MaxDuration = TimeSpan.FromSeconds(2)
        };

        var result = await new AnimationMediaImportService(decoder, limits).ImportAsync(
            new MemoryStream(GifHeader()),
            "Wide",
            AnimationMediaKind.Gif);

        Assert.False(result.Succeeded);
        Assert.Contains("dimension limit", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Import_RejectsDecoderTimingThatExceedsBoundedSourceDuration()
    {
        var image = SolidImage(2, 2, 255);
        var decoder = new FakeDecoder(AnimationMediaDecodeResult.Success(
            [
                new AnimationDecodedFrame(image, TimeSpan.FromSeconds(2)),
                new AnimationDecodedFrame(image, TimeSpan.FromSeconds(2))
            ],
            TimeSpan.FromMilliseconds(100)));
        var limits = new AnimationMediaImportLimits
        {
            MaxBytes = 1024,
            MaxDimension = 64,
            MaxFrames = 10,
            MaxTotalPixels = 10_000,
            MaxDuration = TimeSpan.FromSeconds(2)
        };

        var result = await new AnimationMediaImportService(decoder, limits).ImportAsync(
            new MemoryStream(GifHeader()),
            "Bad timing",
            AnimationMediaKind.Gif);

        Assert.False(result.Succeeded);
        Assert.Contains("timing", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Import_ConvertsToPhysicalCanvas_RemovesConsecutiveDuplicates_AndPreservesTime()
    {
        var dark = SolidImage(4, 4, 0);
        var bright = SolidImage(4, 4, 255);
        var decoder = new FakeDecoder(AnimationMediaDecodeResult.Success(
            [
                new AnimationDecodedFrame(dark, TimeSpan.FromMilliseconds(40)),
                new AnimationDecodedFrame(dark, TimeSpan.FromMilliseconds(60)),
                new AnimationDecodedFrame(bright, TimeSpan.FromMilliseconds(125))
            ],
            TimeSpan.FromMilliseconds(225)));

        var result = await new AnimationMediaImportService(decoder).ImportAsync(
            new MemoryStream(GifHeader()),
            "Imported",
            AnimationMediaKind.Gif,
            new AnimationMediaConversionOptions { ResizeMode = AnimationResizeMode.Fit });

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.DecodedFrameCount);
        Assert.Equal(2, result.OutputFrameCount);
        Assert.Equal(1, result.RemovedDuplicateCount);
        Assert.Equal(TimeSpan.FromMilliseconds(100), result.Project!.Frames[0].Duration);
        Assert.Equal(TimeSpan.FromMilliseconds(125), result.Project.Frames[1].Duration);
        Assert.All(result.Project.Frames, frame => Assert.Equal(FacePattern.PixelCount, frame.Pattern.Pixels.Length));
        Assert.Equal(AnimationProjectSource.GifImport, result.Project.Source);
    }

    [Fact]
    public void Converter_FitLeavesLetterboxWhileCropFillsCanvas()
    {
        var source = SolidImage(100, 20, 255);

        var fit = AnimationMediaFrameConverter.Convert(source, new AnimationMediaConversionOptions
        {
            ResizeMode = AnimationResizeMode.Fit
        });
        var crop = AnimationMediaFrameConverter.Convert(source, new AnimationMediaConversionOptions
        {
            ResizeMode = AnimationResizeMode.Crop
        });

        Assert.True(fit.Count(pixel => pixel.IsLit) < FacePattern.PixelCount / 2);
        Assert.Equal(FacePattern.PixelCount, crop.Count(pixel => pixel.IsLit));
    }

    [Fact]
    public void Converter_EightColorAndMonochromeDitherUseBoundedPalettes()
    {
        var pixels = Enumerable.Range(0, 16)
            .Select(index => new FaceSamplePixel((byte)(index * 16), 100, 220))
            .ToArray();
        var source = new FaceSampleImage(4, 4, pixels);

        var eightColor = AnimationMediaFrameConverter.Convert(source, new AnimationMediaConversionOptions
        {
            PaletteMode = AnimationPaletteMode.EightColor,
            ResizeMode = AnimationResizeMode.Crop
        });
        var dithered = AnimationMediaFrameConverter.Convert(source, new AnimationMediaConversionOptions
        {
            PaletteMode = AnimationPaletteMode.Monochrome,
            DitherMode = AnimationDitherMode.FloydSteinberg,
            ResizeMode = AnimationResizeMode.Crop
        });

        Assert.All(eightColor.Where(pixel => pixel.IsLit), pixel =>
        {
            Assert.Contains(pixel.Color.Red, new byte[] { 0, 255 });
            Assert.Contains(pixel.Color.Green, new byte[] { 0, 255 });
            Assert.Contains(pixel.Color.Blue, new byte[] { 0, 255 });
        });
        Assert.All(dithered.Where(pixel => pixel.IsLit), pixel =>
            Assert.True(pixel.Color == FaceColor.Black || pixel.Color == new FaceColor(255, 255, 255)));
    }

    private static byte[] GifHeader() => Encoding.ASCII.GetBytes("GIF89a");

    private static FaceSampleImage SolidImage(int width, int height, byte value) => new(
        width,
        height,
        Enumerable.Repeat(new FaceSamplePixel(value, value, value), width * height).ToArray());

    private sealed class FakeDecoder(AnimationMediaDecodeResult? result = null) : IAnimationMediaDecoder
    {
        public bool WasCalled { get; private set; }

        public Task<AnimationMediaDecodeResult> DecodeAsync(
            ReadOnlyMemory<byte> data,
            AnimationMediaDecodeRequest request,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result ?? AnimationMediaDecodeResult.Failure("No fake result."));
        }
    }
}
