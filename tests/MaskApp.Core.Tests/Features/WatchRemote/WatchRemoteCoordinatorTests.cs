using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.Stage;
using MaskApp.Core.Features.WatchRemote;

namespace MaskApp.Core.Tests.Features.WatchRemote;

public sealed class WatchRemoteCoordinatorTests
{
    [Fact]
    public async Task State_ReportsSetlistPositionConnectionReadinessAndOnlyTriggerableFavorites()
    {
        var fixture = CreateFixture();

        var state = await fixture.Coordinator.GetStateAsync();

        Assert.Equal("Setlist", state.PositionKind);
        Assert.Equal("Cue 1 of 2", state.PositionText);
        Assert.Equal("cue-a", state.CurrentCueId);
        Assert.Equal("Cue A", state.CurrentCueLabel);
        Assert.Equal("Cue B", state.NextCueLabel);
        Assert.True(state.MaskConnected);
        Assert.Equal("SHOW READY", state.ReadinessStatus);
        var favorite = Assert.Single(state.Favorites);
        Assert.Equal("favorite-a", favorite.Id);
        Assert.Equal("Favorite A", favorite.Label);
    }

    [Fact]
    public async Task NavigationAndCurrentCue_UseTheStageSourceWithoutDirectBleAccess()
    {
        var fixture = CreateFixture();

        var moved = await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.NextCue
        });
        var triggered = await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.TriggerCurrentCue
        });

        Assert.True(moved.Succeeded);
        Assert.True(triggered.Succeeded);
        Assert.Equal(1, fixture.ShowSource.Current.PageIndex);
        Assert.Equal(["cue-b"], fixture.ShowSource.TriggeredIds);
        Assert.Empty(fixture.Dispatcher.DirectActions);
    }

    [Fact]
    public async Task FavoriteBrightnessStopAndBlackout_UseTheSharedSceneDispatcher()
    {
        var fixture = CreateFixture();

        Assert.True((await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.TriggerFavorite,
            FavoriteId = "favorite-a"
        })).Succeeded);
        Assert.True((await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.SetBrightness,
            Brightness = 72
        })).Succeeded);
        Assert.True((await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.Stop
        })).Succeeded);
        Assert.True((await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.Blackout
        })).Succeeded);

        Assert.Equal(["favorite-a", "brightness:72", "stop", "blackout"], fixture.Dispatcher.DirectActions);
    }

    [Fact]
    public async Task PageModeCurrentCueAndMissingFavorite_FailWithoutOutput()
    {
        var fixture = CreateFixture(positionLabel: "Page");

        var cue = await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.TriggerCurrentCue
        });
        var favorite = await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.TriggerFavorite,
            FavoriteId = "missing"
        });

        Assert.False(cue.Succeeded);
        Assert.False(favorite.Succeeded);
        Assert.Empty(fixture.ShowSource.TriggeredIds);
        Assert.Empty(fixture.Dispatcher.DirectActions);
    }

    [Fact]
    public async Task Blackout_BypassesAStillRunningCueDispatch()
    {
        var fixture = CreateFixture();
        fixture.ShowSource.BlockTrigger = true;

        var cue = fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.TriggerCurrentCue
        });
        await fixture.ShowSource.TriggerStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var blackout = await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.Blackout
        }).WaitAsync(TimeSpan.FromSeconds(1));
        fixture.ShowSource.ReleaseTrigger.TrySetResult();
        await cue;

        Assert.True(blackout.Succeeded);
        Assert.Contains("blackout", fixture.Dispatcher.DirectActions);
    }

    [Fact]
    public async Task BackgroundPhone_BlocksOrdinaryOutputButKeepsEmergencyBlackoutAvailable()
    {
        var fixture = CreateFixture();
        fixture.ExecutionSession.SetForeground(false);

        var brightness = await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.SetBrightness,
            Brightness = 50
        });
        var blackout = await fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.Blackout
        });
        var state = await fixture.Coordinator.GetStateAsync();

        Assert.False(brightness.Succeeded);
        Assert.Contains("foreground", brightness.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(blackout.Succeeded);
        Assert.False(state.PhoneForeground);
        Assert.Equal(["blackout"], fixture.Dispatcher.DirectActions);
    }

    [Fact]
    public async Task BackgroundPhone_RejectsOrdinaryActionAlreadyWaitingForTheDispatchGate()
    {
        var fixture = CreateFixture();
        fixture.ShowSource.BlockTrigger = true;

        var cue = fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.TriggerCurrentCue
        });
        await fixture.ShowSource.TriggerStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));

        var brightness = fixture.Coordinator.DispatchAsync(new WatchRemoteAction
        {
            Kind = WatchRemoteActionKind.SetBrightness,
            Brightness = 50
        });
        fixture.ExecutionSession.SetForeground(false);
        fixture.ShowSource.ReleaseTrigger.TrySetResult();

        Assert.True((await cue).Succeeded);
        var result = await brightness;
        Assert.False(result.Succeeded);
        Assert.Contains("foreground", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(fixture.Dispatcher.DirectActions);
    }

    private static Fixture CreateFixture(string positionLabel = "Cue")
    {
        var showSource = new FakeStageShowSource(positionLabel);
        var catalog = new FakeCatalogSource(
        [
            new GalleryItem
            {
                Id = "favorite-a",
                Title = "Favorite A",
                Type = GalleryItemType.QuickAction,
                IsFavorite = true,
                QuickActionId = QuickActionId.Blackout
            },
            new GalleryItem
            {
                Id = "scene-favorite",
                Title = "Scene favorite",
                Type = GalleryItemType.Scene,
                IsFavorite = true
            },
            new GalleryItem
            {
                Id = "not-favorite",
                Title = "Not favorite",
                Type = GalleryItemType.QuickAction,
                IsFavorite = false
            }
        ]);
        var dispatcher = new FakeSceneItemDispatcher();
        var executionSession = new WatchRemoteExecutionSession();
        executionSession.SetForeground(true);
        var coordinator = new WatchRemoteCoordinator(
            showSource,
            catalog,
            dispatcher,
            new ReadyProvider(),
            new ConnectedDevice(),
            executionSession);
        return new Fixture(coordinator, showSource, dispatcher, executionSession);
    }

    private sealed record Fixture(
        WatchRemoteCoordinator Coordinator,
        FakeStageShowSource ShowSource,
        FakeSceneItemDispatcher Dispatcher,
        WatchRemoteExecutionSession ExecutionSession);

    private sealed class FakeStageShowSource : IStageShowSource
    {
        private readonly string positionLabel;

        public FakeStageShowSource(string positionLabel)
        {
            this.positionLabel = positionLabel;
            Current = Create(0);
        }

        public StageShowSnapshot Current { get; private set; }

        public List<string> TriggeredIds { get; } = [];

        public bool BlockTrigger { get; set; }

        public TaskCompletionSource TriggerStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource ReleaseTrigger { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<StageShowSnapshot> InitializeAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Current);

        public Task<StageShowSnapshot> SelectPageAsync(
            int pageIndex,
            CancellationToken cancellationToken = default)
        {
            Current = Create(Math.Clamp(pageIndex, 0, 1));
            return Task.FromResult(Current);
        }

        public async Task<GalleryActionResult> TriggerAsync(
            string tileId,
            CancellationToken cancellationToken = default)
        {
            TriggeredIds.Add(tileId);
            if (BlockTrigger)
            {
                TriggerStarted.TrySetResult();
                await ReleaseTrigger.Task.WaitAsync(cancellationToken);
            }

            return GalleryActionResult.Success("Cue sent.");
        }

        public void StartObservingTransportState()
        {
        }

        public void StopObservingTransportState()
        {
        }

        private StageShowSnapshot Create(int index) => new(
            $"position-{index}",
            index == 0 ? "Cue A" : "Cue B",
            "#A78BFA",
            index,
            2,
            [new StageTile(
                index == 0 ? "cue-a" : "cue-b",
                index == 0 ? "Cue A" : "Cue B",
                "Scene",
                "#A78BFA",
                GalleryItemType.Scene,
                true,
                false,
                "Ready")],
            positionLabel,
            index == 0 ? "Cue B" : string.Empty);
    }

    private sealed class FakeCatalogSource(IReadOnlyList<GalleryItem> items) : ISceneCatalogSource
    {
        public Task<IReadOnlyList<GalleryItem>> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class FakeSceneItemDispatcher : ISceneItemDispatcher
    {
        public List<string> DirectActions { get; } = [];

        public Task<GalleryActionResult> TriggerAsync(
            GalleryItem item,
            CancellationToken cancellationToken = default)
        {
            DirectActions.Add(item.Id);
            return Task.FromResult(GalleryActionResult.Success("Favorite sent."));
        }

        public Task<MaskCommandResult> SetBrightnessAsync(
            int brightness,
            CancellationToken cancellationToken = default)
        {
            DirectActions.Add($"brightness:{brightness}");
            return Task.FromResult(MaskCommandResult.Success("Brightness sent."));
        }

        public Task<MaskCommandResult> SetAnimationSpeedAsync(
            int speed,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Speed sent."));

        public Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default)
        {
            DirectActions.Add("stop");
            return Task.FromResult(MaskCommandResult.Success("Stopped."));
        }

        public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default)
        {
            DirectActions.Add("blackout");
            return Task.FromResult(MaskCommandResult.Success("Blacked out."));
        }
    }

    private sealed class ReadyProvider : IStageReadinessProvider
    {
        public Task<StageReadinessSnapshot> EvaluateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new StageReadinessSnapshot(
                FestivalPreflightStatus.ShowReady,
                "SHOW READY",
                "Prepared for Stage.",
                0,
                0));
    }

    private sealed class ConnectedDevice : IBleDeviceConnection
    {
        public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged
        {
            add { }
            remove { }
        }

        public BleConnectionState State => BleConnectionState.Connected;

        public Task ConnectAsync(
            DiscoveredMaskDevice device,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
