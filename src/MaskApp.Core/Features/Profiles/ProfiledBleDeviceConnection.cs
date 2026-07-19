using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;
using System.Runtime.ExceptionServices;

namespace MaskApp.Core.Features.Profiles;

public sealed class ProfiledBleDeviceConnection : IBleDeviceConnection, IDisposable
{
    private readonly object sync = new();
    private readonly IBleDeviceConnection inner;
    private readonly MaskProfileSession profileSession;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly IFaceUploadTransport faceTransport;
    private readonly IAudioVisualizationTransport? audioVisualizationTransport;
    private Task profileUpdateTask = Task.CompletedTask;
    private DiscoveredMaskDevice? pendingProfileDevice;
    private Task<Exception?>? pendingProfileActivationTask;
    private CancellationTokenSource? pendingProfileActivationCancellation;
    private int connectionAttempt;
    private bool profileActivationPending;
    private bool disposed;

    public ProfiledBleDeviceConnection(
        IBleDeviceConnection inner,
        MaskProfileSession profileSession,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IFaceUploadTransport faceTransport,
        IAudioVisualizationTransport? audioVisualizationTransport = null)
    {
        this.inner = inner;
        this.profileSession = profileSession;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        this.faceTransport = faceTransport;
        this.audioVisualizationTransport = audioVisualizationTransport;
        inner.ConnectionStateChanged += HandleConnectionStateChanged;
        commandTransport.TransportStateChanged += HandleCommandTransportStateChanged;
        textTransport.StateChanged += HandleTextTransportStateChanged;
        faceTransport.StateChanged += HandleFaceTransportStateChanged;
        if (audioVisualizationTransport is not null)
        {
            audioVisualizationTransport.StateChanged += HandleAudioVisualizationTransportStateChanged;
        }
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
        int attempt;
        CancellationTokenSource? previousActivation;
        lock (sync)
        {
            previousActivation = ClearPendingProfileActivationLocked();
            attempt = ++connectionAttempt;
            pendingProfileDevice = device;
            profileActivationPending = true;
        }
        CancelProfileActivation(previousActivation);

        try
        {
            await inner.ConnectAsync(device, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            CancelProfileActivation(ClearPendingProfileActivation(attempt));
            throw;
        }

        Task<Exception?>? activationTask;
        lock (sync)
        {
            if (attempt == connectionAttempt && inner.State == BleConnectionState.Connected)
            {
                activationTask = EnsurePendingProfileActivationLocked(
                    attempt,
                    device,
                    new BleConnectionStateChangedEventArgs(
                        BleConnectionState.Connected,
                        $"Connected to {device.Name}."));
            }
            else
            {
                activationTask = null;
            }
        }

        if (activationTask is not null)
        {
            var activationException = await activationTask.ConfigureAwait(false);
            if (activationException is not null)
            {
                ExceptionDispatchInfo.Capture(activationException).Throw();
            }
        }
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        CancellationTokenSource? pendingActivation;
        lock (sync)
        {
            ++connectionAttempt;
            pendingActivation = ClearPendingProfileActivationLocked();
        }
        CancelProfileActivation(pendingActivation);

        return inner.DisconnectAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        CancellationTokenSource? pendingActivation;
        lock (sync)
        {
            ++connectionAttempt;
            pendingActivation = ClearPendingProfileActivationLocked();
        }
        CancelProfileActivation(pendingActivation);
        inner.ConnectionStateChanged -= HandleConnectionStateChanged;
        commandTransport.TransportStateChanged -= HandleCommandTransportStateChanged;
        textTransport.StateChanged -= HandleTextTransportStateChanged;
        faceTransport.StateChanged -= HandleFaceTransportStateChanged;
        if (audioVisualizationTransport is not null)
        {
            audioVisualizationTransport.StateChanged -= HandleAudioVisualizationTransportStateChanged;
        }
    }

    private void HandleConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs args)
    {
        if (args.State == BleConnectionState.Connected)
        {
            lock (sync)
            {
                if (pendingProfileDevice is { } device)
                {
                    _ = EnsurePendingProfileActivationLocked(
                        connectionAttempt,
                        device,
                        args);
                    return;
                }
            }
        }
        else if (args.State is BleConnectionState.Failed or BleConnectionState.Disconnected)
        {
            CancellationTokenSource? pendingActivation;
            lock (sync)
            {
                ++connectionAttempt;
                pendingActivation = ClearPendingProfileActivationLocked();
            }
            CancelProfileActivation(pendingActivation);
        }

        ConnectionStateChanged?.Invoke(this, args);
    }

    private Task<Exception?> EnsurePendingProfileActivationLocked(
        int attempt,
        DiscoveredMaskDevice device,
        BleConnectionStateChangedEventArgs connectedArgs)
    {
        if (pendingProfileActivationTask is not null)
        {
            return pendingProfileActivationTask;
        }

        pendingProfileActivationCancellation = new CancellationTokenSource();
        pendingProfileActivationTask = ActivateConnectedProfileAsync(
            attempt,
            device,
            connectedArgs,
            pendingProfileActivationCancellation);
        profileUpdateTask = pendingProfileActivationTask;
        return pendingProfileActivationTask;
    }

    private async Task<Exception?> ActivateConnectedProfileAsync(
        int attempt,
        DiscoveredMaskDevice device,
        BleConnectionStateChangedEventArgs connectedArgs,
        CancellationTokenSource activationCancellation)
    {
        try
        {
            await Task.Yield();
            lock (sync)
            {
                if (attempt != connectionAttempt
                    || !profileActivationPending
                    || !ReferenceEquals(pendingProfileActivationCancellation, activationCancellation)
                    || inner.State != BleConnectionState.Connected)
                {
                    return null;
                }
            }

            try
            {
                await profileSession
                    .ActivateAsync(device, activationCancellation.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (activationCancellation.IsCancellationRequested)
            {
                return null;
            }
            catch (Exception profileException)
            {
                await DisconnectAfterProfileActivationFailureAsync(profileException).ConfigureAwait(false);
                return profileException;
            }

            var publishConnected = false;
            lock (sync)
            {
                if (attempt == connectionAttempt
                    && ReferenceEquals(pendingProfileActivationCancellation, activationCancellation))
                {
                    pendingProfileDevice = null;
                    pendingProfileActivationTask = null;
                    pendingProfileActivationCancellation = null;
                    profileActivationPending = false;
                    publishConnected = inner.State == BleConnectionState.Connected;
                }
            }

            if (publishConnected)
            {
                LastProfileError = string.Empty;
                ConnectionStateChanged?.Invoke(this, connectedArgs);
                if (HasObservableTransportState())
                {
                    QueueProfileUpdate();
                }
            }

            return null;
        }
        finally
        {
            lock (sync)
            {
                if (attempt == connectionAttempt
                    && ReferenceEquals(pendingProfileActivationCancellation, activationCancellation))
                {
                    pendingProfileDevice = null;
                    pendingProfileActivationTask = null;
                    pendingProfileActivationCancellation = null;
                    profileActivationPending = false;
                }
            }
            activationCancellation.Dispose();
        }
    }

    private CancellationTokenSource? ClearPendingProfileActivation(int attempt)
    {
        lock (sync)
        {
            if (attempt != connectionAttempt)
            {
                return null;
            }

            return ClearPendingProfileActivationLocked();
        }
    }

    private CancellationTokenSource? ClearPendingProfileActivationLocked()
    {
        var pendingActivation = pendingProfileActivationCancellation;
        pendingProfileDevice = null;
        pendingProfileActivationTask = null;
        pendingProfileActivationCancellation = null;
        profileActivationPending = false;
        return pendingActivation;
    }

    private static void CancelProfileActivation(CancellationTokenSource? pendingActivation)
    {
        try
        {
            pendingActivation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
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

    private void HandleAudioVisualizationTransportStateChanged(
        object? sender,
        AudioVisualizationTransportStateChangedEventArgs args)
    {
        if (inner.State == BleConnectionState.Connected
            && args.State is AudioVisualizationTransportState.Ready
                or AudioVisualizationTransportState.Unsupported
                or AudioVisualizationTransportState.Failed)
        {
            QueueProfileUpdate();
        }
    }

    private void QueueProfileUpdate()
    {
        lock (sync)
        {
            var profileId = profileSession.ActiveProfileId;
            if (profileActivationPending
                || inner.State != BleConnectionState.Connected
                || string.IsNullOrWhiteSpace(profileId))
            {
                return;
            }

            profileUpdateTask = ObserveAfterAsync(profileUpdateTask, profileId);
        }
    }

    private async Task ObserveAfterAsync(Task precedingUpdate, string expectedProfileId)
    {
        await precedingUpdate.ConfigureAwait(false);
        await ObserveConnectedCapabilitiesAsync(expectedProfileId).ConfigureAwait(false);
    }

    private async Task ObserveConnectedCapabilitiesAsync(string expectedProfileId)
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
            await profileSession.ObserveCapabilitiesForProfileAsync(
                expectedProfileId,
                new MaskCapabilitySnapshot
                {
                    CommandWriteAvailable = commandTransport.TransportState == MaskCommandTransportState.Ready,
                    TextUploadAvailable = textReady,
                    FaceUploadAvailable = faceReady,
                    AudioVisualizationWriteAvailable = audioVisualizationTransport?.IsReady == true,
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

    private async Task DisconnectAfterProfileActivationFailureAsync(Exception profileException)
    {
        try
        {
            await inner.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
            LastProfileError = profileException.Message;
        }
        catch (Exception disconnectException) when (
            disconnectException is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            LastProfileError =
                $"{profileException.Message} The mask also could not be disconnected: {disconnectException.Message}";
        }
    }

    private bool HasObservableTransportState() =>
        commandTransport.TransportState is MaskCommandTransportState.Ready or MaskCommandTransportState.Failed
        || textTransport.IsReady
        || faceTransport.IsReady
        || audioVisualizationTransport?.State is AudioVisualizationTransportState.Ready
            or AudioVisualizationTransportState.Unsupported
            or AudioVisualizationTransportState.Failed;
}
