namespace MaskApp.Core.Features.WatchRemote;

public interface IWatchRemoteActionDispatcher
{
    Task<WatchRemoteDispatchResult> DispatchAsync(
        WatchRemoteAction action,
        CancellationToken cancellationToken = default);
}

public interface IWatchRemoteStateProvider
{
    Task<WatchRemoteState> GetStateAsync(CancellationToken cancellationToken = default);
}

public sealed class WatchRemoteActionProcessor
{
    private const int MaximumRememberedMessages = 128;
    private const int MaximumRememberedSenders = 16;
    private static readonly TimeSpan MaximumMessageAge = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan MaximumFutureClockSkew = TimeSpan.FromSeconds(5);
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly IWatchRemoteActionDispatcher dispatcher;
    private readonly IWatchRemoteStateProvider stateProvider;
    private readonly Func<DateTimeOffset> getUtcNow;
    private readonly Queue<string> processedMessageOrder = new();
    private readonly HashSet<string> processedMessages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> lastSequences = new(StringComparer.Ordinal);
    private readonly Queue<string> senderOrder = new();

    public WatchRemoteActionProcessor(
        IWatchRemoteActionDispatcher dispatcher,
        IWatchRemoteStateProvider stateProvider,
        Func<DateTimeOffset>? getUtcNow = null)
    {
        this.dispatcher = dispatcher;
        this.stateProvider = stateProvider;
        this.getUtcNow = getUtcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<WatchRemoteProcessResult> ProcessAsync(
        WatchRemoteEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        (WatchRemoteProcessStatus Status, string Message)? earlyResult = null;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var validation = Validate(envelope, getUtcNow());
            if (validation is not null)
            {
                earlyResult = validation;
            }
            else
            {
                var messageKey = CreateMessageKey(envelope);
                if (processedMessages.Contains(messageKey)
                    || lastSequences.TryGetValue(envelope.SenderInstanceId, out var priorSequence)
                        && envelope.Sequence <= priorSequence)
                {
                    earlyResult = (
                        WatchRemoteProcessStatus.Duplicate,
                        "Duplicate or out-of-order Watch action ignored; no hardware output was replayed.");
                }
                else
                {
                    // Mark accepted input before dispatch so a retry cannot replay partial hardware output.
                    Remember(envelope, messageKey);
                }
            }
        }
        finally
        {
            gate.Release();
        }

        if (earlyResult is not null)
        {
            return await CreateResultAsync(
                envelope,
                earlyResult.Value.Status,
                succeeded: false,
                earlyResult.Value.Message,
                earlyResult.Value.Status is WatchRemoteProcessStatus.Stale or WatchRemoteProcessStatus.Duplicate
                    ? WatchRemoteHaptic.Warning
                    : WatchRemoteHaptic.Failure,
                cancellationToken).ConfigureAwait(false);
        }

        WatchRemoteDispatchResult dispatchResult;
        try
        {
            dispatchResult = await dispatcher.DispatchAsync(envelope.Action!, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            dispatchResult = WatchRemoteDispatchResult.Failure(
                "Watch action was cancelled before completion.");
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException
                or InvalidOperationException or ArgumentException or ObjectDisposedException
                or TimeoutException)
        {
            dispatchResult = WatchRemoteDispatchResult.Failure(exception.Message);
        }

        return await CreateResultAsync(
            envelope,
            dispatchResult.Succeeded
                ? WatchRemoteProcessStatus.Accepted
                : WatchRemoteProcessStatus.Failed,
            dispatchResult.Succeeded,
            dispatchResult.Message,
            dispatchResult.Succeeded ? WatchRemoteHaptic.Success : WatchRemoteHaptic.Failure,
            cancellationToken).ConfigureAwait(false);
    }

    private static (WatchRemoteProcessStatus Status, string Message)? Validate(
        WatchRemoteEnvelope envelope,
        DateTimeOffset now)
    {
        if (envelope.SchemaVersion != WatchRemoteEnvelope.CurrentSchemaVersion)
        {
            return (WatchRemoteProcessStatus.Rejected, "Unsupported Watch action schema version.");
        }

        if (envelope.MessageId == Guid.Empty)
        {
            return (WatchRemoteProcessStatus.Rejected, "Watch action requires a non-empty message ID.");
        }

        if (string.IsNullOrWhiteSpace(envelope.SenderInstanceId)
            || envelope.SenderInstanceId.Length > 64
            || !string.Equals(
                envelope.SenderInstanceId,
                envelope.SenderInstanceId.Trim(),
                StringComparison.Ordinal))
        {
            return (WatchRemoteProcessStatus.Rejected, "Watch action sender identity is missing or too long.");
        }

        if (envelope.Sequence <= 0)
        {
            return (WatchRemoteProcessStatus.Rejected, "Watch action sequence must be positive.");
        }

        if (envelope.SentAt == default
            || now - envelope.SentAt > MaximumMessageAge
            || envelope.SentAt - now > MaximumFutureClockSkew)
        {
            return (WatchRemoteProcessStatus.Stale, "Stale or future-dated Watch action ignored.");
        }

        if (envelope.Action is null)
        {
            return (WatchRemoteProcessStatus.Rejected, "Watch action payload is missing.");
        }

        if (envelope.Action.Kind == WatchRemoteActionKind.Unknown
            || !Enum.IsDefined(envelope.Action.Kind))
        {
            return (WatchRemoteProcessStatus.Rejected, "Watch action kind is unsupported.");
        }

        return envelope.Action.Kind switch
        {
            WatchRemoteActionKind.SetBrightness when envelope.Action.Brightness is < 1 or > 100 or null =>
                (WatchRemoteProcessStatus.Rejected, "Digital Crown brightness must be between 1 and 100."),
            WatchRemoteActionKind.TriggerFavorite when string.IsNullOrWhiteSpace(envelope.Action.FavoriteId)
                || envelope.Action.FavoriteId.Trim().Length > 128 =>
                (WatchRemoteProcessStatus.Rejected, "Favorite action requires a bounded favorite ID."),
            WatchRemoteActionKind.SetBrightness or WatchRemoteActionKind.TriggerFavorite => null,
            _ when envelope.Action.Brightness is not null || !string.IsNullOrWhiteSpace(envelope.Action.FavoriteId) =>
                (WatchRemoteProcessStatus.Rejected, "Watch action contains fields that do not apply to its kind."),
            _ => null
        };
    }

    private async Task<WatchRemoteProcessResult> CreateResultAsync(
        WatchRemoteEnvelope envelope,
        WatchRemoteProcessStatus status,
        bool succeeded,
        string message,
        WatchRemoteHaptic haptic,
        CancellationToken cancellationToken)
    {
        WatchRemoteState state;
        try
        {
            state = await stateProvider.GetStateAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is OperationCanceledException or IOException or UnauthorizedAccessException
                or InvalidOperationException or ArgumentException or ObjectDisposedException
                or TimeoutException)
        {
            state = new WatchRemoteState
            {
                GeneratedAt = getUtcNow(),
                ReadinessSummary = $"Phone state unavailable: {exception.Message}"
            };
        }

        return new WatchRemoteProcessResult
        {
            MessageId = envelope.MessageId,
            Sequence = envelope.Sequence,
            Status = status,
            Succeeded = succeeded,
            Message = Bound(message, 300),
            Haptic = haptic,
            State = state
        };
    }

    private void Remember(WatchRemoteEnvelope envelope, string messageKey)
    {
        processedMessages.Add(messageKey);
        processedMessageOrder.Enqueue(messageKey);
        if (!lastSequences.ContainsKey(envelope.SenderInstanceId))
        {
            senderOrder.Enqueue(envelope.SenderInstanceId);
        }

        lastSequences[envelope.SenderInstanceId] = envelope.Sequence;
        while (processedMessageOrder.Count > MaximumRememberedMessages)
        {
            processedMessages.Remove(processedMessageOrder.Dequeue());
        }

        while (senderOrder.Count > MaximumRememberedSenders)
        {
            lastSequences.Remove(senderOrder.Dequeue());
        }
    }

    private static string CreateMessageKey(WatchRemoteEnvelope envelope) =>
        $"{envelope.SenderInstanceId.Trim()}:{envelope.MessageId:D}";

    private static string Bound(string? value, int maximumLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "Watch action failed." : value.Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }
}
