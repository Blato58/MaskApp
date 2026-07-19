using System.Diagnostics;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Profiles;

namespace MaskApp.Core.Features.Audio;

public enum AudioVisualizerEngineState
{
    Stopped,
    Calibrating,
    Starting,
    Running,
    Stopping,
    Blocked,
    Failed
}

public sealed class AudioVisualizerEngineStateChangedEventArgs(
    AudioVisualizerEngineState state,
    string statusText,
    int framesSent,
    int framesSuppressed) : EventArgs
{
    public AudioVisualizerEngineState State { get; } = state;

    public string StatusText { get; } = statusText;

    public int FramesSent { get; } = framesSent;

    public int FramesSuppressed { get; } = framesSuppressed;
}

public sealed record AudioVisualizerOperationResult(bool Succeeded, string Message)
{
    public static AudioVisualizerOperationResult Success(string message) => new(true, message);

    public static AudioVisualizerOperationResult Failure(string message) => new(false, message);
}

public sealed class AudioVisualizerEngine : IAsyncDisposable
{
    private const int CalibrationBlockTarget = 12;
    private static readonly TimeSpan CalibrationTimeout = TimeSpan.FromSeconds(3);
    private readonly object stateLock = new();
    private readonly SemaphoreSlim lifecycleGate = new(1, 1);
    private readonly SemaphoreSlim sampleSignal = new(0, 1);
    private readonly IAudioCaptureService captureService;
    private readonly IAudioVisualizationTransport transport;
    private readonly MaskProfileSession profileSession;
    private readonly AudioVisualizerProcessor processor;
    private readonly AudioFlashSafetyGate flashSafetyGate;
    private readonly IVisualWorkCancellationSource? cancellationSource;
    private AudioSamplesAvailableEventArgs? latestSamples;
    private CancellationTokenSource? runCancellation;
    private CancellationTokenSource? calibrationCancellation;
    private Task? processingTask;
    private AudioVisualizerSettings settings = new();
    private AudioVisualizerEngineState state;
    private string statusText = "Audio Labs is stopped.";
    private int framesSent;
    private int framesSuppressed;
    private long calibrationStopGeneration;
    private bool disposed;

    public AudioVisualizerEngine(
        IAudioCaptureService captureService,
        IAudioVisualizationTransport transport,
        MaskProfileSession profileSession,
        AudioVisualizerProcessor? processor = null,
        AudioFlashSafetyGate? flashSafetyGate = null,
        IVisualWorkCancellationSource? cancellationSource = null)
    {
        this.captureService = captureService;
        this.transport = transport;
        this.profileSession = profileSession;
        this.processor = processor ?? new AudioVisualizerProcessor();
        this.flashSafetyGate = flashSafetyGate ?? new AudioFlashSafetyGate();
        this.cancellationSource = cancellationSource;
        captureService.SamplesAvailable += HandleSamplesAvailable;
        captureService.StateChanged += HandleCaptureStateChanged;
        transport.StateChanged += HandleTransportStateChanged;
        if (cancellationSource is not null)
        {
            cancellationSource.VisualWorkCancelled += HandleVisualWorkCancelled;
        }
    }

    public event EventHandler<AudioVisualizerEngineStateChangedEventArgs>? StateChanged;

    public AudioVisualizerEngineState State
    {
        get
        {
            lock (stateLock)
            {
                return state;
            }
        }
    }

    public string StatusText
    {
        get
        {
            lock (stateLock)
            {
                return statusText;
            }
        }
    }

    public int FramesSent => Volatile.Read(ref framesSent);

    public int FramesSuppressed => Volatile.Read(ref framesSuppressed);

    public double NoiseFloor => processor.NoiseFloor;

    public AudioVisualizerSettings Settings
    {
        get
        {
            lock (stateLock)
            {
                return settings;
            }
        }
    }

    public void UpdateSettings(AudioVisualizerSettings newSettings)
    {
        ArgumentNullException.ThrowIfNull(newSettings);
        lock (stateLock)
        {
            settings = newSettings.Normalize();
        }
    }

    public async Task<AudioVisualizerOperationResult> StartAsync(
        CancellationToken cancellationToken = default)
    {
        await lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (processingTask is not null)
            {
                return AudioVisualizerOperationResult.Failure("Audio visualization is already running.");
            }

            var profile = await profileSession.GetActiveProfileAsync(cancellationToken).ConfigureAwait(false);
            if (profile is null)
            {
                SetState(AudioVisualizerEngineState.Blocked, "Connect to a mask before starting microphone input.");
                return AudioVisualizerOperationResult.Failure(StatusText);
            }

            var evidence = profile.AudioVisualizationEvidence.Normalize();
            if (!evidence.EnablesLiveMicrophone)
            {
                SetState(
                    AudioVisualizerEngineState.Blocked,
                    "Run the finite deterministic test and physically confirm this mask before enabling microphone input.");
                return AudioVisualizerOperationResult.Failure(StatusText);
            }

            if (!transport.IsReady)
            {
                SetState(AudioVisualizerEngineState.Blocked, transport.StatusText);
                return AudioVisualizerOperationResult.Failure(StatusText);
            }

            SetState(AudioVisualizerEngineState.Starting, "Starting foreground-only microphone capture...");
            processor.Reset();
            flashSafetyGate.Reset();
            Interlocked.Exchange(ref framesSent, 0);
            Interlocked.Exchange(ref framesSuppressed, 0);
            var activeRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            lock (stateLock)
            {
                runCancellation = activeRun;
            }
            processingTask = ProcessSamplesAsync(evidence, activeRun.Token);

            AudioCaptureStartResult captureResult;
            try
            {
                captureResult = await captureService.StartAsync(activeRun.Token).ConfigureAwait(false);
                activeRun.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                await StopUnderLockAsync("Audio Labs start was cancelled.").ConfigureAwait(false);
                throw;
            }

            if (!captureResult.Succeeded)
            {
                await StopUnderLockAsync(captureResult.Message, AudioVisualizerEngineState.Blocked).ConfigureAwait(false);
                return AudioVisualizerOperationResult.Failure(captureResult.Message);
            }

            SetState(
                AudioVisualizerEngineState.Running,
                $"Experimental {Settings.Mode} is running in the foreground at up to {GetCadenceHz(evidence):0.#} Hz.");
            return AudioVisualizerOperationResult.Success(StatusText);
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    public async Task<AudioVisualizerOperationResult> CalibrateAsync(
        CancellationToken cancellationToken = default)
    {
        var stopGeneration = Volatile.Read(ref calibrationStopGeneration);
        await lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        CancellationTokenSource? calibrationRun = null;
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (processingTask is not null)
            {
                return AudioVisualizerOperationResult.Failure("Stop live audio before calibrating the room noise floor.");
            }

            calibrationRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            lock (stateLock)
            {
                calibrationCancellation = calibrationRun;
            }
            if (stopGeneration != Volatile.Read(ref calibrationStopGeneration))
            {
                calibrationRun.Cancel();
            }

            SetState(AudioVisualizerEngineState.Calibrating, "Calibrating room noise. Keep the environment quiet...");
            var blocks = new List<float[]>();
            var enoughSamples = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void CollectSamples(object? sender, AudioSamplesAvailableEventArgs args)
            {
                lock (blocks)
                {
                    if (blocks.Count >= CalibrationBlockTarget)
                    {
                        return;
                    }

                    blocks.Add(args.Samples.ToArray());
                    if (blocks.Count >= CalibrationBlockTarget)
                    {
                        enoughSamples.TrySetResult();
                    }
                }
            }

            captureService.SamplesAvailable += CollectSamples;
            try
            {
                try
                {
                    var captureResult = await captureService.StartAsync(calibrationRun.Token).ConfigureAwait(false);
                    if (!captureResult.Succeeded)
                    {
                        SetState(AudioVisualizerEngineState.Blocked, captureResult.Message);
                        return AudioVisualizerOperationResult.Failure(captureResult.Message);
                    }

                    using var timeout = new CancellationTokenSource(CalibrationTimeout);
                    using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                        calibrationRun.Token,
                        timeout.Token);
                    try
                    {
                        await enoughSamples.Task.WaitAsync(linked.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (
                        timeout.IsCancellationRequested && !calibrationRun.IsCancellationRequested)
                    {
                    }
                }
                catch (OperationCanceledException) when (
                    calibrationRun.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    SetState(AudioVisualizerEngineState.Stopped, "Noise calibration stopped. It will not resume automatically.");
                    return AudioVisualizerOperationResult.Failure(StatusText);
                }
            }
            finally
            {
                captureService.SamplesAvailable -= CollectSamples;
                await captureService.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            float[][] snapshot;
            lock (blocks)
            {
                snapshot = blocks.ToArray();
            }

            if (snapshot.Length == 0)
            {
                SetState(AudioVisualizerEngineState.Failed, "Calibration received no microphone samples.");
                return AudioVisualizerOperationResult.Failure(StatusText);
            }

            var floor = processor.Calibrate(snapshot);
            SetState(AudioVisualizerEngineState.Stopped, $"Noise calibration complete (floor {floor:0.000}).");
            return AudioVisualizerOperationResult.Success(StatusText);
        }
        finally
        {
            lock (stateLock)
            {
                if (ReferenceEquals(calibrationCancellation, calibrationRun))
                {
                    calibrationCancellation = null;
                }
            }
            calibrationRun?.Dispose();
            lifecycleGate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        CancelActiveRun();
        CancelCalibration();
        await lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (disposed || processingTask is null)
            {
                if (!disposed)
                {
                    SetState(AudioVisualizerEngineState.Stopped, "Audio Labs is stopped.");
                }
                return;
            }

            await StopUnderLockAsync("Audio Labs stopped. It will not resume automatically.").ConfigureAwait(false);
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        CancelActiveRun();
        CancelCalibration();
        await lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposed)
            {
                return;
            }

            if (processingTask is not null)
            {
                await StopUnderLockAsync("Audio Labs disposed.").ConfigureAwait(false);
            }

            disposed = true;
            captureService.SamplesAvailable -= HandleSamplesAvailable;
            captureService.StateChanged -= HandleCaptureStateChanged;
            transport.StateChanged -= HandleTransportStateChanged;
            if (cancellationSource is not null)
            {
                cancellationSource.VisualWorkCancelled -= HandleVisualWorkCancelled;
            }
        }
        finally
        {
            lifecycleGate.Release();
            lifecycleGate.Dispose();
            sampleSignal.Dispose();
        }
    }

    private async Task ProcessSamplesAsync(
        AudioVisualizationEvidence evidence,
        CancellationToken cancellationToken)
    {
        var cadence = GetCadenceHz(evidence);
        var minimumInterval = TimeSpan.FromSeconds(1 / cadence);
        var lastWriteAt = 0L;
        var runStartedAt = Stopwatch.GetTimestamp();
        try
        {
            while (true)
            {
                await sampleSignal.WaitAsync(cancellationToken).ConfigureAwait(false);
                var samples = Interlocked.Exchange(ref latestSamples, null);
                if (samples is null)
                {
                    continue;
                }

                if (lastWriteAt != 0)
                {
                    var elapsed = Stopwatch.GetElapsedTime(lastWriteAt);
                    var remaining = minimumInterval - elapsed;
                    if (remaining > TimeSpan.Zero)
                    {
                        await Task.Delay(remaining, cancellationToken).ConfigureAwait(false);
                    }
                }

                AudioVisualizerSettings currentSettings;
                lock (stateLock)
                {
                    currentSettings = settings;
                }

                var monotonicTimestamp = DateTimeOffset.UnixEpoch + Stopwatch.GetElapsedTime(runStartedAt);
                var frame = processor.Process(
                    samples.Samples,
                    samples.SampleRate,
                    currentSettings,
                    monotonicTimestamp);
                var safety = flashSafetyGate.Evaluate(frame.Levels, monotonicTimestamp);
                if (!safety.CanSend)
                {
                    Interlocked.Increment(ref framesSuppressed);
                    PublishState(safety.Message);
                    continue;
                }

                var packet = BuildPacket(frame.Levels, evidence);
                var result = await transport.SendAsync(packet, cancellationToken).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    SetState(AudioVisualizerEngineState.Failed, result.Message);
                    _ = StopAfterExternalSignalAsync(result.Message);
                    return;
                }

                lastWriteAt = Stopwatch.GetTimestamp();
                Interlocked.Increment(ref framesSent);
                PublishState(
                    frame.DropTriggered
                        ? "Experimental drop pulse sent inside the two-per-second refractory limit."
                        : $"Experimental {currentSettings.Mode} active; latest RMS {frame.RootMeanSquare:0.000}.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            SetState(AudioVisualizerEngineState.Failed, $"Audio processing failed: {exception.Message}");
            _ = StopAfterExternalSignalAsync(StatusText);
        }
    }

    private async Task StopAfterExternalSignalAsync(string reason)
    {
        await lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!disposed && processingTask is not null)
            {
                await StopUnderLockAsync(reason, AudioVisualizerEngineState.Stopped)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    private async Task StopUnderLockAsync(
        string message,
        AudioVisualizerEngineState finalState = AudioVisualizerEngineState.Stopped)
    {
        SetState(AudioVisualizerEngineState.Stopping, "Stopping microphone capture and queued audio frames...");
        CancellationTokenSource? cancellation;
        lock (stateLock)
        {
            cancellation = runCancellation;
            runCancellation = null;
        }
        var task = processingTask;
        processingTask = null;
        cancellation?.Cancel();
        TrySignalSamples();
        await captureService.StopAsync(CancellationToken.None).ConfigureAwait(false);
        if (task is not null)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        cancellation?.Dispose();
        Interlocked.Exchange(ref latestSamples, null);
        SetState(finalState, message);
    }

    private static AudioVisualizationPacket BuildPacket(
        IReadOnlyList<byte> renderLevels,
        AudioVisualizationEvidence evidence) =>
        evidence.PackingMode switch
        {
            AudioVisualizationPackingMode.PaletteA or AudioVisualizationPackingMode.PaletteB =>
                AudioVisualizationProtocol.BuildFromLevels(
                    evidence.PackingMode,
                    renderLevels.ToArray(),
                    evidence.Framing),
            AudioVisualizationPackingMode.DuplicatedPairs =>
                AudioVisualizationProtocol.BuildFromLevels(
                    evidence.PackingMode,
                    Enumerable.Range(0, 12)
                        .Select(index => renderLevels[index * 2])
                        .ToArray(),
                    evidence.Framing),
            AudioVisualizationPackingMode.SpacedPairs =>
                AudioVisualizationProtocol.BuildFromLevels(
                    evidence.PackingMode,
                    Enumerable.Range(0, 4)
                        .SelectMany(index => new[] { renderLevels[index * 6], renderLevels[(index * 6) + 1] })
                        .ToArray(),
                    evidence.Framing),
            _ => throw new ArgumentOutOfRangeException(nameof(evidence), evidence.PackingMode, "Unknown audio packing mode.")
        };

    private static double GetCadenceHz(AudioVisualizationEvidence evidence)
    {
        var requested = Math.Clamp(evidence.RequestedCadenceHz, 1, 10);
        var observed = evidence.ObservedWriteCadenceHz is > 0
            ? Math.Clamp(evidence.ObservedWriteCadenceHz.Value, 1, 10)
            : requested;
        return Math.Min(requested, observed);
    }

    private void HandleSamplesAvailable(object? sender, AudioSamplesAvailableEventArgs args)
    {
        if (State != AudioVisualizerEngineState.Running && State != AudioVisualizerEngineState.Starting)
        {
            return;
        }

        Interlocked.Exchange(ref latestSamples, args);
        TrySignalSamples();
    }

    private void HandleCaptureStateChanged(object? sender, AudioCaptureStateChangedEventArgs args)
    {
        if (args.State is AudioCaptureState.Interrupted or AudioCaptureState.PermissionDenied
            or AudioCaptureState.Unavailable or AudioCaptureState.Failed)
        {
            CancelActiveRun();
            CancelCalibration();
            _ = StopAfterExternalSignalAsync(
                $"Microphone capture stopped: {args.StatusText} It will not resume automatically.");
        }
    }

    private void HandleTransportStateChanged(
        object? sender,
        AudioVisualizationTransportStateChangedEventArgs args)
    {
        if (!args.IsReady && processingTask is not null)
        {
            CancelActiveRun();
            _ = StopAfterExternalSignalAsync(
                $"Audio visualizer transport stopped: {args.StatusText} It will not resume automatically.");
        }
    }

    private void HandleVisualWorkCancelled(object? sender, VisualWorkCancelledEventArgs args)
    {
        CancelActiveRun();
        CancelCalibration();
        if (processingTask is not null)
        {
            _ = StopAfterExternalSignalAsync($"{args.Message} Microphone input will not resume automatically.");
        }
    }

    private void TrySignalSamples()
    {
        try
        {
            sampleSignal.Release();
        }
        catch (SemaphoreFullException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void CancelCalibration()
    {
        Interlocked.Increment(ref calibrationStopGeneration);
        CancellationTokenSource? cancellation;
        lock (stateLock)
        {
            cancellation = calibrationCancellation;
        }

        try
        {
            cancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void CancelActiveRun()
    {
        CancellationTokenSource? cancellation;
        lock (stateLock)
        {
            cancellation = runCancellation;
        }

        try
        {
            cancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void SetState(AudioVisualizerEngineState newState, string message)
    {
        lock (stateLock)
        {
            state = newState;
            statusText = message;
        }

        StateChanged?.Invoke(
            this,
            new AudioVisualizerEngineStateChangedEventArgs(
                newState,
                message,
                FramesSent,
                FramesSuppressed));
    }

    private void PublishState(string message)
    {
        AudioVisualizerEngineState currentState;
        lock (stateLock)
        {
            currentState = state;
            statusText = message;
        }

        StateChanged?.Invoke(
            this,
            new AudioVisualizerEngineStateChangedEventArgs(
                currentState,
                message,
                FramesSent,
                FramesSuppressed));
    }
}
