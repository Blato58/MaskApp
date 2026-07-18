using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.Profiles;

public sealed class ProfiledBleDeviceConnection : IBleDeviceConnection, IDisposable
{
    private readonly object sync = new();
    private readonly IBleDeviceConnection inner;
    private readonly MaskProfileSession profileSession;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly IFaceUploadTransport faceTransport;
    private Task profileUpdateTask = Task.CompletedTask;
    private bool disposed;

    public ProfiledBleDeviceConnection(
        IBleDeviceConnection inner,
        MaskProfileSession profileSession,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IFaceUploadTransport faceTransport)
    {
        this.inner = inner;
        this.profileSession = profileSession;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        this.faceTransport = faceTransport;
        inner.ConnectionStateChanged += HandleConnectionStateChanged;
        commandTransport.TransportStateChanged += HandleCommandTransportStateChanged;
        textTransport.StateChanged += HandleTextTransportStateChanged;
        faceTransport.StateChanged += HandleFaceTransportStateChanged;
    }

    public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public BleConnectionState State => inner.State;

    public string LastProfileError { get; private set; } = string.Empty;

    public Task ProfileUpdateTask
    {
        get
        {
            lock (sync)
            {
                return profileUpdateTask;
            }
        }
    }

    public async Task ConnectAsync(
        DiscoveredMaskDevice device,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        await profileSession.ActivateAsync(device, cancellationToken).ConfigureAwait(false);
        await inner.ConnectAsync(device, cancellationToken).ConfigureAwait(false);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return inner.DisconnectAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        inner.ConnectionStateChanged -= HandleConnectionStateChanged;
        commandTransport.TransportStateChanged -= HandleCommandTransportStateChanged;
        textTransport.StateChanged -= HandleTextTransportStateChanged;
        faceTransport.StateChanged -= HandleFaceTransportStateChanged;
    }

    private void HandleConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs args)
    {
        ConnectionStateChanged?.Invoke(this, args);
    }

    private void HandleCommandTransportStateChanged(
        object? sender,
        MaskCommandTransportStateChangedEventArgs args)
    {
        if (inner.State == BleConnectionState.Connected
            && args.State is MaskCommandTransportState.Ready or MaskCommandTransportState.Failed)
        {
            QueueProfileUpdate();
        }
    }

    private void HandleTextTransportStateChanged(
        object? sender,
        TextUploadTransportStateChangedEventArgs args)
    {
        if (inner.State == BleConnectionState.Connected && args.IsReady)
        {
            QueueProfileUpdate();
        }
    }

    private void HandleFaceTransportStateChanged(
        object? sender,
        FaceUploadTransportStateChangedEventArgs args)
    {
        if (inner.State == BleConnectionState.Connected && args.IsReady)
        {
            QueueProfileUpdate();
        }
    }

    private void QueueProfileUpdate()
    {
        lock (sync)
        {
            profileUpdateTask = ObserveAfterAsync(profileUpdateTask);
        }
    }

    private async Task ObserveAfterAsync(Task precedingUpdate)
    {
        await precedingUpdate.ConfigureAwait(false);
        await ObserveConnectedCapabilitiesAsync().ConfigureAwait(false);
    }

    private async Task ObserveConnectedCapabilitiesAsync()
    {
        try
        {
            var textReady = textTransport.IsReady;
            var faceReady = faceTransport.IsReady;
            var acknowledgementMode = !textReady && !faceReady
                ? MaskAcknowledgementMode.Unknown
                : textTransport.SupportsAcknowledgements && faceTransport.SupportsAcknowledgements
                    ? MaskAcknowledgementMode.Acknowledged
                    : MaskAcknowledgementMode.WriteOnly;
            await profileSession.ObserveCapabilitiesAsync(new MaskCapabilitySnapshot
            {
                CommandWriteAvailable = commandTransport.TransportState == MaskCommandTransportState.Ready,
                TextUploadAvailable = textReady,
                FaceUploadAvailable = faceReady,
                AcknowledgementMode = acknowledgementMode,
                DiySlotCapacity = FacePattern.MaxSlot,
                TransportName = commandTransport.TransportDisplayName,
                ObservedAt = DateTimeOffset.UtcNow
            }).ConfigureAwait(false);
            LastProfileError = string.Empty;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            LastProfileError = exception.Message;
        }
    }
}
