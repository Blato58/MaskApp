using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class AnimationProjectCompilerTests
{
    [Fact]
    public void Compile_DeduplicatesPixels_AndPreservesTimelineTiming()
    {
        var black = FacePatternFactory.CreateBlank("Black", 1);
        var white = CreateSolidPattern("White");
        var project = CreateProject(
            new AnimationProjectFrame { Id = "a", Pattern = black, Duration = TimeSpan.FromMilliseconds(40) },
            new AnimationProjectFrame { Id = "b", Pattern = white, Duration = TimeSpan.FromMilliseconds(80) },
            new AnimationProjectFrame { Id = "c", Pattern = black, Duration = TimeSpan.FromMilliseconds(120) });

        var result = new AnimationProjectCompiler().Compile(project);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SourceFrameCount);
        Assert.Equal(2, result.UniqueFrameCount);
        Assert.Equal(1, result.DuplicateFrameCount);
        Assert.NotNull(result.Animation);
        Assert.Equal([1, 2, 1], result.Animation.Frames.Select(frame => frame.Slot));
        Assert.Equal(
            [TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(80), TimeSpan.FromMilliseconds(120)],
            result.Animation.Frames.Select(frame => frame.Duration));
        Assert.Equal(64, result.Animation.RevisionHash.Length);
    }

    [Fact]
    public void Compile_RejectsUniqueFramesBeyondHardwareBudget_WithActionableFeedback()
    {
        var frames = Enumerable.Range(0, 21)
            .Select(index => new AnimationProjectFrame
            {
                Id = $"frame-{index}",
                Pattern = CreateSinglePixelPattern(index),
                Duration = TimeSpan.FromMilliseconds(75)
            })
            .ToArray();

        var result = new AnimationProjectCompiler().Compile(CreateProject(frames));

        Assert.False(result.Succeeded);
        Assert.Null(result.Animation);
        Assert.Equal(21, result.UniqueFrameCount);
        Assert.Contains("20-slot budget", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Compile_TimingAndLoopChangesInvalidateRevision()
    {
        var project = CreateProject(new AnimationProjectFrame
        {
            Id = "one",
            Pattern = CreateSolidPattern("White"),
            Duration = TimeSpan.FromMilliseconds(75)
        });
        var compiler = new AnimationProjectCompiler();

        var original = compiler.Compile(project).Animation!;
        var timingChanged = compiler.Compile(project with
        {
            Frames = [project.Frames[0] with { Duration = TimeSpan.FromMilliseconds(90) }]
        }).Animation!;
        var loopChanged = compiler.Compile(project with
        {
            LoopMode = AnimationLoopMode.Finite,
            FiniteLoopCount = 2
        }).Animation!;

        Assert.NotEqual(original.RevisionHash, timingChanged.RevisionHash);
        Assert.NotEqual(original.RevisionHash, loopChanged.RevisionHash);
    }

    internal static AnimationProject CreateProject(params AnimationProjectFrame[] frames) => new()
    {
        Id = "project",
        DisplayName = "Test project",
        Frames = frames
    };

    internal static FacePattern CreateSolidPattern(string name) => new FacePattern
    {
        Id = name.ToLowerInvariant(),
        DisplayName = name,
        Pixels = Enumerable.Repeat(
            new FacePixel(true, new FaceColor(255, 255, 255)),
            FacePattern.PixelCount).ToArray()
    }.Normalize();

    private static FacePattern CreateSinglePixelPattern(int index)
    {
        var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        pixels[index] = new FacePixel(true, new FaceColor((byte)(index + 1), 255, 255));
        return new FacePattern
        {
            Id = $"pattern-{index}",
            DisplayName = $"Pattern {index}",
            Pixels = pixels
        }.Normalize();
    }
}
