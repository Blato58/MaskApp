using System.Diagnostics;
using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.MaskControl;

public sealed class MaskBleScheduler :
    IMaskCommandTransport,
    ITextUploadTransport,
    IFaceUploadTransport,
    IMaskEmergencyControl,
    IAudioVisualizationTransport,
    IVisualWorkCancellationSource,
    IDisposable,
    IAsyncDisposable
{
    private const int EmergencyPriority = -100;
    private const int ControlPriority = 0;
    private const int UploadPriority = 10;
    private const int AudioPriority = 20;

    private readonly object sync = new();
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textUploadTransport;
    private readonly IFaceUploadTransport faceUploadTransport;
    private readonly IAudioVisualizationTransport? audioVisualizationTransport;
    private readonly IBleDeviceConnection? connection;
    private readonly MaskBleSchedulerOptions options;
    private readonly PriorityQueue<ScheduledOperation, (int Priority, long Sequence)> queue = new();
    private readonly Dictionary<string, ScheduledOperation> supersessionQueue = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim queueSignal = new(0);
    private readonly CancellationTokenSource shutdownCancellation = new();
    private CancellationTokenSource connectionCancellation = new();
    private readonly Task worker;
    private ScheduledOperation? activeOperation;
    private long connectionGeneration;
    private long sequence;
    private long totalEnqueued;
    private long totalCompleted;
    private long totalSuperseded;
    private long totalRejected;
    private long totalEmergencyCancellations;
    private TimeSpan? lastOperationDuration;
    private string? lastCompletedOperationName;
    private bool? lastOperationSucceeded;
    private string? lastError;
    private int pendingOperationCount;
    private bool disposed;

    public MaskBleScheduler(
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textUploadTransport,
        IFaceUploadTransport faceUploadTransport,
        IBleDeviceConnection? connection = null,
        MaskBleSchedulerOptions? options = null,
        IAudioVisualizationTransport? audioVisualizationTransport = null)
    {
        this.commandTransport = commandTransport ?? throw new ArgumentNullException(nameof(commandTransport));
        this.textUploadTransport = textUploadTransport ?? throw new ArgumentNullException(nameof(textUploadTransport));
        this.faceUploadTransport = faceUploadTransport ?? throw new ArgumentNullException(nameof(faceUploadTransport));
        this.audioVisualizationTransport = audioVisualizationTransport;
        this.connection = connection;
        this.options = options ?? new MaskBleSchedulerOptions();
        if (this.options.MaxPendingOperations <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                this.options.MaxPendingOperations,
                "The scheduler queue limit must be greater than zero.");
        }

        if (connection is not null)
        {
            connection.ConnectionStateChanged += HandleConnectionStateChanged;
        }

        worker = Task.Run(ProcessQueueAsync);
    }

    public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
    {
        add => commandTransport.TransportStateChanged += value;
        remove => commandTransport.TransportStateChanged -= value;
    }

    event EventHandler<TextUploadTransportStateChangedEventArgs>? ITextUploadTransport.StateChanged
    {
        add => textUploadTransport.StateChanged += value;
        remove => textUploadTransport.StateChanged -= value;
    }

    event EventHandler<FaceUploadTransportStateChangedEventArgs>? IFaceUploadTransport.StateChanged
    {
        add => faceUploadTransport.StateChanged += value;
        remove => faceUploadTransport.StateChanged -= value;
    }

    public event EventHandler<MaskBleSchedulerDiagnosticsChangedEventArgs>? DiagnosticsChanged;

    public event EventHandler<VisualWorkCancelledEventArgs>? VisualWorkCancelled;

    event EventHandler<AudioVisualizationTransportStateChangedEventArgs>? IAudioVisualizationTransport.StateChanged
    {
        add
        {
            if (audioVisualizationTransport is not null)
            {
                audioVisualizationTransport.StateChanged += value;
            }
        }
        remove
        {
            if (audioVisualizationTransport is not null)
            {
                audioVisualizationTransport.StateChanged -= value;
            }
        }
    }

    public string TransportDisplayName => commandTransport.TransportDisplayName;

    public bool IsSimulated => commandTransport.IsSimulated;

    public MaskCommandTransportState TransportState => commandTransport.TransportState;

    public string TransportStatusText => commandTransport.TransportStatusText;

    bool ITextUploadTransport.IsReady => textUploadTransport.IsReady;

    bool IFaceUploadTransport.IsReady => faceUploadTransport.IsReady;

    bool ITextUploadTransport.SupportsAcknowledgements => textUploadTransport.SupportsAcknowledgements;

    bool IFaceUploadTransport.SupportsAcknowledgements => faceUploadTransport.SupportsAcknowledgements;

    TextUploadTransportState ITextUploadTransport.State => textUploadTransport.State;

    FaceUploadTransportState IFaceUploadTransport.State => faceUploadTransport.State;

    string ITextUploadTransport.StatusText => textUploadTransport.StatusText;

    string IFaceUploadTransport.StatusText => faceUploadTransport.StatusText;

    bool IAudioVisualizationTransport.IsReady => audioVisualizationTransport?.IsReady == true;

    bool IAudioVisualizationTransport.IsSimulated => audioVisualizationTransport?.IsSimulated == true;

    AudioVisualizationTransportState IAudioVisualizationTransport.State =>
        audioVisualizationTransport?.State ?? AudioVisualizationTransportState.Unsupported;

    string IAudioVisualizationTransport.StatusText =>
        audioVisualizationTransport?.StatusText ?? "Audio visualization transport is not configured.";

    public MaskBleSchedulerSnapshot GetSnapshot()
    {
        lock (sync)
        {
            return CreateSnapshot();
        }
    }

    public Task<MaskCommandResult> SendAsync(
        MaskCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var isBlackout = IsBlackout(command);
        return Enqueue(
            isBlackout ? "BLACKOUT" : $"Send {command.DisplayName}",
            isBlackout ? EmergencyPriority : ControlPriority,
            GetSupersessionKey(command),
            isBlackout,
            options.CommandTimeout,
            token => commandTransport.SendAsync(command, token),
            MaskCommandResult.Failure,
            result => result.Succeeded,
            result => result.Message,
            cancellationToken);
    }

    public Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cancelledCount = CancelVisualWork("Cancelled by Stop.");
        PublishVisualWorkCancelled(VisualWorkCancellationReason.Stop, "Live visual work stopped by the user.");
        PublishDiagnostics();
        return Task.FromResult(MaskCommandResult.Success(
            cancelledCount == 0
                ? "No queued mask output was active."
                : $"Stopped {cancelledCount} active or queued mask operation(s)."));
    }

    public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default) =>
        SendAsync(MaskCommandBuilder.Brightness(1), cancellationToken);

    public Task<AudioVisualizationSendResult> SendAsync(
        AudioVisualizationPacket packet,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packet);
        if (audioVisualizationTransport is null)
        {
            return Task.FromResult(AudioVisualizationSendResult.Failure(
                "Audio visualization transport is not configured."));
        }

        return Enqueue(
            $"Audio frame ({packet.PackingMode}, {packet.Framing})",
            AudioPriority,
            "audio:frame",
            isEmergency: false,
            options.AudioVisualizationTimeout,
            token => audioVisualizationTransport.SendAsync(packet, token),
            AudioVisualizationSendResult.Failure,
            result => result.Succeeded,
            result => result.Message,
            cancellationToken);
    }

    public Task<TextUploadResult> UploadAsync(
        TextUploadPackage package,
        TextUploadOptions uploadOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(uploadOptions);

        return Enqueue(
            $"Upload text ({package.Frames.Count} frame(s))",
            UploadPriority,
            supersessionKey: null,
            isEmergency: false,
            options.TextUploadTimeout,
            token => UploadTextAndObserveQuietPeriodAsync(package, uploadOptions, token),
            message => TextUploadResult.Failure(message, 0),
            result => result.Succeeded,
            result => result.Message,
            cancellationToken);
    }

    public Task<FaceUploadResult> UploadAsync(
        FaceUploadPackage package,
        FaceUploadOptions uploadOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(uploadOptions);

        return Enqueue(
            $"Upload DIY slot {package.Slot} ({package.Frames.Count} frame(s))",
            UploadPriority,
            supersessionKey: null,
            isEmergency: false,
            options.FaceUploadTimeout,
            token => UploadFaceAndObserveQuietPeriodAsync(package, uploadOptions, token),
            message => FaceUploadResult.Failure(message, 0),
            result => result.Succeeded,
            result => result.Message,
            cancellationToken);
    }

    public void Dispose()
    {
        ScheduledOperation[] invalidatedOperations;
        ScheduledOperation? operationInProgress;
        CancellationTokenSource priorConnectionCancellation;

        lock (sync)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            if (connection is not null)
            {
                connection.ConnectionStateChanged -= HandleConnectionStateChanged;
            }

            invalidatedOperations = queue.UnorderedItems.Select(item => item.Element).ToArray();
            queue.Clear();
            DrainQueueSignalsUnderLock(invalidatedOperations.Length);
            supersessionQueue.Clear();
            foreach (var operation in invalidatedOperations)
            {
                ReleasePendingCount(operation);
            }

            operationInProgress = activeOperation;
            priorConnectionCancellation = connectionCancellation;
            connectionCancellation = new CancellationTokenSource();
        }

        foreach (var operation in invalidatedOperations)
        {
            operation.TryFail("The mask command scheduler stopped.");
            operation.ReleaseResources();
        }

        operationInProgress?.TryFail("The mask command scheduler stopped.");
        priorConnectionCancellation.Cancel();
        priorConnectionCancellation.Dispose();
        connectionCancellation.Cancel();
        connectionCancellation.Dispose();
        shutdownCancellation.Cancel();
        queueSignal.Release();
        PublishVisualWorkCancelled(
            VisualWorkCancellationReason.SchedulerStopped,
            "Live visual work stopped because the scheduler was disposed.");
        PublishDiagnostics();
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();

        try
        {
            await worker.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        shutdownCancellation.Dispose();
        queueSignal.Dispose();
    }

    private async Task<TextUploadResult> UploadTextAndObserveQuietPeriodAsync(
        TextUploadPackage package,
        TextUploadOptions uploadOptions,
        CancellationToken cancellationToken)
    {
        var result = await textUploadTransport
            .UploadAsync(package, uploadOptions, cancellationToken)
            .ConfigureAwait(false);

        if (uploadOptions.PostUploadQuietPeriod > TimeSpan.Zero && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(uploadOptions.PostUploadQuietPeriod, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private async Task<FaceUploadResult> UploadFaceAndObserveQuietPeriodAsync(
        FaceUploadPackage package,
        FaceUploadOptions uploadOptions,
        CancellationToken cancellationToken)
    {
        var result = await faceUploadTransport
            .UploadAsync(package, uploadOptions, cancellationToken)
            .ConfigureAwait(false);

        if (uploadOptions.PostUploadQuietPeriod > TimeSpan.Zero && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(uploadOptions.PostUploadQuietPeriod, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private Task<T> Enqueue<T>(
        string name,
        int priority,
        string? supersessionKey,
        bool isEmergency,
        TimeSpan timeout,
        Func<CancellationToken, Task<T>> action,
        Func<string, T> failureFactory,
        Func<T, bool> succeeded,
        Func<T, string> resultMessage,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ScheduledOperation<T> operation;
        ScheduledOperation? activeOperationToCancel = null;
        var shouldSignalQueue = true;
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            operation = new ScheduledOperation<T>(
                name,
                connectionGeneration,
                supersessionKey,
                isEmergency,
                timeout,
                action,
                failureFactory,
                succeeded,
                resultMessage,
                cancellationToken);

            if (isEmergency)
            {
                (_, activeOperationToCancel) = MarkVisualWorkCancelledUnderLock($"Cancelled by {name}.");
            }

            if (supersessionKey is not null
                && supersessionQueue.TryGetValue(supersessionKey, out var priorOperation)
                && priorOperation.TrySupersede(name))
            {
                ReleasePendingCount(priorOperation);
                if (queue.Remove(priorOperation, out _, out _))
                {
                    priorOperation.ReleaseResources();
                    shouldSignalQueue = false;
                }

                totalSuperseded++;
            }

            if (supersessionKey is not null)
            {
                supersessionQueue[supersessionKey] = operation;
            }

            if (!isEmergency && pendingOperationCount >= options.MaxPendingOperations)
            {
                supersessionQueue.Remove(supersessionKey ?? string.Empty);
                totalRejected++;
                lastError = $"{name} was rejected because the mask command queue is full.";
                operation.TryFail(lastError);
                operation.ReleaseResources();
                return operation.Completion;
            }

            queue.Enqueue(operation, (priority, sequence++));
            pendingOperationCount++;
            totalEnqueued++;
        }

        activeOperationToCancel?.CancelExecution();
        if (isEmergency)
        {
            PublishVisualWorkCancelled(
                VisualWorkCancellationReason.Blackout,
                "Live visual work stopped by emergency Blackout.");
        }
        if (shouldSignalQueue)
        {
            queueSignal.Release();
        }

        PublishDiagnostics();
        return operation.Completion;
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            while (true)
            {
                await queueSignal.WaitAsync(shutdownCancellation.Token).ConfigureAwait(false);

                ScheduledOperation? operation = null;
                CancellationToken connectionToken = default;
                long operationGeneration = 0;

                lock (sync)
                {
                    while (queue.Count > 0)
                    {
                        var candidate = queue.Dequeue();
                        if (candidate.SupersessionKey is not null
                            && supersessionQueue.TryGetValue(candidate.SupersessionKey, out var current)
                            && ReferenceEquals(candidate, current))
                        {
                            supersessionQueue.Remove(candidate.SupersessionKey);
                        }

                        ReleasePendingCount(candidate);
                        if (candidate.IsCompleted)
                        {
                            candidate.ReleaseResources();
                            continue;
                        }

                        operation = candidate;
                        activeOperation = candidate;
                        connectionToken = connectionCancellation.Token;
                        operationGeneration = connectionGeneration;
                        break;
                    }
                }

                if (operation is null)
                {
                    PublishDiagnostics();
                    continue;
                }

                PublishDiagnostics();
                var startedAt = Stopwatch.GetTimestamp();
                await operation
                    .RunAsync(operationGeneration, connectionToken, shutdownCancellation.Token)
                    .ConfigureAwait(false);
                var duration = Stopwatch.GetElapsedTime(startedAt);

                lock (sync)
                {
                    if (ReferenceEquals(activeOperation, operation))
                    {
                        activeOperation = null;
                    }

                    totalCompleted++;
                    lastOperationDuration = duration;
                    lastCompletedOperationName = operation.Name;
                    lastOperationSucceeded = operation.FailureMessage is null;
                    if (operation.FailureMessage is not null)
                    {
                        lastError = operation.FailureMessage;
                    }
                }

                PublishDiagnostics();
            }
        }
        catch (OperationCanceledException) when (shutdownCancellation.IsCancellationRequested)
        {
        }
    }

    private void HandleConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs args)
    {
        ScheduledOperation[] invalidatedOperations;
        ScheduledOperation? operationInProgress;
        CancellationTokenSource priorConnectionCancellation;

        lock (sync)
        {
            if (disposed)
            {
                return;
            }

            connectionGeneration++;
            invalidatedOperations = queue.UnorderedItems.Select(item => item.Element).ToArray();
            queue.Clear();
            DrainQueueSignalsUnderLock(invalidatedOperations.Length);
            supersessionQueue.Clear();
            foreach (var operation in invalidatedOperations)
            {
                ReleasePendingCount(operation);
            }

            operationInProgress = activeOperation;
            priorConnectionCancellation = connectionCancellation;
            connectionCancellation = new CancellationTokenSource();
        }

        var message = $"Mask connection changed to {args.State}; queued work was discarded.";
        foreach (var operation in invalidatedOperations)
        {
            operation.TryFail(message);
            operation.ReleaseResources();
        }

        operationInProgress?.TryFail(message);
        priorConnectionCancellation.Cancel();
        priorConnectionCancellation.Dispose();
        PublishVisualWorkCancelled(
            VisualWorkCancellationReason.ConnectionChanged,
            message);
        PublishDiagnostics();
    }

    private void ReleasePendingCount(ScheduledOperation operation)
    {
        if (operation.TryReleasePendingCount())
        {
            pendingOperationCount--;
        }
    }

    private MaskBleSchedulerSnapshot CreateSnapshot() =>
        new(
            connectionGeneration,
            pendingOperationCount,
            activeOperation?.Name,
            totalEnqueued,
            totalCompleted,
            totalSuperseded,
            totalRejected,
            totalEmergencyCancellations,
            lastOperationDuration,
            lastError,
            queue.Count,
            queueSignal.CurrentCount,
            lastCompletedOperationName,
            lastOperationSucceeded);

    private void PublishDiagnostics()
    {
        MaskBleSchedulerSnapshot snapshot;
        lock (sync)
        {
            snapshot = CreateSnapshot();
        }

        DiagnosticsChanged?.Invoke(this, new MaskBleSchedulerDiagnosticsChangedEventArgs(snapshot));
    }

    private void PublishVisualWorkCancelled(
        VisualWorkCancellationReason reason,
        string message) =>
        VisualWorkCancelled?.Invoke(this, new VisualWorkCancelledEventArgs(reason, message));

    private static string? GetSupersessionKey(MaskCommand command) =>
        command.Kind switch
        {
            MaskCommandKind.Brightness when IsBlackout(command) => "emergency:blackout",
            MaskCommandKind.Brightness => "control:brightness",
            MaskCommandKind.AnimationSpeed => "control:animation-speed",
            MaskCommandKind.FacePlay => "playback:face-frame",
            _ => null
        };

    private int CancelVisualWork(string reason)
    {
        int cancelledCount;
        ScheduledOperation? activeOperationToCancel;
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            (cancelledCount, activeOperationToCancel) = MarkVisualWorkCancelledUnderLock(reason);
        }

        activeOperationToCancel?.CancelExecution();
        return cancelledCount;
    }

    private (int CancelledCount, ScheduledOperation? ActiveOperation) MarkVisualWorkCancelledUnderLock(
        string reason)
    {
        var cancelledCount = 0;
        if (activeOperation is { IsEmergency: false } active
            && active.TryFail(reason))
        {
            cancelledCount++;
        }
        else
        {
            active = null;
        }

        var queuedVisualOperations = queue.UnorderedItems
            .Select(item => item.Element)
            .Where(operation => !operation.IsEmergency)
            .ToArray();
        var removedCount = 0;
        foreach (var operation in queuedVisualOperations)
        {
            if (operation.TryFail(reason))
            {
                cancelledCount++;
            }

            if (!queue.Remove(operation, out _, out _))
            {
                continue;
            }

            if (operation.SupersessionKey is not null
                && supersessionQueue.TryGetValue(operation.SupersessionKey, out var current)
                && ReferenceEquals(operation, current))
            {
                supersessionQueue.Remove(operation.SupersessionKey);
            }

            ReleasePendingCount(operation);
            operation.ReleaseResources();
            removedCount++;
        }

        DrainQueueSignalsUnderLock(removedCount);

        totalEmergencyCancellations += cancelledCount;
        return (cancelledCount, active);
    }

    private void DrainQueueSignalsUnderLock(int maximumCount)
    {
        for (var index = 0; index < maximumCount && queueSignal.Wait(0); index++)
        {
        }
    }

    private static bool IsBlackout(MaskCommand command) =>
        command.Kind == MaskCommandKind.Brightness
        && command.Plaintext.Span[6] <= 1;

    private abstract class ScheduledOperation
    {
        private int pendingCounted = 1;

        protected ScheduledOperation(
            string name,
            long generation,
            string? supersessionKey,
            bool isEmergency)
        {
            Name = name;
            Generation = generation;
            SupersessionKey = supersessionKey;
            IsEmergency = isEmergency;
        }

        public string Name { get; }

        public long Generation { get; }

        public string? SupersessionKey { get; }

        public bool IsEmergency { get; }

        public abstract string? FailureMessage { get; }

        public abstract bool IsCompleted { get; }

        public bool TryReleasePendingCount() => Interlocked.Exchange(ref pendingCounted, 0) == 1;

        public abstract bool TrySupersede(string replacementName);

        public abstract bool TryFail(string message);

        public abstract void CancelExecution();

        public abstract Task RunAsync(
            long currentGeneration,
            CancellationToken connectionToken,
            CancellationToken shutdownToken);

        public abstract void ReleaseResources();
    }

    private sealed class ScheduledOperation<T> : ScheduledOperation
    {
        private readonly TimeSpan timeout;
        private readonly Func<CancellationToken, Task<T>> action;
        private readonly Func<string, T> failureFactory;
        private readonly Func<T, bool> succeeded;
        private readonly Func<T, string> resultMessage;
        private readonly CancellationToken callerToken;
        private readonly CancellationTokenSource emergencyCancellation = new();
        private readonly TaskCompletionSource<T> completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenRegistration callerCancellationRegistration;
        private string? failureMessage;
        private int resourcesReleased;

        public ScheduledOperation(
            string name,
            long generation,
            string? supersessionKey,
            bool isEmergency,
            TimeSpan timeout,
            Func<CancellationToken, Task<T>> action,
            Func<string, T> failureFactory,
            Func<T, bool> succeeded,
            Func<T, string> resultMessage,
            CancellationToken callerToken)
            : base(name, generation, supersessionKey, isEmergency)
        {
            this.timeout = timeout;
            this.action = action;
            this.failureFactory = failureFactory;
            this.succeeded = succeeded;
            this.resultMessage = resultMessage;
            this.callerToken = callerToken;
            callerCancellationRegistration = callerToken.Register(
                static state => ((ScheduledOperation<T>)state!).completion.TrySetCanceled(),
                this);
        }

        public Task<T> Completion => completion.Task;

        public override bool IsCompleted => completion.Task.IsCompleted;

        public override string? FailureMessage => failureMessage;

        public override bool TrySupersede(string replacementName) =>
            TryFail($"Superseded by {replacementName}.");

        public override bool TryFail(string message)
        {
            if (!completion.TrySetResult(failureFactory(message)))
            {
                return false;
            }

            failureMessage = message;
            return true;
        }

        public override void CancelExecution() => emergencyCancellation.Cancel();

        public override async Task RunAsync(
            long currentGeneration,
            CancellationToken connectionToken,
            CancellationToken shutdownToken)
        {
            if (Generation != currentGeneration)
            {
                TryFail("The mask connection changed before this operation could start.");
                ReleaseResources();
                return;
            }

            using var timeoutCancellation = new CancellationTokenSource();
            if (timeout != Timeout.InfiniteTimeSpan)
            {
                timeoutCancellation.CancelAfter(timeout);
            }

            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                callerToken,
                connectionToken,
                shutdownToken,
                emergencyCancellation.Token,
                timeoutCancellation.Token);

            try
            {
                var result = await action(linkedCancellation.Token).ConfigureAwait(false);
                if (connectionToken.IsCancellationRequested)
                {
                    TryFail("The mask connection changed while this operation was running.");
                }
                else if (shutdownToken.IsCancellationRequested)
                {
                    TryFail("The mask command scheduler stopped.");
                }
                else if (timeoutCancellation.IsCancellationRequested)
                {
                    TryFail($"{Name} timed out after {timeout}.");
                }
                else
                {
                    if (!succeeded(result))
                    {
                        failureMessage = resultMessage(result);
                    }

                    completion.TrySetResult(result);
                }
            }
            catch (OperationCanceledException) when (callerToken.IsCancellationRequested)
            {
                completion.TrySetCanceled(callerToken);
            }
            catch (OperationCanceledException) when (connectionToken.IsCancellationRequested)
            {
                TryFail("The mask connection changed while this operation was running.");
            }
            catch (OperationCanceledException) when (shutdownToken.IsCancellationRequested)
            {
                TryFail("The mask command scheduler stopped.");
            }
            catch (OperationCanceledException) when (emergencyCancellation.IsCancellationRequested)
            {
                TryFail("The operation was cancelled by an emergency mask action.");
            }
            catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
            {
                TryFail($"{Name} timed out after {timeout}.");
            }
            catch (Exception exception)
            {
                TryFail($"{Name} failed: {exception.Message}");
            }
            finally
            {
                ReleaseResources();
            }
        }

        public override void ReleaseResources()
        {
            if (Interlocked.Exchange(ref resourcesReleased, 1) == 0)
            {
                callerCancellationRegistration.Dispose();
                emergencyCancellation.Dispose();
            }
        }
    }
}
