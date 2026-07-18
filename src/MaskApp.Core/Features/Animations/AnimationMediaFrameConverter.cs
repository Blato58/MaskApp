using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public static class AnimationMediaFrameConverter
{
    public static FacePixel[] Convert(
        FaceSampleImage source,
        AnimationMediaConversionOptions? conversionOptions = null)
    {
        var image = source.Normalize();
        var options = (conversionOptions ?? new AnimationMediaConversionOptions()).Normalize();
        var samples = SampleToCanvas(image, options);
        if (options.DitherMode == AnimationDitherMode.FloydSteinberg &&
            options.PaletteMode == AnimationPaletteMode.Monochrome)
        {
            ApplyFloydSteinberg(samples, options.LitThreshold);
        }

        var target = new FacePixel[FacePattern.PixelCount];
        for (var index = 0; index < samples.Length; index++)
        {
            var sample = samples[index];
            if (!sample.Visible || sample.Luminance < options.LitThreshold)
            {
                target[index] = FacePixel.Off;
                continue;
            }

            target[index] = new FacePixel(true, Quantize(sample, options.PaletteMode));
        }

        return target;
    }

    private static WorkingPixel[] SampleToCanvas(
        FaceSampleImage source,
        AnimationMediaConversionOptions options)
    {
        var target = new WorkingPixel[FacePattern.PixelCount];
        var targetAspect = FacePattern.Width / (double)FacePattern.Height;
        var sourceAspect = source.Width / (double)source.Height;
        double scale;
        if (options.ResizeMode == AnimationResizeMode.Crop)
        {
            scale = sourceAspect > targetAspect
                ? FacePattern.Height / (double)source.Height
                : FacePattern.Width / (double)source.Width;
        }
        else
        {
            scale = sourceAspect > targetAspect
                ? FacePattern.Width / (double)source.Width
                : FacePattern.Height / (double)source.Height;
        }

        var scaledWidth = source.Width * scale;
        var scaledHeight = source.Height * scale;
        var overflowX = scaledWidth - FacePattern.Width;
        var overflowY = scaledHeight - FacePattern.Height;
        var baseLeft = (FacePattern.Width - scaledWidth) / 2;
        var baseTop = (FacePattern.Height - scaledHeight) / 2;
        var left = baseLeft - (Math.Max(0, overflowX) * options.HorizontalPosition / 2)
            + (Math.Max(0, -overflowX) * options.HorizontalPosition / 2);
        var top = baseTop - (Math.Max(0, overflowY) * options.VerticalPosition / 2)
            + (Math.Max(0, -overflowY) * options.VerticalPosition / 2);

        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width; column++)
            {
                var sourceX = ((column + 0.5) - left) / scale - 0.5;
                var sourceY = ((row + 0.5) - top) / scale - 0.5;
                var index = (row * FacePattern.Width) + column;
                if (sourceX < 0 || sourceX >= source.Width || sourceY < 0 || sourceY >= source.Height)
                {
                    if (options.ResizeMode == AnimationResizeMode.Fit)
                    {
                        target[index] = new WorkingPixel(0, 0, 0, false);
                        continue;
                    }

                    sourceX = Math.Clamp(sourceX, 0, source.Width - 1);
                    sourceY = Math.Clamp(sourceY, 0, source.Height - 1);
                }

                var sample = source.GetPixel(
                    Math.Clamp((int)Math.Round(sourceX), 0, source.Width - 1),
                    Math.Clamp((int)Math.Round(sourceY), 0, source.Height - 1));
                target[index] = new WorkingPixel(sample.Red, sample.Green, sample.Blue, sample.IsVisible);
            }
        }

        return target;
    }

    private static FaceColor Quantize(WorkingPixel pixel, AnimationPaletteMode mode) => mode switch
    {
        AnimationPaletteMode.EightColor => new FaceColor(
            pixel.Red >= 128 ? (byte)255 : (byte)0,
            pixel.Green >= 128 ? (byte)255 : (byte)0,
            pixel.Blue >= 128 ? (byte)255 : (byte)0),
        AnimationPaletteMode.Monochrome => pixel.Luminance >= 128
            ? new FaceColor(255, 255, 255)
            : FaceColor.Black,
        _ => new FaceColor(ToByte(pixel.Red), ToByte(pixel.Green), ToByte(pixel.Blue))
    };

    private static void ApplyFloydSteinberg(WorkingPixel[] pixels, int threshold)
    {
        var luminance = pixels.Select(pixel => pixel.Visible ? pixel.Luminance : 0).ToArray();
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width; column++)
            {
                var index = (row * FacePattern.Width) + column;
                if (!pixels[index].Visible)
                {
                    continue;
                }

                var oldValue = luminance[index];
                var newValue = oldValue >= threshold ? 255d : 0d;
                var error = oldValue - newValue;
                pixels[index] = new WorkingPixel(newValue, newValue, newValue, true);
                AddError(luminance, column + 1, row, error * 7 / 16);
                AddError(luminance, column - 1, row + 1, error * 3 / 16);
                AddError(luminance, column, row + 1, error * 5 / 16);
                AddError(luminance, column + 1, row + 1, error / 16);
            }
        }
    }

    private static void AddError(double[] values, int column, int row, double error)
    {
        if (column < 0 || column >= FacePattern.Width || row < 0 || row >= FacePattern.Height)
        {
            return;
        }

        var index = (row * FacePattern.Width) + column;
        values[index] = Math.Clamp(values[index] + error, 0, 255);
    }

    private static byte ToByte(double value) => (byte)Math.Clamp((int)Math.Round(value), 0, 255);

    private readonly record struct WorkingPixel(double Red, double Green, double Blue, bool Visible)
    {
        public double Luminance => (Red * 299 + Green * 587 + Blue * 114) / 1000;
    }
}
