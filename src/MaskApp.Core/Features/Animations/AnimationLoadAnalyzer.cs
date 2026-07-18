using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public sealed record AnimationLoadAssessment(
    double AverageCadenceHz,
    double PeakCadenceHz,
    double TimeWeightedLitAreaRatio,
    double TimeWeightedBrightness,
    bool HasHighSustainedLoad);

public sealed class AnimationLoadAnalyzer
{
    public AnimationLoadAssessment Analyze(PerformanceAnimation source)
    {
        var animation = source.Normalize();
        var patterns = animation.StoredFrames.ToDictionary(frame => frame.Slot, frame => frame.Pattern);
        var cycleSeconds = animation.CycleDuration.TotalSeconds;
        var weightedLitArea = 0d;
        var weightedBrightness = 0d;
        foreach (var frame in animation.Frames)
        {
            var pattern = patterns[frame.Slot];
            var litPixels = 0;
            var brightness = 0d;
            foreach (var pixel in pattern.Pixels)
            {
                if (!pixel.IsLit)
                {
                    continue;
                }

                litPixels++;
                brightness += Math.Max(pixel.Color.Red, Math.Max(pixel.Color.Green, pixel.Color.Blue)) / 255d;
            }

            var durationRatio = frame.Duration.TotalSeconds / cycleSeconds;
            weightedLitArea += (litPixels / (double)FacePattern.PixelCount) * durationRatio;
            weightedBrightness += (brightness / FacePattern.PixelCount) * durationRatio;
        }

        var averageCadence = animation.Frames.Count / cycleSeconds;
        var peakCadence = 1 / animation.Frames.Min(frame => frame.Duration.TotalSeconds);
        var highSustainedLoad = averageCadence >= 10
            && weightedLitArea >= 0.50
            && weightedBrightness >= 0.35;
        return new AnimationLoadAssessment(
            averageCadence,
            peakCadence,
            weightedLitArea,
            weightedBrightness,
            highSustainedLoad);
    }
}
