using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.HolyPriest;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class AppBuiltInAnimationCatalogTests
{
    [Fact]
    public void CreateBuiltIns_ReturnsHolyPriestCollectionWithSharedFrameBankAndPerAnimationTiming()
    {
        var animations = AppBuiltInAnimationCatalog.CreateBuiltIns();

        Assert.Equal(4, animations.Count);
        Assert.Equal(
            [
                HolyPriestBuiltInCatalog.BlackWhiteAnimationId,
                HolyPriestBuiltInCatalog.BlueRedBlackAnimationId,
                HolyPriestBuiltInCatalog.FiveMaskAnimationId,
                HolyPriestBuiltInCatalog.ColorPulseAnimationId
            ],
            animations.Select(animation => animation.Id));
        Assert.Equal(
            [
                "Holy Priest · Black / White Flash",
                "Holy Priest · Blue → Red → Black",
                "Holy Priest · Five Mask Cycle",
                "Holy Priest · Color Pulse"
            ],
            animations.Select(animation => animation.DisplayName));
        Assert.Equal([150, 180, 220, 200], animations.Select(animation => animation.FrameDurationMilliseconds));
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
    public void FaceCollection_IncludesFiveOriginalMaskColorwaysWithDedicatedSlots()
    {
        var faces = FacePatternFactory.CreateBuiltIns()
            .Where(face => face.Id.StartsWith("built-in-face-holy-priest-", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(5, faces.Length);
        Assert.Equal(
            [
                "Holy Priest · Original",
                "Holy Priest · Inverted",
                "Holy Priest · Red",
                "Holy Priest · Blue",
                "Holy Priest · Gold"
            ],
            faces.Select(face => face.DisplayName));
        Assert.Equal([15, 16, 17, 18, 19], faces.Select(face => face.PreferredSlot));
        Assert.Equal(
            faces.Length,
            faces.Select(FaceContentFingerprint.Compute).Distinct(StringComparer.Ordinal).Count());

        string[][] expectedColors =
        [
            ["#000000", "#FFFFFF"],
            ["#000000", "#FFFFFF"],
            ["#000000", "#FF0000"],
            ["#000000", "#0000FF"],
            ["#000000", "#FFFF00"]
        ];
        for (var index = 0; index < faces.Length; index++)
        {
            var colors = faces[index].Pixels
                .Where(pixel => pixel.IsLit)
                .Select(pixel => pixel.Color.Hex)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(expectedColors[index], colors);
        }
    }

    [Fact]
    public void FrameBank_ReusesEveryStaticFaceSlot_AndKeepsBlackoutAnimationOnly()
    {
        var facesBySlot = FacePatternFactory.CreateBuiltIns()
            .Where(face => face.Id.StartsWith("built-in-face-holy-priest-", StringComparison.Ordinal))
            .ToDictionary(face => face.PreferredSlot);
        var framesBySlot = AppBuiltInAnimationCatalog.CreateBuiltIns()
            .SelectMany(animation => animation.Frames)
            .GroupBy(frame => frame.Slot)
            .ToDictionary(group => group.Key, group => group.First().Pattern);

        Assert.Equal([15, 16, 17, 18, 19], facesBySlot.Keys.Order().ToArray());
        foreach (var (slot, face) in facesBySlot)
        {
            Assert.Equal(
                FaceContentFingerprint.Compute(face),
                FaceContentFingerprint.Compute(framesBySlot[slot]));
        }

        Assert.False(facesBySlot.ContainsKey(20));
        Assert.All(framesBySlot[20].Pixels, pixel =>
        {
            Assert.True(pixel.IsLit);
            Assert.Equal("#000000", pixel.Color.Hex);
        });
    }

    [Fact]
    public void BlueRedBlack_PreservesRequestedSequenceAndOwnCadence()
    {
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[1];

        Assert.Equal(180, animation.FrameDurationMilliseconds);
        Assert.Equal([18, 17, 20, 18, 17, 20, 15, 20], animation.PlaybackSlots);
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
