#if IOS
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Foundation;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.WatchRemote;
using WatchConnectivity;

namespace MaskApp.App.Infrastructure.WatchRemote;

public sealed class IosWatchConnectivityService : WCSessionDelegate, IWatchConnectivityService
{
    private static readonly NSString StateJsonKey = new("stateJson");
    private readonly WatchRemoteActionProcessor processor;
    private readonly IWatchRemoteStateProvider stateProvider;
    private readonly WatchRemoteMessageCodec codec;
    private readonly IBleDeviceConnection connection;
    private readonly object snapshotSync = new();
    private readonly SemaphoreSlim publishGate = new(1, 1);
    private WatchConnectivitySnapshot snapshot = new()
    {
        Availability = WatchConnectivityAvailability.Activating,
        IsSupported = true,
        StatusText = "Watch Connectivity has not started yet."
    };
    private WCSession? session;
    private int started;

    public IosWatchConnectivityService(
        WatchRemoteActionProcessor processor,
        IWatchRemoteStateProvider stateProvider,
        WatchRemoteMessageCodec codec,
        IBleDeviceConnection connection)
    {
        this.processor = processor;
        this.stateProvider = stateProvider;
        this.codec = codec;
        this.connection = connection;
    }

    public event EventHandler<WatchConnectivityChangedEventArgs>? StateChanged;

    public WatchConnectivitySnapshot Snapshot
    {
        get
        {
            lock (snapshotSync)
            {
                return snapshot;
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.Exchange(ref started, 1) != 0)
        {
            return Task.CompletedTask;
        }

        connection.ConnectionStateChanged += OnConnectionStateChanged;
        if (!WCSession.IsSupported)
        {
            SetSnapshot(new WatchConnectivitySnapshot
            {
                Availability = WatchConnectivityAvailability.Unsupported,
                StatusText = "This iPhone does not support Watch Connectivity."
            });
            return Task.CompletedTask;
        }

        session = WCSession.DefaultSession;
        session.Delegate = this;
        SetSnapshot(new WatchConnectivitySnapshot
        {
            Availability = WatchConnectivityAvailability.Activating,
            IsSupported = true,
            StatusText = "Activating Watch Connectivity."
        });
        session.ActivateSession();
        return Task.CompletedTask;
    }

    public async Task PublishStateAsync(
        WatchRemoteState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        await publishGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var activeSession = session;
            if (activeSession is null
                || activeSession.ActivationState != WCSessionActivationState.Activated
                || !activeSession.Paired
                || !activeSession.WatchAppInstalled)
            {
                return;
            }

            var stateBytes = codec.EncodeState(Decorate(state));
            using var json = new NSString(Encoding.UTF8.GetString(stateBytes));
            using var context = new NSDictionary<NSString, NSObject>(StateJsonKey, json);
            if (!activeSession.UpdateApplicationContext(context, out var error))
            {
                throw new IOException(error?.LocalizedDescription
                    ?? "Watch state could not be published.");
            }
        }
        finally
        {
            publishGate.Release();
        }
    }

    public override void ActivationDidComplete(
        WCSession session,
        WCSessionActivationState activationState,
        NSError? error)
    {
        if (error is not null)
        {
            SetSnapshot(new WatchConnectivitySnapshot
            {
                Availability = WatchConnectivityAvailability.Failed,
                IsSupported = true,
                StatusText = $"Watch Connectivity activation failed: {error.LocalizedDescription}"
            });
            return;
        }

        UpdateSnapshot(session);
        _ = PublishCurrentStateAsync();
    }

    public override void SessionReachabilityDidChange(WCSession session)
    {
        UpdateSnapshot(session);
        _ = PublishCurrentStateAsync();
    }

    public override void DidBecomeInactive(WCSession session)
    {
        SetSnapshot(CreateSnapshot(
            session,
            WatchConnectivityAvailability.Unreachable,
            "Watch Connectivity became inactive."));
    }

    public override void DidDeactivate(WCSession session)
    {
        SetSnapshot(CreateSnapshot(
            session,
            WatchConnectivityAvailability.Activating,
            "Reactivating Watch Connectivity after a companion change."));
        session.ActivateSession();
    }

    public override void DidReceiveMessageData(WCSession session, NSData messageData)
    {
        try
        {
            _ = ProcessWithoutReplyAsync(CopyBounded(messageData));
        }
        catch (Exception exception) when (IsExpectedMessageException(exception))
        {
            System.Diagnostics.Debug.WriteLine($"Watch action processing failed: {exception}");
        }
    }

    public override void DidReceiveMessageData(
        WCSession session,
        NSData messageData,
        WCSessionReplyDataHandler replyHandler)
    {
        byte[]? payload = null;
        string? copyError = null;
        try
        {
            payload = CopyBounded(messageData);
        }
        catch (Exception exception) when (IsExpectedMessageException(exception))
        {
            copyError = exception.Message;
        }

        _ = ProcessWithReplyAsync(payload, copyError, replyHandler);
    }

    private async Task ProcessWithoutReplyAsync(byte[] payload)
    {
        try
        {
            var result = await ProcessAsync(payload).ConfigureAwait(false);
            await PublishStateAsync(result.State).ConfigureAwait(false);
        }
        catch (Exception exception) when (IsExpectedMessageException(exception))
        {
            System.Diagnostics.Debug.WriteLine($"Watch action processing failed: {exception}");
        }
    }

    private async Task ProcessWithReplyAsync(
        byte[]? payload,
        string? copyError,
        WCSessionReplyDataHandler replyHandler)
    {
        WatchRemoteProcessResult result;
        try
        {
            result = payload is null
                ? await CreateRejectedResultAsync(copyError ?? "Malformed Watch action rejected.")
                    .ConfigureAwait(false)
                : await ProcessAsync(payload).ConfigureAwait(false);
        }
        catch (Exception exception) when (IsExpectedMessageException(exception))
        {
            result = await CreateRejectedResultAsync(exception.Message).ConfigureAwait(false);
        }

        using var replyData = NSData.FromArray(codec.EncodeResult(result));
        replyHandler(replyData);
        try
        {
            await PublishStateAsync(result.State).ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is IOException or InvalidOperationException or ObjectDisposedException)
        {
            System.Diagnostics.Debug.WriteLine($"Watch state publication failed: {exception}");
        }
    }

    private async Task<WatchRemoteProcessResult> ProcessAsync(byte[] payload)
    {
        var envelope = codec.DecodeEnvelope(payload);
        var result = await processor.ProcessAsync(envelope).ConfigureAwait(false);
        return result with { State = Decorate(result.State) };
    }

    private async Task<WatchRemoteProcessResult> CreateRejectedResultAsync(string message)
    {
        WatchRemoteState state;
        try
        {
            state = Decorate(await stateProvider.GetStateAsync().ConfigureAwait(false));
        }
        catch (Exception exception) when (IsExpectedMessageException(exception))
        {
            state = Decorate(new WatchRemoteState
            {
                GeneratedAt = DateTimeOffset.UtcNow,
                ReadinessSummary = $"Phone state unavailable: {exception.Message}"
            });
        }

        return new WatchRemoteProcessResult
        {
            Status = WatchRemoteProcessStatus.Rejected,
            Message = Bound(message, 300, "Malformed Watch action rejected."),
            Haptic = WatchRemoteHaptic.Failure,
            State = state
        };
    }

    private async Task PublishCurrentStateAsync()
    {
        try
        {
            await PublishStateAsync(await stateProvider.GetStateAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is IOException or InvalidDataException or InvalidOperationException
                or ArgumentException or ObjectDisposedException)
        {
            System.Diagnostics.Debug.WriteLine($"Watch state refresh failed: {exception}");
        }
    }

    private void OnConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs args) =>
        _ = PublishCurrentStateAsync();

    private void UpdateSnapshot(WCSession session)
    {
        if (session.ActivationState != WCSessionActivationState.Activated)
        {
            SetSnapshot(CreateSnapshot(
                session,
                WatchConnectivityAvailability.Activating,
                "Watch Connectivity is activating."));
        }
        else if (!session.Paired)
        {
            SetSnapshot(CreateSnapshot(
                session,
                WatchConnectivityAvailability.Unpaired,
                "No Apple Watch is paired with this iPhone."));
        }
        else if (!session.WatchAppInstalled)
        {
            SetSnapshot(CreateSnapshot(
                session,
                WatchConnectivityAvailability.CompanionNotInstalled,
                "The MaskApp watchOS companion is not installed."));
        }
        else if (session.Reachable)
        {
            SetSnapshot(CreateSnapshot(
                session,
                WatchConnectivityAvailability.Ready,
                "Apple Watch is reachable."));
        }
        else
        {
            SetSnapshot(CreateSnapshot(
                session,
                WatchConnectivityAvailability.Unreachable,
                "Apple Watch is paired but not currently reachable."));
        }
    }

    private static WatchConnectivitySnapshot CreateSnapshot(
        WCSession session,
        WatchConnectivityAvailability availability,
        string statusText) => new()
        {
            Availability = availability,
            IsSupported = true,
            IsPaired = session.Paired,
            IsCompanionInstalled = session.WatchAppInstalled,
            IsReachable = session.Reachable,
            StatusText = statusText
        };

    private WatchRemoteState Decorate(WatchRemoteState state)
    {
        var current = Snapshot;
        return state with
        {
            WatchReachable = current.IsReachable,
            CompanionStatus = Bound(current.StatusText, 200, "Watch companion state unavailable.")
        };
    }

    private void SetSnapshot(WatchConnectivitySnapshot value)
    {
        lock (snapshotSync)
        {
            snapshot = value;
        }

        StateChanged?.Invoke(this, new WatchConnectivityChangedEventArgs(value));
    }

    private static byte[] CopyBounded(NSData data)
    {
        if (data.Length == 0 || data.Length > WatchRemoteMessageCodec.MaximumMessageBytes)
        {
            throw new InvalidDataException(
                $"Watch action must contain 1 to {WatchRemoteMessageCodec.MaximumMessageBytes} bytes.");
        }

        var bytes = new byte[checked((int)data.Length)];
        Marshal.Copy(data.Bytes, bytes, 0, bytes.Length);
        return bytes;
    }

    private static string Bound(string? value, int maximumLength, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }

    private static bool IsExpectedMessageException(Exception exception) =>
        exception is OperationCanceledException or IOException or UnauthorizedAccessException or InvalidDataException
            or InvalidOperationException or ArgumentException or JsonException
            or ObjectDisposedException or OverflowException or TimeoutException;
}
#endif
