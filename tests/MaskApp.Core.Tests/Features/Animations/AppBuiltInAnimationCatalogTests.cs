using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class AppBuiltInAnimationCatalogTests
{
    [Fact]
    public void CreateBuiltIns_ReturnsHolyPriestAnimationsWithDedicatedSlots()
    {
        var animations = AppBuiltInAnimationCatalog.CreateBuiltIns();

        Assert.Equal(2, animations.Count);
        Assert.Equal("Holy Priest · Black / White Flash", animations[0].DisplayName);
        Assert.Equal("Holy Priest · Black → Red → Blue", animations[1].DisplayName);
        Assert.Equal([15, 16], animations[0].ReservedSlots);
        Assert.Equal([17, 18, 19], animations[1].ReservedSlots);
        Assert.Equal([15, 16, 15, 16, 15, 16, 15, 16, 15, 16], animations[0].PlaybackSlots);
        Assert.Equal([17, 18, 19, 17, 18, 19, 17, 18, 19], animations[1].PlaybackSlots);
        Assert.All(animations, animation =>
        {
            Assert.Equal("Holy Priest", animation.ArtistName);
            Assert.InRange(animation.PlaybackSlots.Count, 2, AppBuiltInAnimation.MaxPlaybackSlots);
            Assert.All(animation.PlaybackSlots, slot => Assert.Contains(slot, animation.ReservedSlots));
            Assert.All(animation.Frames, frame =>
            {
                Assert.InRange(frame.Slot, 15, 19);
                Assert.Equal(frame.Slot, frame.Pattern.PreferredSlot);
                Assert.Equal(FacePattern.PixelCount, frame.Pattern.Pixels.Length);
            });
            Assert.Equal(
                animation.Frames.Count,
                animation.Frames
                    .Select(frame => FaceContentFingerprint.Compute(frame.Pattern))
                    .Distinct(StringComparer.Ordinal)
                    .Count());
        });

        var slots = animations.SelectMany(animation => animation.ReservedSlots).ToArray();
        Assert.Equal(slots.Length, slots.Distinct().Count());
        Assert.Equal(5, slots.Length);
        Assert.Equal(AppBuiltInAnimationCatalog.ReservedSlots, slots.Order());
        Assert.Equal(
            FaceContentFingerprint.Compute(animations[0].Frames[0].Pattern),
            FaceContentFingerprint.Compute(animations[1].Frames[0].Pattern));
    }

    [Fact]
    public void NextCustomFaceSlot_DoesNotConsumeAnimationReservations()
    {
        var state = new FacePatternStoreState
        {
            Patterns = Enumerable.Range(7, 8)
                .Select(slot => FacePatternFactory.CreateBlank($"Face {slot}", slot))
                .ToArray()
        };

        var slot = state.NextCustomSlot(AppBuiltInAnimationCatalog.ReservedSlots);

        Assert.Equal(FacePattern.MaxSlot, slot);
        Assert.DoesNotContain(slot, AppBuiltInAnimationCatalog.ReservedSlots);
    }

    [Fact]
    public void FaceCollection_IncludesSingleBlackAndWhiteHolyPriestMask()
    {
        var faces = FacePatternFactory.CreateBuiltIns()
            .Where(face => face.Id.StartsWith("built-in-face-holy-priest-", StringComparison.Ordinal))
            .ToArray();

        var cross = Assert.Single(faces);
        Assert.Equal("Holy Priest · Cross", cross.DisplayName);
        Assert.Equal(
            ["#000000", "#FFFFFF"],
            cross.Pixels
                .Where(pixel => pixel.IsLit)
                .Select(pixel => pixel.Color.Hex)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal));
        Assert.All(cross.Pixels, pixel => Assert.True(pixel.IsLit));
        Assert.Equal("#FFFFFF", cross.GetPixel(0, 0).Color.Hex);
        Assert.Equal("#FFFFFF", cross.GetPixel(45, 57).Color.Hex);
        Assert.Equal("#FFFFFF", cross.GetPixel(23, 0).Color.Hex);
        Assert.Equal("#FFFFFF", cross.GetPixel(23, 4).Color.Hex);
        Assert.Equal("#000000", cross.GetPixel(23, 5).Color.Hex);
        Assert.Equal("#000000", cross.GetPixel(23, 10).Color.Hex);
        Assert.Equal("#000000", cross.GetPixel(23, 57).Color.Hex);
        Assert.Equal("#FFFFFF", cross.GetPixel(4, 15).Color.Hex);
        Assert.Equal("#000000", cross.GetPixel(5, 15).Color.Hex);
        Assert.Equal("#000000", cross.GetPixel(40, 15).Color.Hex);
        Assert.Equal("#FFFFFF", cross.GetPixel(41, 15).Color.Hex);
        Assert.Equal("#FFFFFF", cross.GetPixel(10, 14).Color.Hex);
        Assert.Equal("#FFFFFF", cross.GetPixel(10, 20).Color.Hex);
    }

    [Fact]
    public void HolyPriestAnimations_UseRequestedFramePalettesAndCalibratedGeometry()
    {
        var animations = AppBuiltInAnimationCatalog.CreateBuiltIns();
        var monochrome = animations[0];
        var colorCycle = animations[1];

        Assert.All(monochrome.Frames, frame =>
            Assert.Subset(
                new HashSet<string>(["#000000", "#FFFFFF"], StringComparer.Ordinal),
                frame.Pattern.Pixels
                    .Where(pixel => pixel.IsLit)
                    .Select(pixel => pixel.Color.Hex)
                    .ToHashSet(StringComparer.Ordinal)));
        Assert.Equal("#000000", monochrome.Frames[0].Pattern.GetPixel(23, 10).Color.Hex);
        Assert.Equal("#FFFFFF", monochrome.Frames[1].Pattern.GetPixel(23, 10).Color.Hex);

        Assert.Equal(
            ["#000000", "#FF0000", "#0000FF"],
            colorCycle.Frames.Select(frame => frame.Pattern.GetPixel(23, 10).Color.Hex));

        (FacePattern Pattern, string Shell, string Cross)[] expectedFrames =
        [
            (monochrome.Frames[0].Pattern, "#FFFFFF", "#000000"),
            (monochrome.Frames[1].Pattern, "#000000", "#FFFFFF"),
            (colorCycle.Frames[0].Pattern, "#FFFFFF", "#000000"),
            (colorCycle.Frames[1].Pattern, "#FFFFFF", "#FF0000"),
            (colorCycle.Frames[2].Pattern, "#FFFFFF", "#0000FF")
        ];
        (int Row, int LeftStart, int LeftEnd, int RightStart, int RightEnd)[] eyeRows =
        [
            (16, 5, 15, 30, 40),
            (17, 6, 17, 28, 38),
            (18, 7, 17, 28, 38),
            (19, 9, 14, 31, 36)
        ];

        foreach (var (pattern, shellColor, crossColor) in expectedFrames)
        {
            Assert.All(pattern.Pixels, pixel => Assert.True(pixel.IsLit));
            Assert.Equal(shellColor, pattern.GetPixel(23, 4).Color.Hex);
            Assert.Equal(crossColor, pattern.GetPixel(23, 5).Color.Hex);
            Assert.Equal(crossColor, pattern.GetPixel(23, 57).Color.Hex);

            Assert.Equal(shellColor, pattern.GetPixel(4, 15).Color.Hex);
            Assert.Equal(crossColor, pattern.GetPixel(5, 15).Color.Hex);
            Assert.Equal(crossColor, pattern.GetPixel(40, 15).Color.Hex);
            Assert.Equal(shellColor, pattern.GetPixel(41, 15).Color.Hex);

            foreach (var (row, leftStart, leftEnd, rightStart, rightEnd) in eyeRows)
            {
                for (var column = leftStart; column <= leftEnd; column++)
                {
                    Assert.Equal("#000000", pattern.GetPixel(column, row).Color.Hex);
                }

                for (var column = rightStart; column <= rightEnd; column++)
                {
                    Assert.Equal("#000000", pattern.GetPixel(column, row).Color.Hex);
                }

                Assert.Equal(crossColor, pattern.GetPixel(leftEnd + 1, row).Color.Hex);
                Assert.Equal(crossColor, pattern.GetPixel(rightStart - 1, row).Color.Hex);
                Assert.Equal(
                    leftStart == 5 ? shellColor : crossColor,
                    pattern.GetPixel(leftStart - 1, row).Color.Hex);
                Assert.Equal(
                    rightEnd == 40 ? shellColor : crossColor,
                    pattern.GetPixel(rightEnd + 1, row).Color.Hex);
            }
        }
    }
}
