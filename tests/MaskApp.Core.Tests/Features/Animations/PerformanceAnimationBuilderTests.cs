using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class PerformanceAnimationBuilderTests
{
    [Fact]
    public void FromAppBuiltIn_DeduplicatesIdenticalStoredFrames_AndRewritesPlaybackSlots()
    {
        var pattern = FacePatternFactory.CreateBlank("Same", preferredSlot: 15);
        var animation = new AppBuiltInAnimation
        {
            Id = "dedupe",
            DisplayName = "Dedupe",
            Frames =
            [
                new AppBuiltInAnimationFrame { Slot = 15, Pattern = pattern },
                new AppBuiltInAnimationFrame { Slot = 16, Pattern = pattern with { PreferredSlot = 16 } }
            ],
            PlaybackSlots = [15, 16, 15, 16]
        };

        var result = new PerformanceAnimationBuilder().FromAppBuiltIn(animation);

        var stored = Assert.Single(result.StoredFrames);
        Assert.Equal(15, stored.Slot);
        Assert.All(result.Frames, frame => Assert.Equal(15, frame.Slot));
        Assert.Equal(4, result.Frames.Count);
        Assert.Equal(64, result.RevisionHash.Length);
    }

    [Fact]
    public void WithBpm_ChangesTimingAndRevision_ButNotPixels()
    {
        var builder = new PerformanceAnimationBuilder();
        var original = builder.FromAppBuiltIn(AppBuiltInAnimationCatalog.CreateBuiltIns()[0]);

        var faster = builder.WithBpm(original, 240);

        Assert.All(faster.Frames, frame => Assert.Equal(TimeSpan.FromMilliseconds(37.5), frame.Duration));
        Assert.Equal(
            original.StoredFrames.Select(frame => (frame.Slot, frame.ContentFingerprint)),
            faster.StoredFrames.Select(frame => (frame.Slot, frame.ContentFingerprint)));
        Assert.NotEqual(original.RevisionHash, faster.RevisionHash);
    }

    [Fact]
    public void FromAppBuiltIn_RejectsAUniqueFrameCountBeyondVerifiedBudget()
    {
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[1];

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new PerformanceAnimationBuilder().FromAppBuiltIn(animation, slotBudget: 2));

        Assert.Contains("2-slot budget", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TapTempo_UsesMedianAndResetsAfterLongGap()
    {
        var tracker = new TapTempoTracker();

        Assert.Null(tracker.AddTap(TimeSpan.Zero));
        Assert.Equal(120, tracker.AddTap(TimeSpan.FromMilliseconds(500)).GetValueOrDefault());
        Assert.Equal(119, tracker.AddTap(TimeSpan.FromMilliseconds(1010)).GetValueOrDefault(), precision: 0);
        Assert.Null(tracker.AddTap(TimeSpan.FromSeconds(4)));
        Assert.Equal(60, tracker.AddTap(TimeSpan.FromSeconds(5)).GetValueOrDefault());
    }

    [Fact]
    public void LoadAnalyzer_ReportsCadenceAndHighBrightDutyCycle()
    {
        var animation = FlashSafetyAnalyzerTests.CreateBlackWhiteAnimation(
            TimeSpan.FromMilliseconds(50),
            AnimationLoopMode.Continuous,
            1,
            [2, 2, 2, 1]);

        var result = new AnimationLoadAnalyzer().Analyze(animation);

        Assert.Equal(20, result.AverageCadenceHz, precision: 1);
        Assert.Equal(20, result.PeakCadenceHz, precision: 1);
        Assert.True(result.TimeWeightedLitAreaRatio > 0.70);
        Assert.True(result.HasHighSustainedLoad);
    }
}
