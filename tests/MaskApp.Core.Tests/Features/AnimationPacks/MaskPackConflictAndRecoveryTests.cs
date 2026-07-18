using System.IO.Compression;
using System.Security.Cryptography;
using MaskApp.Core.Features.AnimationPacks;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.AnimationPacks;

public sealed class MaskPackConflictAndRecoveryTests
{
    [Fact]
    public async Task Import_ExactMerge_KeepsExistingItemWithoutDuplicate()
    {
        var face = Face("face-conflict", 0, 0x11);
        await using var archive = V2Archive(Content(MaskPackContentType.Face, face.Id, face.DisplayName, MaskPackPayloadCodec.SerializeFace(face)));
        var faces = new InMemoryFacePatternStore(new FacePatternStoreState { Patterns = [face] });
        var service = Service(faces: faces);
        var inspection = await service.InspectAsync(archive);

        var conflict = Assert.Single(inspection.Conflicts);
        Assert.True(conflict.IsExactMatch);

        var result = await service.ImportAsync(new MaskPackImportRequest { Inspection = inspection });

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(1, result.SkippedCount);
        Assert.Single((await faces.LoadAsync()).Patterns, item => item.Id == face.Id);
    }

    [Fact]
    public async Task Import_DifferentMerge_SafelyRenamesAndPreservesBothItems()
    {
        var local = Face("face-conflict", 0, 0x11);
        var incoming = Face("face-conflict", 1, 0xEE) with { DisplayName = "Incoming" };
        await using var archive = V2Archive(Content(MaskPackContentType.Face, incoming.Id, incoming.DisplayName, MaskPackPayloadCodec.SerializeFace(incoming)));
        var faces = new InMemoryFacePatternStore(new FacePatternStoreState { Patterns = [local] });
        var service = Service(faces: faces);
        var inspection = await service.InspectAsync(archive);

        var conflict = Assert.Single(inspection.Conflicts);
        Assert.False(conflict.IsExactMatch);

        var result = await service.ImportAsync(new MaskPackImportRequest { Inspection = inspection });

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(1, result.RenamedCount);
        var state = await faces.LoadAsync();
        Assert.Contains(state.Patterns, item => item.Id == local.Id && item.GetPixel(0, 0).IsLit);
        Assert.Contains(state.Patterns, item => item.Id == conflict.SuggestedId && item.GetPixel(1, 0).IsLit);
    }

    [Fact]
    public async Task Import_Skip_LeavesDifferentLocalItemUnchanged()
    {
        var local = Face("face-conflict", 0, 0x11);
        var incoming = Face("face-conflict", 1, 0xEE);
        await using var archive = V2Archive(Content(MaskPackContentType.Face, incoming.Id, incoming.DisplayName, MaskPackPayloadCodec.SerializeFace(incoming)));
        var faces = new InMemoryFacePatternStore(new FacePatternStoreState { Patterns = [local] });
        var service = Service(faces: faces);
        var inspection = await service.InspectAsync(archive);
        var conflict = Assert.Single(inspection.Conflicts);

        var result = await service.ImportAsync(new MaskPackImportRequest
        {
            Inspection = inspection,
            ConflictResolutions = new Dictionary<string, MaskPackConflictResolution>
            {
                [conflict.Key] = MaskPackConflictResolution.Skip
            }
        });

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(1, result.SkippedCount);
        var stored = Assert.Single((await faces.LoadAsync()).Patterns, item => item.Id == local.Id);
        Assert.True(stored.GetPixel(0, 0).IsLit);
        Assert.False(stored.GetPixel(1, 0).IsLit);
    }

    [Fact]
    public async Task Import_Replace_RequiresConfirmationThenReplacesSelectedId()
    {
        var local = Face("face-conflict", 0, 0x11);
        var incoming = Face("face-conflict", 1, 0xEE);
        await using var archive = V2Archive(Content(MaskPackContentType.Face, incoming.Id, incoming.DisplayName, MaskPackPayloadCodec.SerializeFace(incoming)));
        var faces = new InMemoryFacePatternStore(new FacePatternStoreState { Patterns = [local] });
        var service = Service(faces: faces);
        var inspection = await service.InspectAsync(archive);
        var conflict = Assert.Single(inspection.Conflicts);
        var resolutions = new Dictionary<string, MaskPackConflictResolution>
        {
            [conflict.Key] = MaskPackConflictResolution.Replace
        };

        var rejected = await service.ImportAsync(new MaskPackImportRequest
        {
            Inspection = inspection,
            ConflictResolutions = resolutions
        });
        var accepted = await service.ImportAsync(new MaskPackImportRequest
        {
            Inspection = inspection,
            ConflictResolutions = resolutions,
            ConfirmReplace = true
        });

        Assert.False(rejected.Succeeded);
        Assert.Contains("requires explicit confirmation", rejected.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(accepted.Succeeded, accepted.Message);
        Assert.Equal(1, accepted.ReplacedCount);
        var stored = Assert.Single((await faces.LoadAsync()).Patterns, item => item.Id == local.Id);
        Assert.False(stored.GetPixel(0, 0).IsLit);
        Assert.True(stored.GetPixel(1, 0).IsLit);
    }

    [Fact]
    public async Task Import_PageMerge_AddsNewShortcutsWithoutDiscardingExistingOnes()
    {
        var existing = new GalleryPageLayout
        {
            PageId = "page-live",
            Title = "Live",
            Items = [MaskPackArchiveRoundTripTests.PageItem("existing", "built-in:StaticImage:0", "Existing", 0)]
        };
        var incoming = existing with
        {
            Items = [MaskPackArchiveRoundTripTests.PageItem("incoming", "built-in:StaticImage:1", "Incoming", 0)]
        };
        await using var archive = V2Archive(Content(MaskPackContentType.Page, incoming.PageId, incoming.Title, MaskPackPayloadCodec.SerializePage(incoming)));
        var gallery = new InMemoryGalleryLayoutStore(new GalleryLayoutState { Pages = [existing] });
        var service = Service(gallery: gallery);
        var inspection = await service.InspectAsync(archive);

        var result = await service.ImportAsync(new MaskPackImportRequest { Inspection = inspection });

        Assert.True(result.Succeeded, result.Message);
        var page = Assert.Single((await gallery.LoadAsync()).Pages, item => item.PageId == existing.PageId);
        Assert.Equal(2, page.Items.Count);
        Assert.Contains(page.Items, item => item.GalleryItemId == "built-in:StaticImage:0");
        Assert.Contains(page.Items, item => item.GalleryItemId == "built-in:StaticImage:1");
    }

    [Fact]
    public async Task Import_LocalChangeAfterInspection_RejectsStaleConflictPreview()
    {
        var incoming = Face("face-stale", 1, 0xEE);
        await using var archive = V2Archive(Content(MaskPackContentType.Face, incoming.Id, incoming.DisplayName, MaskPackPayloadCodec.SerializeFace(incoming)));
        var faces = new InMemoryFacePatternStore();
        var service = Service(faces: faces);
        var inspection = await service.InspectAsync(archive);
        Assert.Empty(inspection.Conflicts);
        await ((IFacePatternStore)faces).UpsertAsync(Face("face-stale", 0, 0x11));

        var result = await service.ImportAsync(new MaskPackImportRequest { Inspection = inspection });

        Assert.False(result.Succeeded);
        Assert.Contains("changed after the conflict preview", result.Message, StringComparison.OrdinalIgnoreCase);
        var stored = Assert.Single((await faces.LoadAsync()).Patterns, item => item.Id == incoming.Id);
        Assert.True(stored.GetPixel(0, 0).IsLit);
    }

    [Fact]
    public async Task Import_MidTransactionFailure_RestoresEveryOriginalStoreAndClearsJournal()
    {
        var incoming = Face("face-rollback", 1, 0xEE);
        await using var archive = V2Archive(Content(MaskPackContentType.Face, incoming.Id, incoming.DisplayName, MaskPackPayloadCodec.SerializeFace(incoming)));
        var text = new InMemoryTextPresetStore();
        var faces = new InMemoryFacePatternStore();
        var animations = new FailOnceAnimationStore();
        var gallery = new InMemoryGalleryLayoutStore();
        var scenes = new InMemorySceneShowStore();
        var journal = new InMemoryMaskPackImportJournalStore();
        var service = MaskPackArchiveRoundTripTests.CreateService(text, faces, animations, gallery, scenes, journal);
        var beforeFaceIds = (await faces.LoadAsync()).Patterns.Select(item => item.Id).ToArray();
        var inspection = await service.InspectAsync(archive);

        var result = await service.ImportAsync(new MaskPackImportRequest { Inspection = inspection });

        Assert.False(result.Succeeded);
        Assert.Contains("all original stores were restored", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeFaceIds, (await faces.LoadAsync()).Patterns.Select(item => item.Id));
        Assert.Null(await journal.LoadAsync());
    }

    [Fact]
    public async Task RecoverInterruptedImport_RestoresJournalSnapshotBeforeClearingIt()
    {
        var original = Face("face-original", 0, 0x11);
        var mutated = Face("face-mutated", 1, 0xEE);
        var text = new InMemoryTextPresetStore();
        var faces = new InMemoryFacePatternStore(new FacePatternStoreState { Patterns = [mutated] });
        var animations = new InMemoryAnimationProjectStore();
        var gallery = new InMemoryGalleryLayoutStore();
        var scenes = new InMemorySceneShowStore();
        var journal = new InMemoryMaskPackImportJournalStore();
        await journal.SaveAsync(new MaskPackImportJournal
        {
            ImportId = "interrupted",
            Original = new MaskPackImportSnapshot
            {
                TextPresets = await text.LoadAsync(),
                Faces = new FacePatternStoreState { Patterns = [original] }.Normalize(),
                Animations = await animations.LoadAsync(),
                GalleryLayout = await gallery.LoadAsync(),
                SceneShow = await scenes.LoadAsync()
            }
        });
        var service = MaskPackArchiveRoundTripTests.CreateService(text, faces, animations, gallery, scenes, journal);

        var recovered = await service.RecoverInterruptedImportAsync();

        Assert.True(recovered);
        var state = await faces.LoadAsync();
        Assert.Contains(state.Patterns, item => item.Id == original.Id);
        Assert.DoesNotContain(state.Patterns, item => item.Id == mutated.Id);
        Assert.Null(await journal.LoadAsync());
    }

    [Fact]
    public async Task JsonJournal_CorruptDataIsPreservedForManualRecovery()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maskapp-journal-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "journal.json");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(path, "{not-json");
        try
        {
            var store = new JsonMaskPackImportJournalStoreCore(path);

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() => store.LoadAsync());

            Assert.Contains("preserved for manual recovery", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    internal static TestContent Content(MaskPackContentType type, string id, string name, byte[] bytes) =>
        new(type, id, name, bytes);

    internal static MemoryStream V2Archive(params TestContent[] contents)
    {
        var entries = contents.Select((content, index) => new MaskPackContentEntry
        {
            Type = content.Type,
            Id = content.Id,
            Name = content.Name,
            Path = $"content/{index:000}-{content.Id}.json",
            Sha256 = Convert.ToHexString(SHA256.HashData(content.Bytes)).ToLowerInvariant()
        }).ToArray();
        var manifest = new MaskPackManifest
        {
            SchemaVersion = 2,
            PackName = "Test Pack",
            Author = "Tests",
            ArtDisplay = new MaskPackDisplayGeometry { Width = 46, Height = 58 },
            TextDisplay = new MaskPackDisplayGeometry { Width = 44, Height = 58 },
            Contents = entries
        };
        var result = new MemoryStream();
        using (var zip = new ZipArchive(result, ZipArchiveMode.Create, leaveOpen: true))
        {
            Write(zip, "manifest.json", System.Text.Encoding.UTF8.GetBytes(MaskPackManifestParser.ToJson(manifest)));
            for (var index = 0; index < contents.Length; index++)
            {
                Write(zip, entries[index].Path, contents[index].Bytes);
            }
        }

        result.Position = 0;
        return result;
    }

    internal static void Write(ZipArchive archive, string path, byte[] bytes)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        stream.Write(bytes);
    }

    private static MaskPackArchiveService Service(
        IFacePatternStore? faces = null,
        IGalleryLayoutStore? gallery = null) =>
        MaskPackArchiveRoundTripTests.CreateService(
            new InMemoryTextPresetStore(),
            faces ?? new InMemoryFacePatternStore(),
            new InMemoryAnimationProjectStore(),
            gallery ?? new InMemoryGalleryLayoutStore(),
            new InMemorySceneShowStore());

    private static FacePattern Face(string id, int litIndex, byte red) =>
        MaskPackArchiveRoundTripTests.CreateFace(id, "Conflict Face", litIndex, new FaceColor(red, 0x22, 0x33));

    internal sealed record TestContent(MaskPackContentType Type, string Id, string Name, byte[] Bytes);

    private sealed class FailOnceAnimationStore : IAnimationProjectStore
    {
        private AnimationProjectStoreState state = new();
        private bool fail = true;

        public Task<AnimationProjectStoreState> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(state);

        public Task SaveAsync(AnimationProjectStoreState state, CancellationToken cancellationToken = default)
        {
            if (fail)
            {
                fail = false;
                throw new IOException("Injected animation store failure.");
            }

            this.state = state.Normalize();
            return Task.CompletedTask;
        }
    }
}
