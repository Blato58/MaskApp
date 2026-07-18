using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.Gallery;

public sealed class CustomAnimationCatalogTests
{
    [Fact]
    public void Build_AddsSavedProjectAsSendableEditableProductionAnimation()
    {
        var project = AnimationProject.CreateBlank("My animation") with
        {
            Source = AnimationProjectSource.GifImport,
            Frames =
            [
                AnimationProject.CreateBlank().Frames[0],
                new AnimationProjectFrame
                {
                    Id = "white",
                    Pattern = new FacePattern
                    {
                        Id = "white",
                        DisplayName = "White",
                        Pixels = Enumerable.Repeat(
                            new FacePixel(true, new FaceColor(255, 255, 255)),
                            FacePattern.PixelCount).ToArray()
                    },
                    Duration = TimeSpan.FromMilliseconds(125)
                }
            ]
        };

        var catalog = new GalleryCatalogBuilder(new QuickActionCatalog()).Build(
            new TextPresetStoreState(),
            new BuiltInAssetArchive(),
            new FacePatternStoreState(),
            new GalleryOrderState(),
            new AnimationProjectStoreState { Projects = [project] });

        var item = Assert.Single(catalog, item => item.Type == GalleryItemType.CustomAnimation);
        Assert.True(item.CanSend);
        Assert.True(item.CanManage);
        Assert.Equal("animation-studio", item.ManageTarget);
        Assert.Equal("My animation", item.Title);
        Assert.NotNull(item.PerformanceAnimation);
        Assert.Equal(2, item.PerformanceAnimation.StoredFrames.Count);
        Assert.Equal(FacePattern.Width, item.FacePattern!.Normalize().Pixels.Length / FacePattern.Height);
    }

    [Fact]
    public void Build_KeepsOverBudgetProjectVisibleWithActionableNonSendableState()
    {
        var frames = Enumerable.Range(0, 21)
            .Select(index =>
            {
                var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
                pixels[index] = new FacePixel(true, new FaceColor((byte)(index + 1), 255, 0));
                return new AnimationProjectFrame
                {
                    Id = $"f-{index}",
                    Pattern = new FacePattern
                    {
                        Id = $"p-{index}",
                        DisplayName = $"P {index}",
                        Pixels = pixels
                    }
                };
            })
            .ToArray();
        var project = new AnimationProject
        {
            Id = "too-many",
            DisplayName = "Too many",
            Frames = frames
        };

        var catalog = new GalleryCatalogBuilder(new QuickActionCatalog()).Build(
            new TextPresetStoreState(),
            new BuiltInAssetArchive(),
            new FacePatternStoreState(),
            new GalleryOrderState(),
            new AnimationProjectStoreState { Projects = [project] });

        var item = Assert.Single(catalog, item => item.Id == "animation:too-many");
        Assert.False(item.CanSend);
        Assert.True(item.CanManage);
        Assert.Null(item.PerformanceAnimation);
        Assert.Contains("20-slot budget", item.LastSendStatus, StringComparison.OrdinalIgnoreCase);
    }
}
