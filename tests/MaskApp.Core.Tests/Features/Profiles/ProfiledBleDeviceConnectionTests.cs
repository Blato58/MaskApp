using MaskApp.Core.Features.Connect;
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

    private sealed class FakeCombinedTransport :
        IBleDeviceConnection,
        IMaskCommandTransport,
        ITextUploadTransport,
        IFaceUploadTransport
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

        private event EventHandler<TextUploadTransportStateChangedEventArgs>? TextStateChanged;

        private event EventHandler<FaceUploadTransportStateChangedEventArgs>? FaceStateChanged;

        public BleConnectionState State { get; private set; } = BleConnectionState.Disconnected;

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

        public string StatusText => TransportStatusText;

        public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
        {
            IsReady = false;
            TransportState = MaskCommandTransportState.Disconnected;
            State = BleConnectionState.Connecting;
            ConnectionStateChanged?.Invoke(
                this,
                new BleConnectionStateChangedEventArgs(State, "Connecting."));
            State = BleConnectionState.Connected;
            ConnectionStateChanged?.Invoke(
                this,
                new BleConnectionStateChangedEventArgs(State, "Connected; discovering."));
            return Task.CompletedTask;
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
        }
    }
}
