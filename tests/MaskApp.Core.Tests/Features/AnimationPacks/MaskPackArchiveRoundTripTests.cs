using MaskApp.Core.Features.AnimationPacks;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.AnimationPacks;

public sealed class MaskPackArchiveRoundTripTests
{
    [Fact]
    public async Task Import_MigratesLegacyHolyPriestPageAndSceneReferences()
    {
        var pageEntry = new MaskPackContentEntry
        {
            Id = "legacy-page",
            Type = MaskPackContentType.Page,
            Name = "Legacy Page",
            Path = "pages/legacy-page.json",
            Sha256 = new string('1', 64)
        };
        var sceneEntry = new MaskPackContentEntry
        {
            Id = "legacy-scene",
            Type = MaskPackContentType.Scene,
            Name = "Legacy Scene",
            Path = "scenes/legacy-scene.json",
            Sha256 = new string('2', 64)
        };
        var package = new MaskPackDecodedPackage
        {
            Manifest = new MaskPackManifest
            {
                SchemaVersion = MaskPackManifest.CurrentSchemaVersion,
                PackName = "Legacy Holy Priest",
                Author = "Test",
                Contents = [pageEntry, sceneEntry]
            },
            Pages =
            [
                new MaskPackDecodedItem<GalleryPageLayout>(
                    pageEntry,
                    new GalleryPageLayout
                    {
                        PageId = pageEntry.Id,
                        Title = pageEntry.Name,
                        Items =
                        [
                            PageItem("face", "face:built-in-face-holy-priest-retro-future", "Legacy face", 0),
                            PageItem("animation", "app-animation:holy-priest-red-mass", "Legacy animation", 1)
                        ]
                    })
            ],
            Scenes =
            [
                new MaskPackDecodedItem<PerformanceScene>(
                    sceneEntry,
                    new PerformanceScene
                    {
                        Id = sceneEntry.Id,
                        DisplayName = sceneEntry.Name,
                        Steps =
                        [
                            new PerformanceSceneStep
                            {
                                Id = "face",
                                Kind = SceneStepKind.Face,
                                GalleryItemId = "face:built-in-face-holy-priest-retro-future"
                            },
                            new PerformanceSceneStep
                            {
                                Id = "animation",
                                Kind = SceneStepKind.Animation,
                                GalleryItemId = "app-animation:holy-priest-red-mass"
                            }
                        ]
                    })
            ]
        };
        var gallery = new InMemoryGalleryLayoutStore();
        var scenes = new InMemorySceneShowStore();
        var service = CreateService(
            new InMemoryTextPresetStore(),
            new InMemoryFacePatternStore(),
            new InMemoryAnimationProjectStore(),
            gallery,
            scenes);

        var result = await service.ImportAsync(new MaskPackImportRequest
        {
            Inspection = new MaskPackInspection { Package = package }
        });

        Assert.True(result.Succeeded, result.Message);
        var importedPage = Assert.Single((await gallery.LoadAsync()).Pages, item => item.PageId == pageEntry.Id);
        Assert.Contains(importedPage.Items, item => item.GalleryItemId == "face:built-in-face-holy-priest-original");
        Assert.Contains(importedPage.Items, item => item.GalleryItemId == "app-animation:holy-priest-blue-red-black");
        var importedScene = Assert.Single((await scenes.LoadAsync()).Scenes, item => item.Id == sceneEntry.Id);
        Assert.Equal("face:built-in-face-holy-priest-original", importedScene.Steps[0].GalleryItemId);
        Assert.Equal("app-animation:holy-priest-blue-red-black", importedScene.Steps[1].GalleryItemId);
    }

    [Fact]
    public async Task Export_MaximumRepeatedAnimation_RemainsInspectableUnderZipBombGuard()
    {
        var pattern = CreateFace("frame", "Frame", 0, new FaceColor(1, 2, 3));
        var animation = new AnimationProject
        {
            Id = "animation-max-repeated",
            DisplayName = "Maximum Repeated Animation",
            Frames = Enumerable.Range(0, AnimationProject.MaxSourceFrames)
                .Select(index => new AnimationProjectFrame
                {
                    Id = $"frame-{index:000}",
                    Duration = TimeSpan.FromMilliseconds(100),
                    Pattern = pattern
                })
                .ToArray()
        }.Normalize(DateTimeOffset.UnixEpoch);
        var source = CreateService(
            new InMemoryTextPresetStore(),
            new InMemoryFacePatternStore(),
            new InMemoryAnimationProjectStore(new AnimationProjectStoreState { Projects = [animation] }),
            new InMemoryGalleryLayoutStore(),
            new InMemorySceneShowStore());
        await using var archive = new MemoryStream();

        var exported = await source.ExportAsync(archive, new MaskPackExportRequest());
        archive.Position = 0;
        var inspection = await CreateService(
            new InMemoryTextPresetStore(),
            new InMemoryFacePatternStore(),
            new InMemoryAnimationProjectStore(),
            new InMemoryGalleryLayoutStore(),
            new InMemorySceneShowStore()).InspectAsync(archive);

        Assert.True(exported.Succeeded, exported.Message);
        Assert.True(inspection.IsValid, string.Join(" ", inspection.Errors));
        Assert.Equal(AnimationProject.MaxSourceFrames, Assert.Single(inspection.Package!.Animations).Value.Frames.Count);
    }

    [Fact]
    public async Task ExportInspectImport_RoundTripsCompleteOfflineShow()
    {
        var face = CreateFace("face-show", "Show Face", 0, new FaceColor(0x11, 0x99, 0xEE)) with
        {
            LastUploadedAt = DateTimeOffset.UtcNow,
            LastUploadStatus = "Prepared on another device"
        };
        var animation = new AnimationProject
        {
            Id = "animation-show",
            DisplayName = "Show Animation",
            Source = AnimationProjectSource.Drawn,
            LoopMode = AnimationLoopMode.Finite,
            FiniteLoopCount = 4,
            Bpm = 132,
            Frames =
            [
                new AnimationProjectFrame { Id = "frame-a", Duration = TimeSpan.FromMilliseconds(90), Pattern = face },
                new AnimationProjectFrame { Id = "frame-b", Duration = TimeSpan.FromMilliseconds(175), Pattern = CreateFace("frame-b", "Frame B", 20, new FaceColor(0xFF, 0x22, 0x88)) }
            ]
        }.Normalize(DateTimeOffset.UnixEpoch);
        var text = new TextPreset
        {
            Id = new TextPresetId("text-show"),
            DisplayName = "Show Caption",
            InputText = "HELLO PRAGUE",
            MaskText = "HELLO PRAGUE",
            PackName = "Show",
            IsFavorite = true,
            Style = TextPresetStyle.Default with { IsBold = true, Speed = 61 },
            LastSentAt = DateTimeOffset.UtcNow,
            LastSendStatus = "Sent on another mask"
        };
        var page = new GalleryPageLayout
        {
            PageId = "page-show",
            Title = "Show Page",
            ColorHex = "#22C55E",
            Items =
            [
                PageItem("slot-face", "face:face-show", "Face", 0) with
                {
                    FastMaskSlot = 12,
                    FastContentFingerprint = "device-bound",
                    FastPreparedAt = DateTimeOffset.UtcNow
                },
                PageItem("slot-animation", "animation:animation-show", "Animation", 1),
                PageItem("slot-text", "text:text-show", "Caption", 2)
            ]
        };
        var scene = new PerformanceScene
        {
            Id = "scene-show",
            DisplayName = "Opening Scene",
            Steps =
            [
                new PerformanceSceneStep { Id = "step-face", Kind = SceneStepKind.Face, GalleryItemId = "face:face-show" },
                new PerformanceSceneStep { Id = "step-wait", Kind = SceneStepKind.Wait, Duration = TimeSpan.FromMilliseconds(250) },
                new PerformanceSceneStep { Id = "step-animation", Kind = SceneStepKind.Animation, GalleryItemId = "animation:animation-show" },
                new PerformanceSceneStep { Id = "step-text", Kind = SceneStepKind.Text, GalleryItemId = "text:text-show" }
            ]
        }.Normalize(DateTimeOffset.UnixEpoch);
        var setlist = new PerformanceSetlist
        {
            Id = "setlist-show",
            DisplayName = "Main Set",
            Cues = [new PerformanceSetlistCue { Id = "cue-open", Label = "Open", SceneId = scene.Id }]
        }.Normalize(DateTimeOffset.UnixEpoch);
        var source = CreateService(
            new InMemoryTextPresetStore(new TextPresetStoreState { Presets = [text] }),
            new InMemoryFacePatternStore(new FacePatternStoreState { Patterns = [face] }),
            new InMemoryAnimationProjectStore(new AnimationProjectStoreState { Projects = [animation] }),
            new InMemoryGalleryLayoutStore(new GalleryLayoutState
            {
                Pages = [page],
                Order = new GalleryOrderState
                {
                    ItemOrders =
                    [
                        new GalleryItemOrder { ItemId = "face:face-show", SortIndex = 1 },
                        new GalleryItemOrder { ItemId = "animation:animation-show", SortIndex = 2 },
                        new GalleryItemOrder { ItemId = "text:text-show", SortIndex = 3 }
                    ]
                }
            }),
            new InMemorySceneShowStore(new SceneShowState
            {
                Scenes = [scene],
                Setlists = [setlist],
                ActiveSetlistId = setlist.Id
            }));
        await using var archive = new MemoryStream();

        var exported = await source.ExportAsync(
            archive,
            new MaskPackExportRequest { PackName = "Complete Show", Author = "Test Artist" });

        Assert.True(exported.Succeeded, exported.Message);
        Assert.Equal(7, exported.ContentCount);
        archive.Position = 0;
        var targetText = new InMemoryTextPresetStore();
        var targetFaces = new InMemoryFacePatternStore();
        var targetAnimations = new InMemoryAnimationProjectStore();
        var targetGallery = new InMemoryGalleryLayoutStore();
        var targetScenes = new InMemorySceneShowStore();
        var target = CreateService(targetText, targetFaces, targetAnimations, targetGallery, targetScenes);

        var inspection = await target.InspectAsync(archive);

        Assert.True(inspection.IsValid, string.Join(" ", inspection.Errors));
        Assert.False(inspection.MigratedFromV1);
        Assert.Equal(46, inspection.Package?.Manifest.ArtDisplay.Width);
        Assert.Equal(44, inspection.Package?.Manifest.TextDisplay.Width);
        Assert.Equal(7, inspection.Package?.Entries.Count);
        Assert.Contains(inspection.Conflicts, conflict => conflict.Type == MaskPackContentType.Appearance);

        var imported = await target.ImportAsync(new MaskPackImportRequest { Inspection = inspection });

        Assert.True(imported.Succeeded, imported.Message);
        var importedFace = Assert.Single((await targetFaces.LoadAsync()).Patterns, item => item.Id == face.Id);
        Assert.Null(importedFace.LastUploadedAt);
        Assert.Empty(importedFace.LastUploadStatus);
        Assert.Equal(face.GetPixel(0, 0), importedFace.GetPixel(0, 0));
        var importedAnimation = Assert.Single((await targetAnimations.LoadAsync()).Projects, item => item.Id == animation.Id);
        Assert.Equal([90d, 175d], importedAnimation.Frames.Select(frame => frame.Duration.TotalMilliseconds));
        Assert.Equal(AnimationProjectSource.MaskPackImport, importedAnimation.Source);
        var importedText = Assert.Single((await targetText.LoadAsync()).Presets, item => item.Id == text.Id);
        Assert.True(importedText.Style.IsBold);
        Assert.Null(importedText.LastSentAt);
        Assert.Empty(importedText.LastSendStatus);
        var importedPage = Assert.Single((await targetGallery.LoadAsync()).Pages, item => item.PageId == page.PageId);
        Assert.Equal(3, importedPage.Items.Count);
        Assert.All(importedPage.Items, item =>
        {
            Assert.Null(item.FastMaskSlot);
            Assert.Empty(item.FastContentFingerprint);
            Assert.Null(item.FastPreparedAt);
        });
        var importedShow = await targetScenes.LoadAsync();
        Assert.Contains(importedShow.Scenes, item => item.Id == scene.Id);
        Assert.Contains(importedShow.Setlists, item => item.Id == setlist.Id && item.Cues[0].SceneId == scene.Id);
        Assert.Empty(importedShow.ActiveSetlistId);
        Assert.Contains((await targetGallery.LoadAsync()).Order.ItemOrders, order => order.ItemId == "face:face-show");
    }

    internal static MaskPackArchiveService CreateService(
        ITextPresetStore text,
        IFacePatternStore faces,
        IAnimationProjectStore animations,
        IGalleryLayoutStore gallery,
        ISceneShowStore scenes,
        IMaskPackImportJournalStore? journal = null) =>
        new(text, faces, animations, gallery, scenes, journal ?? new InMemoryMaskPackImportJournalStore());

    internal static FacePattern CreateFace(string id, string name, int litIndex, FaceColor color)
    {
        var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        pixels[litIndex] = new FacePixel(true, color);
        return new FacePattern
        {
            Id = id,
            DisplayName = name,
            Source = FacePatternSource.Custom,
            PreferredSlot = 7,
            Pixels = pixels
        }.Normalize(DateTimeOffset.UnixEpoch);
    }

    internal static GalleryPageItemLayout PageItem(string slotId, string itemId, string label, int sortIndex) => new()
    {
        SlotId = slotId,
        GalleryItemId = itemId,
        Label = label,
        SortIndex = sortIndex
    };
}
