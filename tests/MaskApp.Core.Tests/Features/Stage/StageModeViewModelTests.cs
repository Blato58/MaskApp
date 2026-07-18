using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Stage;

namespace MaskApp.Core.Tests.Features.Stage;

public sealed class StageModeViewModelTests
{
    [Fact]
    public async Task Activate_LocksStage_KeepsDisplayAwake_AndLoadsReadiness()
    {
        var fixture = CreateFixture();

        await fixture.ViewModel.ActivateAsync();

        Assert.True(fixture.ViewModel.IsLocked);
        Assert.True(fixture.Display.IsKeepAwake);
        Assert.Equal("SHOW READY", fixture.ViewModel.ReadinessText);
        Assert.Equal("Page A", fixture.ViewModel.PageTitle);
        Assert.Equal(2, fixture.ViewModel.Tiles.Count);
        Assert.Equal(1, fixture.Source.InitializeCount);

        await fixture.ViewModel.DeactivateAsync();
        Assert.False(fixture.Display.IsKeepAwake);
    }

    [Fact]
    public async Task LayoutAndPageControls_CycleThreeModes_AndMoveWithoutOutput()
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();

        await fixture.ViewModel.CycleLayoutCommand.ExecuteAsync();
        Assert.Equal(StageLayoutMode.Dense, fixture.ViewModel.LayoutMode);
        Assert.Equal(3, fixture.ViewModel.GridSpan);
        await fixture.ViewModel.CycleLayoutCommand.ExecuteAsync();
        Assert.Equal(StageLayoutMode.Giant, fixture.ViewModel.LayoutMode);
        Assert.Equal(1, fixture.ViewModel.GridSpan);
        await fixture.ViewModel.CycleLayoutCommand.ExecuteAsync();
        Assert.Equal(StageLayoutMode.Grid2x2, fixture.ViewModel.LayoutMode);

        await fixture.ViewModel.NextPageCommand.ExecuteAsync();
        Assert.Equal("Page B", fixture.ViewModel.PageTitle);
        Assert.Equal(1, fixture.ViewModel.Show.PageIndex);
        Assert.Empty(fixture.Source.TriggeredTileIds);
    }

    [Fact]
    public async Task SuccessfulAndFailedTiles_ProduceTypedHapticsAndCueState()
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();
        var first = fixture.ViewModel.Tiles[0];
        var second = fixture.ViewModel.Tiles[1];
        fixture.Source.Results[second.TileId] = GalleryActionResult.Failure("Transport failed.");

        await fixture.ViewModel.TriggerAsync(first);
        Assert.Equal(StageActionDeliveryState.Sent, fixture.ViewModel.ActionDeliveryState);
        Assert.Equal("SENT", fixture.ViewModel.ActionDeliveryText);
        await fixture.ViewModel.TriggerAsync(second);

        Assert.Equal([first.TileId, second.TileId], fixture.Source.TriggeredTileIds);
        Assert.Equal($"Current: {first.Label}", fixture.ViewModel.CurrentCueText);
        Assert.Equal(1, fixture.Feedback.SuccessCount);
        Assert.Equal(1, fixture.Feedback.FailureCount);
        Assert.Equal("Transport failed.", fixture.ViewModel.StatusText);
        Assert.Equal(StageActionDeliveryState.Failed, fixture.ViewModel.ActionDeliveryState);
        Assert.Equal("Transport failed.", fixture.ViewModel.ActionDeliveryDetail);
    }

    [Fact]
    public async Task UnpreparedTile_IsBlockedWithoutStartingLiveUpload()
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();
        var unprepared = fixture.ViewModel.Tiles[0] with { IsPrepared = false };

        await fixture.ViewModel.TriggerAsync(unprepared);

        Assert.Empty(fixture.Source.TriggeredTileIds);
        Assert.Contains("not prepared", fixture.ViewModel.StatusText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(StageActionDeliveryState.Failed, fixture.ViewModel.ActionDeliveryState);
        Assert.Equal(1, fixture.Feedback.FailureCount);
    }

    [Fact]
    public async Task HoldRelease_RestoresPreviousStableTile()
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();
        var stable = fixture.ViewModel.Tiles[0];
        var hold = fixture.ViewModel.Tiles[1];
        Assert.True(hold.IsHoldAction);

        await fixture.ViewModel.TriggerAsync(stable);
        await fixture.ViewModel.BeginHoldAsync(hold);
        Assert.True(fixture.ViewModel.IsHoldActive);
        await fixture.ViewModel.EndHoldAsync(hold);

        Assert.False(fixture.ViewModel.IsHoldActive);
        Assert.Equal([stable.TileId, hold.TileId, stable.TileId], fixture.Source.TriggeredTileIds);
        Assert.Contains("restored", fixture.ViewModel.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Deactivate_ReleasesActiveHoldBeforeLeavingStage()
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();
        var stable = fixture.ViewModel.Tiles[0];
        var hold = fixture.ViewModel.Tiles[1];
        await fixture.ViewModel.TriggerAsync(stable);
        await fixture.ViewModel.BeginHoldAsync(hold);

        await fixture.ViewModel.DeactivateAsync();

        Assert.False(fixture.ViewModel.IsHoldActive);
        Assert.False(fixture.Display.IsKeepAwake);
        Assert.Equal([stable.TileId, hold.TileId, stable.TileId], fixture.Source.TriggeredTileIds);
    }

    [Fact]
    public async Task DisconnectPreservesSession_ReconnectRequiresExplicitRecovery_AndNeverReplays()
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();
        await fixture.ViewModel.TriggerAsync(fixture.ViewModel.Tiles[0]);
        var triggerCount = fixture.Source.TriggeredTileIds.Count;

        fixture.Connection.SetState(BleConnectionState.Disconnected, "Radio lost.");
        Assert.True(fixture.ViewModel.HasConnectionOverlay);
        Assert.True(fixture.ViewModel.RequiresExplicitRecovery);
        await fixture.ViewModel.TriggerAsync(fixture.ViewModel.Tiles[0]);
        Assert.Equal(triggerCount, fixture.Source.TriggeredTileIds.Count);

        fixture.Connection.SetState(BleConnectionState.Connected, "Connected again.");
        Assert.True(fixture.ViewModel.HasConnectionOverlay);
        Assert.True(fixture.ViewModel.CanRecover);
        Assert.Equal(triggerCount, fixture.Source.TriggeredTileIds.Count);

        await fixture.ViewModel.RecoverCommand.ExecuteAsync();
        Assert.False(fixture.ViewModel.HasConnectionOverlay);
        Assert.False(fixture.ViewModel.RequiresExplicitRecovery);
        Assert.Equal(triggerCount, fixture.Source.TriggeredTileIds.Count);
        Assert.Contains("Nothing was replayed", fixture.ViewModel.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StopAndBlackout_AreDistinct_AndBlackoutPreemptsBlockedTileAction()
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();
        var releaseTrigger = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        fixture.Source.TriggerRelease = releaseTrigger.Task;
        var triggerTask = fixture.ViewModel.TriggerAsync(fixture.ViewModel.Tiles[0]);
        await fixture.Source.TriggerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await fixture.ViewModel.BlackoutCommand.ExecuteAsync();
        Assert.Equal(1, fixture.Emergency.BlackoutCount);
        Assert.Equal(0, fixture.Emergency.StopCount);
        Assert.Equal("Current: BLACKOUT", fixture.ViewModel.CurrentCueText);

        releaseTrigger.SetResult(true);
        await triggerTask;
        await fixture.ViewModel.StopCommand.ExecuteAsync();
        Assert.Equal(1, fixture.Emergency.BlackoutCount);
        Assert.Equal(1, fixture.Emergency.StopCount);
        Assert.Contains("last stable look", fixture.ViewModel.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1999, false)]
    [InlineData(2000, true)]
    [InlineData(2500, true)]
    public async Task UnlockRequiresFullHoldDuration(int milliseconds, bool expected)
    {
        var fixture = CreateFixture();
        await fixture.ViewModel.ActivateAsync();

        var unlocked = fixture.ViewModel.TryUnlock(TimeSpan.FromMilliseconds(milliseconds));

        Assert.Equal(expected, unlocked);
        Assert.Equal(!expected, fixture.ViewModel.IsLocked);
        Assert.Equal(!expected, fixture.Display.IsKeepAwake);
    }

    private static Fixture CreateFixture()
    {
        var source = new FakeStageShowSource();
        var readiness = new FakeReadinessProvider();
        var connection = new FakeConnection();
        var feedback = new RecordingFeedback();
        var display = new RecordingDisplayControl();
        var commandTransport = new RecordingCommandTransport();
        var emergency = new RecordingEmergencyControl();
        var engine = new PerformanceAnimationEngine(commandTransport, emergency);
        var coordinator = new DiySlotPlaybackCoordinator(
            new InMemoryFacePatternStore(),
            new ReadyFaceTransport(),
            commandTransport,
            engine,
            new PerformanceAnimationBuilder());
        var viewModel = new StageModeViewModel(
            source,
            readiness,
            connection,
            coordinator,
            engine,
            emergency,
            feedback,
            display);
        return new Fixture(viewModel, source, connection, emergency, feedback, display);
    }

    private sealed record Fixture(
        StageModeViewModel ViewModel,
        FakeStageShowSource Source,
        FakeConnection Connection,
        RecordingEmergencyControl Emergency,
        RecordingFeedback Feedback,
        RecordingDisplayControl Display);

    private sealed class FakeStageShowSource : IStageShowSource
    {
        private readonly StageShowSnapshot[] pages =
        [
            new StageShowSnapshot(
                "page-a",
                "Page A",
                "#52E3FF",
                0,
                2,
                [
                    new StageTile("stable", "Stable Face", "Face", "#52E3FF", GalleryItemType.BuiltInStaticImage, true, false, "Instant"),
                    new StageTile("hold", "Pulse", "Animation", "#FF375F", GalleryItemType.AppBuiltInAnimation, true, true, "Prepared")
                ]),
            new StageShowSnapshot(
                "page-b",
                "Page B",
                "#FFD60A",
                1,
                2,
                [new StageTile("text", "Caption", "Text", "#FFD60A", GalleryItemType.TextPreset, true, false, "Prepared")])
        ];
        private int selectedIndex;

        public Dictionary<string, GalleryActionResult> Results { get; } = new(StringComparer.Ordinal);

        public List<string> TriggeredTileIds { get; } = [];

        public int InitializeCount { get; private set; }

        public TaskCompletionSource<bool> TriggerStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task? TriggerRelease { get; set; }

        public Task<StageShowSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
        {
            InitializeCount++;
            return Task.FromResult(pages[selectedIndex]);
        }

        public Task<StageShowSnapshot> SelectPageAsync(
            int pageIndex,
            CancellationToken cancellationToken = default)
        {
            selectedIndex = Math.Clamp(pageIndex, 0, pages.Length - 1);
            return Task.FromResult(pages[selectedIndex]);
        }

        public async Task<GalleryActionResult> TriggerAsync(
            string tileId,
            CancellationToken cancellationToken = default)
        {
            TriggeredTileIds.Add(tileId);
            TriggerStarted.TrySetResult(true);
            if (TriggerRelease is not null)
            {
                await TriggerRelease.WaitAsync(cancellationToken);
            }

            return Results.GetValueOrDefault(tileId, GalleryActionResult.Success("Sent."));
        }

        public void StartObservingTransportState()
        {
        }

        public void StopObservingTransportState()
        {
        }
    }

    private sealed class FakeReadinessProvider : IStageReadinessProvider
    {
        public Task<StageReadinessSnapshot> EvaluateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new StageReadinessSnapshot(
                FestivalPreflightStatus.ShowReady,
                "SHOW READY",
                "All test actions are ready.",
                0,
                0));
    }

    private sealed class FakeConnection : IBleDeviceConnection
    {
        public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;

        public BleConnectionState State { get; private set; } = BleConnectionState.Connected;

        public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void SetState(BleConnectionState state, string message)
        {
            State = state;
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(state, message));
        }
    }

    private sealed class RecordingFeedback : IStageDeviceFeedback
    {
        public int SuccessCount { get; private set; }

        public int FailureCount { get; private set; }

        public int WarningCount { get; private set; }

        public void Success() => SuccessCount++;

        public void Failure() => FailureCount++;

        public void Warning() => WarningCount++;
    }

    private sealed class RecordingDisplayControl : IStageDisplayControl
    {
        public bool IsKeepAwake { get; private set; }

        public void SetKeepAwake(bool enabled) => IsKeepAwake = enabled;
    }

    private sealed class RecordingEmergencyControl : IMaskEmergencyControl
    {
        public int StopCount { get; private set; }

        public int BlackoutCount { get; private set; }

        public Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default)
        {
            StopCount++;
            return Task.FromResult(MaskCommandResult.Success("Stopped."));
        }

        public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default)
        {
            BlackoutCount++;
            return Task.FromResult(MaskCommandResult.Success("Blacked out."));
        }
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

        public string TransportStatusText => "Ready.";

        public Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Sent."));
    }

    private sealed class ReadyFaceTransport : IFaceUploadTransport
    {
        public event EventHandler<FaceUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        public FaceUploadTransportState State => FaceUploadTransportState.Ready;

        public string StatusText => "Ready.";

        public Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(FaceUploadResult.Success("Uploaded.", package.Frames.Count));
    }
}
