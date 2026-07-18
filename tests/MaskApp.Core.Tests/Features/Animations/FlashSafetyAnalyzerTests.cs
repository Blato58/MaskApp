using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class FlashSafetyAnalyzerTests
{
    [Fact]
    public void Analyze_ThreeFlashesInOneSecond_IsAllowedBoundary()
    {
        var animation = CreateBlackWhiteAnimation(
            frameDuration: TimeSpan.FromMilliseconds(250),
            loopMode: AnimationLoopMode.Finite,
            finiteLoops: 1,
            playbackSlots: [1, 2, 1, 2]);

        var result = new FlashSafetyAnalyzer().Analyze(animation);

        Assert.Equal(3, result.MaximumFlashesPerSecond);
        Assert.True(result.IsSafeByDefault);
    }

    [Fact]
    public void Analyze_FourFlashesInOneSecond_IsBlocked()
    {
        var animation = CreateBlackWhiteAnimation(
            frameDuration: TimeSpan.FromMilliseconds(200),
            loopMode: AnimationLoopMode.Finite,
            finiteLoops: 1,
            playbackSlots: [1, 2, 1, 2, 1]);

        var result = new FlashSafetyAnalyzer().Analyze(animation);

        Assert.Equal(4, result.MaximumFlashesPerSecond);
        Assert.False(result.IsSafeByDefault);
    }

    [Fact]
    public void Analyze_ContinuousLoop_IncludesLastToFirstBoundary()
    {
        var animation = CreateBlackWhiteAnimation(
            frameDuration: TimeSpan.FromMilliseconds(300),
            loopMode: AnimationLoopMode.Continuous,
            finiteLoops: 1,
            playbackSlots: [1, 1, 2]);

        var result = new FlashSafetyAnalyzer().Analyze(animation);

        Assert.Contains(result.FlashTransitions, finding =>
            finding.FromFrameIndex == 2 && finding.ToFrameIndex == 0);
    }

    [Fact]
    public void Decide_OverrideMatchesOnlyExactContentAndTimingRevision()
    {
        var analyzer = new FlashSafetyAnalyzer();
        var builder = new PerformanceAnimationBuilder();
        var unsafeAnimation = CreateBlackWhiteAnimation(
            TimeSpan.FromMilliseconds(100),
            AnimationLoopMode.Continuous,
            1,
            [1, 2]);
        var assessment = analyzer.Analyze(unsafeAnimation);
        var state = new FlashSafetyAcknowledgementState
        {
            Acknowledgements =
            [
                new FlashSafetyAcknowledgement
                {
                    ContentId = assessment.ContentId,
                    RevisionHash = assessment.RevisionHash,
                    AcknowledgedAt = DateTimeOffset.UtcNow,
                    Warning = FlashSafetyAcknowledgementService.RequiredWarning
                }
            ]
        };

        var acknowledged = analyzer.Decide(assessment, state);
        var changed = builder.WithBpm(unsafeAnimation with { Bpm = 120 }, 180);
        var changedDecision = analyzer.Decide(analyzer.Analyze(changed), state);

        Assert.Equal(FlashSafetyStatus.AcknowledgedOverride, acknowledged.Status);
        Assert.True(acknowledged.CanPlay);
        Assert.Equal(FlashSafetyStatus.Blocked, changedDecision.Status);
        Assert.False(changedDecision.CanPlay);
    }

    [Fact]
    public async Task AcknowledgementStore_IsReversible_AndCorruptDataIsNotOverwritten()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maskapp-flash-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "flash.json");
        try
        {
            var store = new JsonFlashSafetyAcknowledgementStoreCore(path);
            var service = new FlashSafetyAcknowledgementService(store);
            var assessment = new FlashSafetyAnalyzer().Analyze(CreateBlackWhiteAnimation(
                TimeSpan.FromMilliseconds(100),
                AnimationLoopMode.Continuous,
                1,
                [1, 2]));

            await service.AcknowledgeAsync(assessment);
            Assert.Single((await store.LoadAsync()).Acknowledgements);
            await service.RevokeAsync(assessment.ContentId);
            Assert.Empty((await store.LoadAsync()).Acknowledgements);

            await File.WriteAllTextAsync(path, "{ corrupt");
            var corruptState = await store.LoadAsync();
            Assert.True(corruptState.UsedFallback);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                store.SaveAsync(corruptState with { Acknowledgements = [] }));
            Assert.Equal("{ corrupt", await File.ReadAllTextAsync(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    internal static PerformanceAnimation CreateBlackWhiteAnimation(
        TimeSpan frameDuration,
        AnimationLoopMode loopMode,
        int finiteLoops,
        IReadOnlyList<int> playbackSlots)
    {
        var black = CreateSolidPattern("black", FaceColor.Black, 1, lit: false);
        var white = CreateSolidPattern("white", new FaceColor(255, 255, 255), 2, lit: true);
        var animation = new PerformanceAnimation
        {
            Id = "black-white",
            DisplayName = "Black White",
            StoredFrames =
            [
                new PerformanceAnimationStoredFrame { Slot = 1, Pattern = black },
                new PerformanceAnimationStoredFrame { Slot = 2, Pattern = white }
            ],
            Frames = playbackSlots.Select(slot => new PerformanceAnimationFrame
            {
                Slot = slot,
                Duration = frameDuration
            }).ToArray(),
            LoopMode = loopMode,
            FiniteLoopCount = finiteLoops,
            Bpm = 120
        };
        return new PerformanceAnimationBuilder().WithRevision(animation);
    }

    internal static FacePattern CreateSolidPattern(
        string id,
        FaceColor color,
        int slot,
        bool lit) =>
        new FacePattern
        {
            Id = id,
            DisplayName = id,
            PreferredSlot = slot,
            Pixels = Enumerable.Repeat(new FacePixel(lit, color), FacePattern.PixelCount).ToArray()
        }.Normalize();
}
