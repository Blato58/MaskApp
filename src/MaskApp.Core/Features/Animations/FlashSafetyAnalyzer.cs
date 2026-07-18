using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public sealed class FlashSafetyAnalyzer
{
    public const int MaximumDefaultFlashesPerSecond = 3;
    private const double MinimumAffectedAreaRatio = 0.25;
    private const double MinimumBrightnessDelta = 0.10;
    private const double MinimumContrastRatio = 3.0;
    private static readonly TimeSpan FlashWindow = TimeSpan.FromSeconds(1);

    public FlashSafetyAssessment Analyze(PerformanceAnimation source)
    {
        var animation = source.Normalize();
        if (string.IsNullOrWhiteSpace(animation.RevisionHash))
        {
            throw new ArgumentException("Animation must have a revision hash before safety analysis.", nameof(source));
        }

        var patterns = animation.StoredFrames.ToDictionary(frame => frame.Slot, frame => frame.Pattern);
        var transitionTemplates = BuildTransitionTemplates(animation, patterns);
        var occurrenceCount = GetAnalyzedCycleCount(animation);
        var findings = new List<FlashTransitionFinding>();
        var cycleDuration = animation.CycleDuration;
        for (var cycle = 0; cycle < occurrenceCount; cycle++)
        {
            var offset = TimeSpan.FromTicks(cycleDuration.Ticks * cycle);
            foreach (var template in transitionTemplates)
            {
                if (template.IsLoopBoundary && !HasBoundaryAfterCycle(animation, cycle, occurrenceCount))
                {
                    continue;
                }

                if (template.IsFlash)
                {
                    findings.Add(new FlashTransitionFinding(
                        template.FromFrameIndex,
                        template.ToFrameIndex,
                        offset + template.OccursAt,
                        template.AffectedAreaRatio,
                        template.AverageBrightnessDelta,
                        template.ContrastRatio));
                }
            }
        }

        var ordered = findings.OrderBy(item => item.OccursAt).ToArray();
        var maximumFlashes = FindMaximumWindowCount(ordered);
        return new FlashSafetyAssessment
        {
            ContentId = animation.Id,
            RevisionHash = animation.RevisionHash,
            MaximumFlashesPerSecond = maximumFlashes,
            AnalyzedTransitionCount = transitionTemplates.Count * occurrenceCount,
            MaximumAffectedAreaRatio = transitionTemplates.Count == 0
                ? 0
                : transitionTemplates.Max(item => item.AffectedAreaRatio),
            MaximumBrightnessDelta = transitionTemplates.Count == 0
                ? 0
                : transitionTemplates.Max(item => item.AverageBrightnessDelta),
            MaximumContrastRatio = transitionTemplates.Count == 0
                ? 1
                : transitionTemplates.Max(item => item.ContrastRatio),
            FlashTransitions = ordered
        };
    }

    public FlashSafetyDecision Decide(
        FlashSafetyAssessment assessment,
        FlashSafetyAcknowledgementState acknowledgementState)
    {
        if (assessment.IsSafeByDefault)
        {
            return new FlashSafetyDecision(
                FlashSafetyStatus.Safe,
                $"Safety analysis found at most {assessment.MaximumFlashesPerSecond} full flash(es) in one second.",
                null);
        }

        var acknowledgement = acknowledgementState.Normalize().Acknowledgements.FirstOrDefault(item =>
            string.Equals(item.ContentId, assessment.ContentId, StringComparison.Ordinal)
            && string.Equals(item.RevisionHash, assessment.RevisionHash, StringComparison.OrdinalIgnoreCase));
        if (acknowledgement is not null)
        {
            return new FlashSafetyDecision(
                FlashSafetyStatus.AcknowledgedOverride,
                $"Explicit override recorded for this exact revision; analysis found {assessment.MaximumFlashesPerSecond} flashes in one second.",
                acknowledgement);
        }

        return new FlashSafetyDecision(
            FlashSafetyStatus.Blocked,
            $"Playback blocked: analysis found {assessment.MaximumFlashesPerSecond} full flashes in one second; the default maximum is {MaximumDefaultFlashesPerSecond}.",
            null);
    }

    private static IReadOnlyList<TransitionTemplate> BuildTransitionTemplates(
        PerformanceAnimation animation,
        IReadOnlyDictionary<int, FacePattern> patterns)
    {
        var result = new List<TransitionTemplate>();
        var occursAt = TimeSpan.Zero;
        for (var index = 0; index < animation.Frames.Count; index++)
        {
            occursAt += animation.Frames[index].Duration;
            var hasNext = index + 1 < animation.Frames.Count;
            if (!hasNext && animation.LoopMode == AnimationLoopMode.Finite && animation.FiniteLoopCount == 1)
            {
                continue;
            }

            var nextIndex = hasNext ? index + 1 : 0;
            result.Add(AnalyzeTransition(
                patterns[animation.Frames[index].Slot],
                patterns[animation.Frames[nextIndex].Slot],
                index,
                nextIndex,
                occursAt,
                isLoopBoundary: !hasNext));
        }

        return result;
    }

    private static TransitionTemplate AnalyzeTransition(
        FacePattern from,
        FacePattern to,
        int fromFrameIndex,
        int toFrameIndex,
        TimeSpan occursAt,
        bool isLoopBoundary)
    {
        var affectedPixels = 0;
        var totalDelta = 0d;
        var fromBrightness = 0d;
        var toBrightness = 0d;
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width; column++)
            {
                var fromLuminance = RelativeLuminance(from.GetPixel(column, row));
                var toLuminance = RelativeLuminance(to.GetPixel(column, row));
                var delta = Math.Abs(fromLuminance - toLuminance);
                if (delta >= MinimumBrightnessDelta)
                {
                    affectedPixels++;
                }

                totalDelta += delta;
                fromBrightness += fromLuminance;
                toBrightness += toLuminance;
            }
        }

        var affectedArea = affectedPixels / (double)FacePattern.PixelCount;
        var averageDelta = totalDelta / FacePattern.PixelCount;
        var averageFrom = fromBrightness / FacePattern.PixelCount;
        var averageTo = toBrightness / FacePattern.PixelCount;
        var contrast = (Math.Max(averageFrom, averageTo) + 0.05)
            / (Math.Min(averageFrom, averageTo) + 0.05);
        var isFlash = affectedArea >= MinimumAffectedAreaRatio
            && averageDelta >= MinimumBrightnessDelta
            && contrast >= MinimumContrastRatio;
        return new TransitionTemplate(
            fromFrameIndex,
            toFrameIndex,
            occursAt,
            affectedArea,
            averageDelta,
            contrast,
            isLoopBoundary,
            isFlash);
    }

    private static double RelativeLuminance(FacePixel pixel)
    {
        if (!pixel.IsLit)
        {
            return 0;
        }

        static double Linearize(byte component)
        {
            var value = component / 255d;
            return value <= 0.04045
                ? value / 12.92
                : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Linearize(pixel.Color.Red))
            + (0.7152 * Linearize(pixel.Color.Green))
            + (0.0722 * Linearize(pixel.Color.Blue));
    }

    private static int GetAnalyzedCycleCount(PerformanceAnimation animation)
    {
        if (animation.LoopMode == AnimationLoopMode.Finite && animation.FiniteLoopCount == 1)
        {
            return 1;
        }

        var cyclesForWindow = (int)Math.Ceiling(FlashWindow.Ticks / (double)animation.CycleDuration.Ticks) + 2;
        return animation.LoopMode == AnimationLoopMode.Finite
            ? Math.Min(animation.FiniteLoopCount, cyclesForWindow)
            : cyclesForWindow;
    }

    private static bool HasBoundaryAfterCycle(
        PerformanceAnimation animation,
        int cycle,
        int analyzedCycleCount) =>
        animation.LoopMode == AnimationLoopMode.Continuous
        || cycle + 1 < Math.Min(animation.FiniteLoopCount, analyzedCycleCount);

    private static int FindMaximumWindowCount(IReadOnlyList<FlashTransitionFinding> findings)
    {
        var maximum = 0;
        var start = 0;
        for (var end = 0; end < findings.Count; end++)
        {
            while (findings[end].OccursAt - findings[start].OccursAt > FlashWindow)
            {
                start++;
            }

            maximum = Math.Max(maximum, end - start + 1);
        }

        return maximum;
    }

    private sealed record TransitionTemplate(
        int FromFrameIndex,
        int ToFrameIndex,
        TimeSpan OccursAt,
        double AffectedAreaRatio,
        double AverageBrightnessDelta,
        double ContrastRatio,
        bool IsLoopBoundary,
        bool IsFlash);
}
