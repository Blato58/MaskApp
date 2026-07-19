#if ANDROID
using Android.Media;
using MaskApp.Core.Features.Audio;
using Microsoft.Maui.ApplicationModel;

namespace MaskApp.App.Infrastructure.Audio;

public sealed class AndroidAudioCaptureService : IAudioCaptureService, IAsyncDisposable
{
    private const int SampleRate = 16_000;
    private const int RequestedSampleCount = 512;
    private readonly object sync = new();
    private readonly SemaphoreSlim lifecycleGate = new(1, 1);
    private AudioRecord? audioRecord;
    private CancellationTokenSource? captureCancellation;
    private Task? captureTask;
    private AudioCaptureState state;
    private string statusText = "Microphone capture is stopped.";
    private bool disposed;

    public event EventHandler<AudioSamplesAvailableEventArgs>? SamplesAvailable;

    public event EventHandler<AudioCaptureStateChangedEventArgs>? StateChanged;

    public AudioCaptureState State
    {
        get
        {
            lock (sync)
            {
                return state;
            }
        }
    }

    public string StatusText
    {
        get
        {
            lock (sync)
            {
                return statusText;
            }
        }
    }

    public async Task<AudioCaptureStartResult> StartAsync(
        CancellationToken cancellationToken = default)
    {
        await lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (audioRecord is not null)
            {
                return AudioCaptureStartResult.Success("Android microphone capture is already active.");
            }

            SetState(AudioCaptureState.RequestingPermission, "Requesting microphone permission...");
            var permission = await Permissions.RequestAsync<Permissions.Microphone>();
            if (permission != PermissionStatus.Granted)
            {
                SetState(
                    AudioCaptureState.PermissionDenied,
                    "Microphone permission was denied. Use system Settings, then retry explicitly.");
                return AudioCaptureStartResult.Failure(State, StatusText);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var minimumBufferSize = AudioRecord.GetMinBufferSize(
                SampleRate,
                ChannelIn.Mono,
                Android.Media.Encoding.Pcm16bit);
            if (minimumBufferSize <= 0)
            {
                SetState(AudioCaptureState.Unavailable, "Android reported no usable microphone buffer size.");
                return AudioCaptureStartResult.Failure(State, StatusText);
            }

            var record = new AudioRecord(
                AudioSource.Mic,
                SampleRate,
                ChannelIn.Mono,
                Android.Media.Encoding.Pcm16bit,
                Math.Max(minimumBufferSize, RequestedSampleCount * sizeof(short)));
            if (record.State != Android.Media.State.Initialized)
            {
                record.Release();
                record.Dispose();
                SetState(AudioCaptureState.Unavailable, "Android could not initialize the microphone recorder.");
                return AudioCaptureStartResult.Failure(State, StatusText);
            }

            record.StartRecording();
            audioRecord = record;
            captureCancellation = new CancellationTokenSource();
            captureTask = Task.Run(
                () => CaptureLoopAsync(record, captureCancellation.Token),
                CancellationToken.None);
            SetState(AudioCaptureState.Capturing, "Android microphone capture is active in the foreground.");
            return AudioCaptureStartResult.Success(StatusText);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await StopResourcesAsync().ConfigureAwait(false);
            SetState(AudioCaptureState.Failed, $"Android microphone capture failed: {exception.Message}");
            return AudioCaptureStartResult.Failure(State, StatusText);
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (disposed)
            {
                return;
            }

            await StopResourcesAsync().ConfigureAwait(false);
            SetState(AudioCaptureState.Stopped, "Android microphone capture is stopped.");
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposed)
            {
                return;
            }

            await StopResourcesAsync().ConfigureAwait(false);
            disposed = true;
        }
        finally
        {
            lifecycleGate.Release();
            lifecycleGate.Dispose();
        }
    }

    private async Task CaptureLoopAsync(AudioRecord record, CancellationToken cancellationToken)
    {
        var pcm = new short[RequestedSampleCount];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var read = record.Read(pcm, 0, pcm.Length);
                if (read > 0)
                {
                    var samples = new float[read];
                    for (var index = 0; index < read; index++)
                    {
                        samples[index] = pcm[index] / 32768f;
                    }

                    SamplesAvailable?.Invoke(
                        this,
                        new AudioSamplesAvailableEventArgs(samples, SampleRate, DateTimeOffset.UtcNow));
                    continue;
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    SetState(AudioCaptureState.Failed, $"Android microphone read failed with code {read}.");
                    _ = StopAfterCaptureFailureAsync();
                }
                return;
            }
        }
        catch (Exception exception) when (
            !cancellationToken.IsCancellationRequested
            && exception is not OperationCanceledException)
        {
            SetState(AudioCaptureState.Failed, $"Android microphone capture failed: {exception.Message}");
            _ = StopAfterCaptureFailureAsync();
        }

        await Task.CompletedTask;
    }

    private async Task StopAfterCaptureFailureAsync()
    {
        await lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!disposed && audioRecord is not null)
            {
                await StopResourcesAsync(skipCaptureAwait: true).ConfigureAwait(false);
            }
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    private async Task StopResourcesAsync(bool skipCaptureAwait = false)
    {
        var cancellation = captureCancellation;
        var task = captureTask;
        var record = audioRecord;
        captureCancellation = null;
        captureTask = null;
        audioRecord = null;
        cancellation?.Cancel();
        if (record is not null)
        {
            try
            {
                record.Stop();
            }
            catch (Java.Lang.IllegalStateException)
            {
            }
        }

        if (!skipCaptureAwait && task is not null)
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
        if (record is not null)
        {
            record.Release();
            record.Dispose();
        }
    }

    private void SetState(AudioCaptureState newState, string message)
    {
        lock (sync)
        {
            state = newState;
            statusText = message;
        }

        StateChanged?.Invoke(this, new AudioCaptureStateChangedEventArgs(newState, message));
    }
}
#endif
