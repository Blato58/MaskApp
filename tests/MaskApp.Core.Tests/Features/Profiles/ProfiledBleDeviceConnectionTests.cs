using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Profiles;

public sealed class ProfiledBleDeviceConnectionTests
{
    [Fact]
    public async Task CapabilitiesAreObservedAfterTransportReadiness_NotRawConnectedEvent()
    {
        var profileStore = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(profileStore);
        var transport = new FakeCombinedTransport();
        using var connection = new ProfiledBleDeviceConnection(
            transport,
            session,
            transport,
            transport,
            transport,
            transport);
        var device = new DiscoveredMaskDevice("device-a", "Mask A", -40);

        await connection.ConnectAsync(device);
        var connectedProfile = await session.GetActiveProfileAsync();
        Assert.Equal(string.Empty, connectedProfile!.Capabilities.TransportName);

        transport.RaiseReady();
        await connection.ProfileUpdateTask.WaitAsync(TimeSpan.FromSeconds(5));
        var readyProfile = await session.GetActiveProfileAsync();
        Assert.True(readyProfile!.Capabilities.CommandWriteAvailable);
        Assert.True(readyProfile.Capabilities.TextUploadAvailable);
        Assert.True(readyProfile.Capabilities.AudioVisualizationWriteAvailable);
        Assert.Equal(MaskAcknowledgementMode.Acknowledged, readyProfile.Capabilities.AcknowledgementMode);

        await session.ReplacePreparedSlotsAsync(
        [
            new FaceSlotInstallation
            {
                Slot = 7,
                ContentFingerprint = "READY",
                SourceId = "face",
                InstalledAt = DateTimeOffset.UtcNow
            }
        ]);
        await connection.ConnectAsync(device);
        transport.RaiseReady();
        await connection.ProfileUpdateTask.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Single((await session.GetActiveProfileAsync())!.PreparedSlots);
        Assert.Equal(string.Empty, connection.LastProfileError);
    }

    [Fact]
    public async Task FailedMaskSwitch_LeavesPreviousProfileActive()
    {
        var profileStore = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(profileStore);
        var transport = new FakeCombinedTransport();
        using var connection = new ProfiledBleDeviceConnection(
            transport,
            session,
            transport,
            transport,
            transport,
            transport);
        var firstDevice = new DiscoveredMaskDevice("device-a", "Mask A", -40);
        var failedDevice = new DiscoveredMaskDevice("device-b", "Mask B", -45);

        await connection.ConnectAsync(firstDevice);
        var firstProfile = await session.GetActiveProfileAsync();
        transport.ConnectException = new InvalidOperationException("Connection failed.");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => connection.ConnectAsync(failedDevice));

        Assert.Equal("Connection failed.", exception.Message);
        Assert.Equal(firstProfile!.ProfileId, session.ActiveProfileId);
        Assert.Equal(firstProfile.ProfileId, (await session.GetActiveProfileAsync())!.ProfileId);
        Assert.DoesNotContain(
            (await profileStore.LoadAsync()).Profiles,
            profile => string.Equals(profile.DisplayName, "Mask B", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReadinessRaisedDuringConnect_IsRecordedOnlyAfterTargetProfileActivation()
    {
        var profileStore = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(profileStore);
        var transport = new FakeCombinedTransport();
        using var connection = new ProfiledBleDeviceConnection(
            transport,
            session,
            transport,
            transport,
            transport,
            transport);

        await connection.ConnectAsync(new DiscoveredMaskDevice("device-a", "Mask A", -40));
        transport.RaiseReadyDuringConnect = true;
        await connection.ConnectAsync(new DiscoveredMaskDevice("device-b", "Mask B", -45));
        await connection.ProfileUpdateTask.WaitAsync(TimeSpan.FromSeconds(5));

        var state = await profileStore.LoadAsync();
        var first = state.Profiles.Single(profile => profile.DisplayName == "Mask A");
        var second = state.Profiles.Single(profile => profile.DisplayName == "Mask B");
        Assert.False(first.Capabilities.CommandWriteAvailable);
        Assert.True(second.Capabilities.CommandWriteAvailable);
    }

    [Fact]
    public async Task NativeStyleLateFailure_LeavesPreviousProfileActive()
    {
        var profileStore = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(profileStore);
        var transport = new FakeCombinedTransport();
        using var connection = new ProfiledBleDeviceConnection(
            transport,
            session,
            transport,
            transport,
            transport,
            transport);

        await connection.ConnectAsync(new DiscoveredMaskDevice("device-a", "Mask A", -40));
        var firstProfileId = session.ActiveProfileId;
        transport.CompleteConnectSynchronously = false;

        await connection.ConnectAsync(new DiscoveredMaskDevice("device-b", "Mask B", -45));
        transport.RaiseConnectionFailure("Native link failed.");

        Assert.Equal(firstProfileId, session.ActiveProfileId);
        Assert.Equal(firstProfileId, (await session.GetActiveProfileAsync())!.ProfileId);
        Assert.DoesNotContain(
            (await profileStore.LoadAsync()).Profiles,
            profile => string.Equals(profile.DisplayName, "Mask B", StringComparison.Ordinal));
    }

    [Fact]
    public async Task NativeStyleLateSuccess_ActivatesProfileBeforePublishingConnected()
    {
        var profileStore = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(profileStore);
        var transport = new FakeCombinedTransport { CompleteConnectSynchronously = false };
        using var connection = new ProfiledBleDeviceConnection(
            transport,
            session,
            transport,
            transport,
            transport,
            transport);
        var publishedActiveProfileIds = new List<string>();
        connection.ConnectionStateChanged += (_, args) =>
        {
            if (args.State == BleConnectionState.Connected)
            {
                publishedActiveProfileIds.Add(session.ActiveProfileId);
            }
        };

        await connection.ConnectAsync(new DiscoveredMaskDevice("device-b", "Mask B", -45));
        transport.RaiseConnected();
        await connection.ProfileUpdateTask.WaitAsync(TimeSpan.FromSeconds(5));

        var expectedProfileId = MaskProfileSession.DeriveProfileId("device-b");
        Assert.Equal(expectedProfileId, session.ActiveProfileId);
        Assert.Equal([expectedProfileId], publishedActiveProfileIds);
    }

    [Fact]
    public async Task NativeStyleDisconnectDuringDeferredActivation_LeavesPreviousProfileActive()
    {
        var profileStore = new BlockingMaskProfileStore();
        var session = new MaskProfileSession(profileStore);
        var transport = new FakeCombinedTransport();
        using var connection = new ProfiledBleDeviceConnection(
            transport,
            session,
            transport,
            transport,
            transport,
            transport);

        await connection.ConnectAsync(new DiscoveredMaskDevice("device-a", "Mask A", -40));
        var firstProfileId = session.ActiveProfileId;
        var publishedConnectedCount = 0;
        connection.ConnectionStateChanged += (_, args) =>
        {
            if (args.State == BleConnectionState.Connected)
            {
                publishedConnectedCount++;
            }
        };
        transport.CompleteConnectSynchronously = false;
        profileStore.BlockNextSave();

        await connection.ConnectAsync(new DiscoveredMaskDevice("device-b", "Mask B", -45));
        transport.RaiseConnected();
        await profileStore.SaveStarted.WaitAsync(TimeSpan.FromSeconds(5));
        transport.RaiseDisconnected();
        profileStore.ReleaseSave();
        await connection.ProfileUpdateTask.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(firstProfileId, session.ActiveProfileId);
        Assert.Equal(firstProfileId, (await session.GetActiveProfileAsync())!.ProfileId);
        Assert.Equal(0, publishedConnectedCount);
        Assert.DoesNotContain(
            (await profileStore.LoadAsync()).Profiles,
            profile => string.Equals(profile.DisplayName, "Mask B", StringComparison.Ordinal));
    }

    private sealed class BlockingMaskProfileStore : IMaskProfileStore
    {
        private readonly object sync = new();
        private readonly InMemoryMaskProfileStore inner = new();
        private TaskCompletionSource saveStarted = CreateSignal();
        private TaskCompletionSource releaseSave = CreateSignal();
        private bool blockNextSave;

        public Task SaveStarted => saveStarted.Task;

        public Task<MaskProfileStoreState> LoadAsync(CancellationToken cancellationToken = default) =>
            inner.LoadAsync(cancellationToken);

        public async Task SaveAsync(
            MaskProfileStoreState state,
            CancellationToken cancellationToken = default)
        {
            Task releaseTask;
            lock (sync)
            {
                if (!blockNextSave)
                {
                    releaseTask = Task.CompletedTask;
                }
                else
                {
                    blockNextSave = false;
                    saveStarted.TrySetResult();
                    releaseTask = releaseSave.Task;
                }
            }

            await releaseTask.WaitAsync(cancellationToken);
            await inner.SaveAsync(state, cancellationToken);
        }

        public void BlockNextSave()
        {
            lock (sync)
            {
                blockNextSave = true;
                saveStarted = CreateSignal();
                releaseSave = CreateSignal();
            }
        }

        public void ReleaseSave()
        {
            lock (sync)
            {
                releaseSave.TrySetResult();
            }
        }

        private static TaskCompletionSource CreateSignal() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class FakeCombinedTransport :
        IBleDeviceConnection,
        IMaskCommandTransport,
        ITextUploadTransport,
        IFaceUploadTransport,
        IAudioVisualizationTransport
    {
        public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;

        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        event EventHandler<TextUploadTransportStateChangedEventArgs>? ITextUploadTransport.StateChanged
        {
            add => TextStateChanged += value;
            remove => TextStateChanged -= value;
        }

        event EventHandler<FaceUploadTransportStateChangedEventArgs>? IFaceUploadTransport.StateChanged
        {
            add => FaceStateChanged += value;
            remove => FaceStateChanged -= value;
        }

        event EventHandler<AudioVisualizationTransportStateChangedEventArgs>? IAudioVisualizationTransport.StateChanged
        {
            add => AudioStateChanged += value;
            remove => AudioStateChanged -= value;
        }

        private event EventHandler<TextUploadTransportStateChangedEventArgs>? TextStateChanged;

        private event EventHandler<FaceUploadTransportStateChangedEventArgs>? FaceStateChanged;

        private event EventHandler<AudioVisualizationTransportStateChangedEventArgs>? AudioStateChanged;

        public BleConnectionState State { get; private set; } = BleConnectionState.Disconnected;

        public Exception? ConnectException { get; set; }

        public bool RaiseReadyDuringConnect { get; set; }

        public bool CompleteConnectSynchronously { get; set; } = true;

        public string TransportDisplayName => "Fake combined BLE";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; private set; } =
            MaskCommandTransportState.Disconnected;

        public string TransportStatusText => IsReady ? "Ready." : "Discovering.";

        public bool IsReady { get; private set; }

        public bool SupportsAcknowledgements => true;

        TextUploadTransportState ITextUploadTransport.State => IsReady
            ? TextUploadTransportState.Ready
            : TextUploadTransportState.Disconnected;

        FaceUploadTransportState IFaceUploadTransport.State => IsReady
            ? FaceUploadTransportState.Ready
            : FaceUploadTransportState.Disconnected;

        AudioVisualizationTransportState IAudioVisualizationTransport.State => IsReady
            ? AudioVisualizationTransportState.Ready
            : AudioVisualizationTransportState.Disconnected;

        public string StatusText => TransportStatusText;

        public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
        {
            if (ConnectException is not null)
            {
                throw ConnectException;
            }

            IsReady = false;
            TransportState = MaskCommandTransportState.Disconnected;
            State = BleConnectionState.Connecting;
            ConnectionStateChanged?.Invoke(
                this,
                new BleConnectionStateChangedEventArgs(State, "Connecting."));
            if (CompleteConnectSynchronously)
            {
                RaiseConnected();
            }

            return Task.CompletedTask;
        }

        public void RaiseConnected()
        {
            State = BleConnectionState.Connected;
            ConnectionStateChanged?.Invoke(
                this,
                new BleConnectionStateChangedEventArgs(State, "Connected; discovering."));
            if (RaiseReadyDuringConnect)
            {
                RaiseReady();
            }
        }

        public void RaiseConnectionFailure(string message)
        {
            State = BleConnectionState.Failed;
            ConnectionStateChanged?.Invoke(
                this,
                new BleConnectionStateChangedEventArgs(State, message));
        }

        public void RaiseDisconnected()
        {
            State = BleConnectionState.Disconnected;
            ConnectionStateChanged?.Invoke(
                this,
                new BleConnectionStateChangedEventArgs(State, "Disconnected."));
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            State = BleConnectionState.Disconnected;
            IsReady = false;
            TransportState = MaskCommandTransportState.Disconnected;
            ConnectionStateChanged?.Invoke(
                this,
                new BleConnectionStateChangedEventArgs(State, "Disconnected."));
            return Task.CompletedTask;
        }

        public Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Sent."));

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));

        public Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(FaceUploadResult.Success("Uploaded.", package.Frames.Count));

        public Task<AudioVisualizationSendResult> SendAsync(
            AudioVisualizationPacket packet,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AudioVisualizationSendResult.Success("Sent."));

        public void RaiseReady()
        {
            IsReady = true;
            TransportState = MaskCommandTransportState.Ready;
            TransportStateChanged?.Invoke(
                this,
                new MaskCommandTransportStateChangedEventArgs(TransportState, "Ready."));
            TextStateChanged?.Invoke(
                this,
                new TextUploadTransportStateChangedEventArgs(
                    TextUploadTransportState.Ready,
                    "Ready.",
                    SupportsAcknowledgements,
                    IsReady));
            FaceStateChanged?.Invoke(
                this,
                new FaceUploadTransportStateChangedEventArgs(
                    FaceUploadTransportState.Ready,
                    "Ready.",
                    SupportsAcknowledgements,
                    IsReady));
            AudioStateChanged?.Invoke(
                this,
                new AudioVisualizationTransportStateChangedEventArgs(
                    AudioVisualizationTransportState.Ready,
                    "Ready.",
                    IsReady));
        }
    }
}
