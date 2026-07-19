using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.Core.Tests.Features.Scenes;

public sealed class ScenePersistenceAndStudioTests
{
    [Fact]
    public void Normalize_MigratesLegacyHolyPriestGalleryReferences()
    {
        var state = new SceneShowState
        {
            Scenes =
            [
                new PerformanceScene
                {
                    Id = "legacy-scene",
                    DisplayName = "Legacy Scene",
                    Steps =
                    [
                        new PerformanceSceneStep
                        {
                            Id = "animation",
                            Kind = SceneStepKind.Animation,
                            GalleryItemId = "app-animation:holy-priest-ritual-inversion"
                        },
                        new PerformanceSceneStep
                        {
                            Id = "face",
                            Kind = SceneStepKind.Face,
                            GalleryItemId = "face:built-in-face-holy-priest-retro-future"
                        }
                    ]
                }
            ]
        };

        var normalized = state.Normalize();
        var steps = Assert.Single(normalized.Scenes).Steps;

        Assert.Equal("app-animation:holy-priest-five-mask-cycle", steps[0].GalleryItemId);
        Assert.Equal("face:built-in-face-holy-priest-original", steps[1].GalleryItemId);
    }

    [Fact]
    public async Task JsonStore_RoundTripsVersionedScenesSetlistsAndPosition()
    {
        var path = TempPath();
        try
        {
            var scene = PerformanceScene.CreateBlank("Opening");
            var setlist = new PerformanceSetlist
            {
                Id = "set-main",
                DisplayName = "Main",
                Cues = [new PerformanceSetlistCue { Id = "cue-open", Label = "Open", SceneId = scene.Id }]
            };
            var source = new SceneShowState
            {
                Scenes = [scene],
                Setlists = [setlist],
                ActiveSetlistId = setlist.Id,
                Positions = [new SetlistPosition(setlist.Id, 0, DateTimeOffset.Parse("2026-07-17T12:00:00Z"))]
            };
            var store = new JsonSceneShowStoreCore(path);

            await store.SaveAsync(source);
            var loaded = await store.LoadAsync();

            Assert.Equal(SceneShowState.CurrentSchemaVersion, loaded.SchemaVersion);
            Assert.Equal(scene.Id, Assert.Single(loaded.Scenes).Id);
            Assert.Equal(setlist.Id, Assert.Single(loaded.Setlists).Id);
            Assert.Equal(setlist.Id, loaded.ActiveSetlistId);
            Assert.False(loaded.UsedFallback);
        }
        finally
        {
            DeleteTemp(path);
        }
    }

    [Fact]
    public async Task JsonStore_CorruptInputUsesProtectedFallback()
    {
        var path = TempPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, "{not-json");
            var store = new JsonSceneShowStoreCore(path);

            var fallback = await store.LoadAsync();

            Assert.True(fallback.UsedFallback);
            Assert.Empty(fallback.Scenes);
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.SaveAsync(fallback));
            Assert.Equal("{not-json", await File.ReadAllTextAsync(path));
        }
        finally
        {
            DeleteTemp(path);
        }
    }

    [Fact]
    public async Task SetlistCoordinator_PersistsCurrentCueAcrossInstances()
    {
        var firstScene = PerformanceScene.CreateBlank("One");
        var secondScene = PerformanceScene.CreateBlank("Two");
        var setlist = new PerformanceSetlist
        {
            Id = "set-main",
            DisplayName = "Main",
            Cues =
            [
                new PerformanceSetlistCue { Id = "cue-one", SceneId = firstScene.Id, Label = "One" },
                new PerformanceSetlistCue { Id = "cue-two", SceneId = secondScene.Id, Label = "Two" }
            ]
        };
        var store = new InMemorySceneShowStore(new SceneShowState
        {
            Scenes = [firstScene, secondScene],
            Setlists = [setlist]
        });
        var engine = CreateEngine([]);
        var coordinator = new SetlistCoordinator(store, engine);

        await coordinator.ActivateAsync(setlist.Id);
        Assert.Equal("cue-two", coordinator.Current.NextCue?.Id);
        var moved = await coordinator.NextAsync();
        var reloaded = await new SetlistCoordinator(store, engine).InitializeAsync();

        Assert.Equal(1, moved.CueIndex);
        Assert.Equal("cue-two", moved.CurrentCue?.Id);
        Assert.Null(moved.NextCue);
        Assert.Equal(1, reloaded.CueIndex);
        Assert.Equal("cue-two", reloaded.CurrentCue?.Id);
    }

    [Fact]
    public void CatalogAndReadiness_ProjectValidSceneAndPreparedDependencies()
    {
        var face = FacePatternFactory.CreateBlank("Ready face", preferredSlot: 4);
        var faceItem = new GalleryItem
        {
            Id = $"face:{face.Id}",
            Type = GalleryItemType.CustomStaticFace,
            Title = face.DisplayName,
            FacePattern = face
        };
        var scene = new PerformanceScene
        {
            Id = "scene-face",
            DisplayName = "Face cue",
            Steps =
            [
                new PerformanceSceneStep
                {
                    Id = "face",
                    Kind = SceneStepKind.Face,
                    GalleryItemId = faceItem.Id
                }
            ]
        };
        var catalog = new GalleryCatalogBuilder(new QuickActionCatalog()).Build(
            new(),
            new(),
            new FacePatternStoreState { Patterns = [face] },
            new(),
            new(),
            new SceneShowState { Scenes = [scene] });
        var sceneItem = Assert.Single(catalog, item => item.Type == GalleryItemType.Scene);
        var byId = catalog.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var notPrepared = new SceneReadinessEvaluator().Evaluate(scene, byId, new FacePatternStoreState());
        var preparedState = new FacePatternStoreState().MarkSlotInstalled(
            face.PreferredSlot,
            FaceContentFingerprint.Compute(face),
            faceItem.Id,
            DateTimeOffset.UtcNow);
        var prepared = new SceneReadinessEvaluator().Evaluate(scene, byId, preparedState);

        Assert.Equal("scene:scene-face", sceneItem.Id);
        Assert.Equal("scene-editor", sceneItem.ManageTarget);
        Assert.True(sceneItem.CanSend);
        Assert.False(notPrepared.IsReady);
        Assert.True(prepared.IsReady);
    }

    [Fact]
    public async Task Studio_EditsOrdersDuplicatesSavesAndActivatesSetlist()
    {
        var faceItem = new GalleryItem
        {
            Id = "face:test",
            Type = GalleryItemType.CustomStaticFace,
            Title = "Test face"
        };
        var store = new InMemorySceneShowStore();
        var engine = CreateEngine([faceItem]);
        var coordinator = new SetlistCoordinator(store, engine);
        var viewModel = new SceneStudioViewModel(
            store,
            new StaticCatalogSource([faceItem]),
            new SceneValidator(),
            engine,
            coordinator);
        await viewModel.InitializeAsync();

        Assert.True(viewModel.AddStep());
        Assert.Equal(2, viewModel.StepRows.Count);
        Assert.True(viewModel.MoveSelectedStep(-1));
        await viewModel.SaveSceneCommand.ExecuteAsync();
        Assert.Single((await store.LoadAsync()).Scenes);

        await viewModel.DuplicateSceneCommand.ExecuteAsync();
        var duplicated = await store.LoadAsync();
        Assert.Equal(2, duplicated.Scenes.Count);
        Assert.Equal(2, duplicated.Scenes.Select(scene => scene.Id).Distinct().Count());

        viewModel.SelectedCueScene = viewModel.Scenes[0];
        Assert.True(viewModel.AddCue());
        await viewModel.SaveSetlistCommand.ExecuteAsync();
        await viewModel.ActivateSetlistCommand.ExecuteAsync();
        var activated = await store.LoadAsync();

        Assert.Single(activated.Setlists);
        Assert.Single(activated.Setlists[0].Cues);
        Assert.Equal(activated.Setlists[0].Id, activated.ActiveSetlistId);
    }

    private static SceneExecutionEngine CreateEngine(IReadOnlyList<GalleryItem> catalog) => new(
        new SceneValidator(),
        new StaticCatalogSource(catalog),
        new SuccessfulDispatcher(),
        new ImmediateClock());

    private static string TempPath() =>
        Path.Combine(Path.GetTempPath(), $"maskapp-scenes-{Guid.NewGuid():N}", "scenes.json");

    private static void DeleteTemp(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private sealed class StaticCatalogSource(IReadOnlyList<GalleryItem> items) : ISceneCatalogSource
    {
        public Task<IReadOnlyList<GalleryItem>> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class SuccessfulDispatcher : ISceneItemDispatcher
    {
        public Task<GalleryActionResult> TriggerAsync(GalleryItem item, CancellationToken cancellationToken = default) =>
            Task.FromResult(GalleryActionResult.Success("Triggered."));
        public Task<MaskCommandResult> SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Brightness."));
        public Task<MaskCommandResult> SetAnimationSpeedAsync(int speed, CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Speed."));
        public Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Stopped."));
        public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Blacked out."));
    }

    private sealed class ImmediateClock : IAnimationClock
    {
        public long GetTimestamp() => 0;
        public long Add(long timestamp, TimeSpan duration) => timestamp + duration.Ticks;
        public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
            TimeSpan.FromTicks(endingTimestamp - startingTimestamp);
        public Task DelayUntilAsync(long deadlineTimestamp, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
