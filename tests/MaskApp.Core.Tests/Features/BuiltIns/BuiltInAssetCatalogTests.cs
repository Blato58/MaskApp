using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.Core.Tests.Features.BuiltIns;

public sealed class BuiltInAssetCatalogTests
{
    [Fact]
    public void StaticImages_UsesAndroidCatalogIds()
    {
        Assert.Equal(70, BuiltInAssetCatalog.StaticImages.Count);
        Assert.Equal(Enumerable.Range(0, 70), BuiltInAssetCatalog.StaticImages.Select(definition => definition.Id));
    }

    [Fact]
    public void Animations_UsesAndroidCatalogIdsAndSkipsFour()
    {
        Assert.Equal(45, BuiltInAssetCatalog.Animations.Count);
        Assert.Equal([0, 1, 2, 3], BuiltInAssetCatalog.Animations.Take(4).Select(definition => definition.Id));
        Assert.Equal(5, BuiltInAssetCatalog.Animations[4].Id);
        Assert.Equal(45, BuiltInAssetCatalog.Animations[^1].Id);
        Assert.False(BuiltInAssetCatalog.IsKnown(BuiltInAssetType.Animation, 4));
    }

    [Fact]
    public void KnownIdStepping_SkipsAnimationFour()
    {
        Assert.Equal(5, BuiltInAssetCatalog.GetNextKnownId(BuiltInAssetType.Animation, 3));
        Assert.Equal(3, BuiltInAssetCatalog.GetPreviousKnownId(BuiltInAssetType.Animation, 5));
        Assert.Equal(5, BuiltInAssetCatalog.GetNextKnownId(BuiltInAssetType.Animation, 4));
        Assert.Equal(3, BuiltInAssetCatalog.GetPreviousKnownId(BuiltInAssetType.Animation, 4));
    }

    [Fact]
    public void Previews_MapEveryKnownIdToExactResourceMetadata()
    {
        var face = BuiltInAssetCatalog.GetDefinitionOrFallback(BuiltInAssetType.StaticImage, 63).Preview;
        var animation = BuiltInAssetCatalog.GetDefinitionOrFallback(BuiltInAssetType.Animation, 45).Preview;

        Assert.All(BuiltInAssetCatalog.Definitions, definition => Assert.True(definition.Preview.IsAvailable));
        Assert.Equal("builtin_face_63.png", face.ResourceName);
        Assert.Equal(52, face.Width);
        Assert.Equal(64, face.Height);
        Assert.False(face.IsAnimated);
        Assert.Equal("builtin_anim_45.gif", animation.ResourceName);
        Assert.True(animation.IsAnimated);
        Assert.Equal(10, animation.FrameCount);
        Assert.Equal(100, animation.FrameDurationMilliseconds);
    }

    [Theory]
    [InlineData(3, 9)]
    [InlineData(8, 4)]
    [InlineData(21, 3)]
    [InlineData(25, 6)]
    [InlineData(40, 9)]
    [InlineData(43, 4)]
    public void AnimationPreviews_PreserveExceptionalFrameCounts(int id, int expectedFrameCount)
    {
        var preview = BuiltInAssetCatalog.GetDefinitionOrFallback(BuiltInAssetType.Animation, id).Preview;

        Assert.Equal(expectedFrameCount, preview.FrameCount);
    }
}
