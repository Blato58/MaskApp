using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class AppBuiltInAnimationCatalogTests
{
    [Fact]
    public void CreateBuiltIns_ReturnsHolyPriestCollectionWithSharedFrameBankAndPerAnimationTiming()
    {
        var animations = AppBuiltInAnimationCatalog.CreateBuiltIns();

        Assert.Equal(6, animations.Count);
        Assert.Equal(
            [
                "Holy Priest · Black / White Flash",
                "Holy Priest · Red Mass",
                "Holy Priest · Antihero Scan",
                "Holy Priest · Atlantis Signal",
                "Holy Priest · No Balance",
                "Holy Priest · Ritual Inversion"
            ],
            animations.Select(animation => animation.DisplayName));
        Assert.Equal([150, 180, 200, 240, 210, 170], animations.Select(animation => animation.FrameDurationMilliseconds));
        Assert.Equal([15, 16, 17, 18, 19, 20], AppBuiltInAnimationCatalog.ReservedSlots);

        Assert.All(animations, animation =>
        {
            Assert.Equal("Holy Priest", animation.ArtistName);
            Assert.NotNull(animation.FrameDuration);
            Assert.InRange(animation.PlaybackSlots.Count, 2, AppBuiltInAnimation.MaxPlaybackSlots);
            Assert.All(animation.PlaybackSlots, slot => Assert.Contains(slot, animation.ReservedSlots));
            Assert.All(animation.Frames, frame =>
            {
                Assert.Contains(frame.Slot, AppBuiltInAnimationCatalog.ReservedSlots);
                Assert.Equal(frame.Slot, frame.Pattern.PreferredSlot);
                Assert.Equal(FacePattern.PixelCount, frame.Pattern.Pixels.Length);
            });
        });

        var sharedFrames = animations
            .SelectMany(animation => animation.Frames)
            .GroupBy(frame => frame.Slot)
            .OrderBy(group => group.Key)
            .ToArray();
        Assert.Equal(AppBuiltInAnimationCatalog.ReservedSlots, sharedFrames.Select(group => group.Key));
        Assert.All(sharedFrames, group =>
            Assert.Single(
                group.Select(frame => FaceContentFingerprint.Compute(frame.Pattern))
                    .Distinct(StringComparer.Ordinal)));
        Assert.Equal(
            sharedFrames.Length,
            sharedFrames
                .Select(group => FaceContentFingerprint.Compute(group.First().Pattern))
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void NextCustomFaceSlot_DoesNotConsumeAnimationReservations()
    {
        var state = new FacePatternStoreState
        {
            Patterns = Enumerable.Range(7, 6)
                .Select(slot => FacePatternFactory.CreateBlank($"Face {slot}", slot))
                .ToArray()
        };

        var slot = state.NextCustomSlot(AppBuiltInAnimationCatalog.ReservedSlots);

        Assert.Equal(13, slot);
        Assert.DoesNotContain(slot, AppBuiltInAnimationCatalog.ReservedSlots);
    }

    [Fact]
    public void FaceCollection_IncludesSixDistinctHolyPriestLooks()
    {
        var faces = FacePatternFactory.CreateBuiltIns()
            .Where(face => face.Id.StartsWith("built-in-face-holy-priest-", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(6, faces.Length);
        Assert.Equal(
            [
                "Holy Priest · Cross",
                "Holy Priest · Masked Antihero",
                "Holy Priest · Bass Pistons",
                "Holy Priest · Atlantis Sonar",
                "Holy Priest · No Balance",
                "Holy Priest · 90s → Future"
            ],
            faces.Select(face => face.DisplayName));
        Assert.Equal(
            faces.Length,
            faces.Select(FaceContentFingerprint.Compute).Distinct(StringComparer.Ordinal).Count());

        var cross = faces[0];
        Assert.Equal(
            ["#000000", "#FFFFFF"],
            cross.Pixels
                .Where(pixel => pixel.IsLit)
                .Select(pixel => pixel.Color.Hex)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal));
        Assert.All(faces.Skip(1), face =>
            Assert.True(
                face.Pixels.Where(pixel => pixel.IsLit).Select(pixel => pixel.Color).Distinct().Count() >= 4,
                $"{face.DisplayName} should use at least four color layers."));
    }

    [Fact]
    public void BlackWhiteFlash_PreservesCalibratedMaskGeometryAtSlowerCadence()
    {
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];
        var normal = animation.Frames[0].Pattern;
        var inverted = animation.Frames[1].Pattern;

        Assert.Equal(150, animation.FrameDurationMilliseconds);
        Assert.Equal("#000000", normal.GetPixel(23, 10).Color.Hex);
        Assert.Equal("#FFFFFF", inverted.GetPixel(23, 10).Color.Hex);

        (FacePattern Pattern, string Shell, string Cross)[] expectedFrames =
        [
            (normal, "#FFFFFF", "#000000"),
            (inverted, "#000000", "#FFFFFF")
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
            }
        }
    }
}
