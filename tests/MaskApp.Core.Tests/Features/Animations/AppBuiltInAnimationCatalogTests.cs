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
        Assert.All(animations, animation =>
        {
            Assert.Equal("Holy Priest", animation.ArtistName);
            Assert.Equal(3, animation.Frames.Count);
            Assert.InRange(animation.PlaybackSlots.Count, 2, AppBuiltInAnimation.MaxPlaybackSlots);
            Assert.All(animation.PlaybackSlots, slot => Assert.Contains(slot, animation.ReservedSlots));
            Assert.All(animation.Frames, frame =>
            {
                Assert.InRange(frame.Slot, 15, 20);
                Assert.Equal(frame.Slot, frame.Pattern.PreferredSlot);
                Assert.Equal(FacePattern.PixelCount, frame.Pattern.Pixels.Length);
            });
        });

        var slots = animations.SelectMany(animation => animation.ReservedSlots).ToArray();
        Assert.Equal(slots.Length, slots.Distinct().Count());
        Assert.Equal(6, slots.Length);
        Assert.Equal(AppBuiltInAnimationCatalog.ReservedSlots, slots.Order());
        Assert.Equal(
            animations.Sum(animation => animation.Frames.Count),
            animations
                .SelectMany(animation => animation.Frames)
                .Select(frame => FaceContentFingerprint.Compute(frame.Pattern))
                .Distinct(StringComparer.Ordinal)
                .Count());
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

        Assert.Equal(14, slot);
        Assert.DoesNotContain(slot, AppBuiltInAnimationCatalog.ReservedSlots);
    }

    [Fact]
    public void FaceCollection_IncludesThreeHolyPriestMaskVariants()
    {
        var faces = FacePatternFactory.CreateBuiltIns()
            .Where(face => face.Id.StartsWith("built-in-face-holy-priest-", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(3, faces.Length);
        Assert.Contains(faces, face => face.DisplayName == "Holy Priest · Cross");
        Assert.Contains(faces, face => face.DisplayName == "Holy Priest · Blood Cross");
        Assert.Contains(faces, face => face.DisplayName == "Holy Priest · Negative");

        var cross = faces.Single(face => face.DisplayName == "Holy Priest · Cross");
        Assert.True(cross.GetPixel(23, 25).IsLit);
        Assert.Equal(FaceColor.Black, cross.GetPixel(23, 25).Color);
        Assert.NotEqual(FaceColor.Black, cross.GetPixel(15, 15).Color);
    }
}
