#if IOS
using System.Runtime.InteropServices;
using AVFoundation;
using Foundation;
using MaskApp.Core.Features.Audio;
using Microsoft.Maui.ApplicationModel;

namespace MaskApp.App.Infrastructure.Audio;

public sealed class IosAudioCaptureService : IAudioCaptureService, IAsyncDisposable
{
    private const uint TapBufferSize = 512;
    private readonly object sync = new();
    private readonly SemaphoreSlim lifecycleGate = new(1, 1);
    private AVAudioEngine? audioEngine;
    private AVAudioInputNode? inputNode;
    private NSObject? interruptionObserver;
    private NSObject? routeChangeObserver;
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
            if (audioEngine is not null)
            {
                return AudioCaptureStartResult.Success("iOS microphone capture is already active.");
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
            var session = AVAudioSession.SharedInstance();
            var categoryError = session.SetCategory(
                AVAudioSessionCategory.Record,
                AVAudioSessionCategoryOptions.AllowBluetooth);
            if (categoryError is not null)
            {
                SetState(
                    AudioCaptureState.Failed,
                    $"iOS could not configure the recording audio session: {categoryError.LocalizedDescription}");
                return AudioCaptureStartResult.Failure(State, StatusText);
            }

            var activationError = session.SetActive(true);
            if (activationError is not null)
            {
                SetState(
                    AudioCaptureState.Failed,
                    $"iOS could not activate the recording audio session: {activationError.LocalizedDescription}");
                return AudioCaptureStartResult.Failure(State, StatusText);
            }

            var engine = new AVAudioEngine();
            var input = engine.InputNode;
            audioEngine = engine;
            inputNode = input;
            var format = input.GetBusOutputFormat(0);
            input.InstallTapOnBus(0, TapBufferSize, format, HandleAudioBuffer);
            engine.Prepare();
            if (!engine.StartAndReturnError(out var error))
            {
                await StopResourcesAsync().ConfigureAwait(false);
                SetState(
                    AudioCaptureState.Failed,
                    $"iOS microphone capture did not start: {error?.LocalizedDescription ?? "unknown AVAudioEngine error"}");
                return AudioCaptureStartResult.Failure(State, StatusText);
            }

            interruptionObserver = AVAudioSession.Notifications.ObserveInterruption(
                (_, _) => _ = StopForSystemEventAsync(
                    AudioCaptureState.Interrupted,
                    "The iOS audio session was interrupted."));
            routeChangeObserver = AVAudioSession.Notifications.ObserveRouteChange(
                (_, _) => _ = StopForSystemEventAsync(
                    AudioCaptureState.Interrupted,
                    "The iOS audio input route changed."));
            SetState(AudioCaptureState.Capturing, "iOS microphone capture is active in the foreground.");
            return AudioCaptureStartResult.Success(StatusText);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await StopResourcesAsync().ConfigureAwait(false);
            SetState(AudioCaptureState.Failed, $"iOS microphone capture failed: {exception.Message}");
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
            SetState(AudioCaptureState.Stopped, "iOS microphone capture is stopped.");
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

    private void HandleAudioBuffer(AVAudioPcmBuffer buffer, AVAudioTime when)
    {
        var frameCount = checked((int)buffer.FrameLength);
        var channels = buffer.FloatChannelData;
        if (frameCount <= 0 || channels == IntPtr.Zero)
        {
            return;
        }

        var firstChannel = Marshal.ReadIntPtr(channels);
        if (firstChannel == IntPtr.Zero)
        {
            return;
        }

        var samples = new float[frameCount];
        Marshal.Copy(firstChannel, samples, 0, frameCount);
        var sampleRate = checked((int)Math.Round(buffer.Format.SampleRate));
        SamplesAvailable?.Invoke(
            this,
            new AudioSamplesAvailableEventArgs(samples, sampleRate, DateTimeOffset.UtcNow));
    }

    private async Task StopForSystemEventAsync(AudioCaptureState finalState, string message)
    {
        await lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposed || audioEngine is null)
            {
                return;
            }

            await StopResourcesAsync().ConfigureAwait(false);
            SetState(finalState, $"{message} Capture will not resume automatically.");
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    private Task StopResourcesAsync()
    {
        interruptionObserver?.Dispose();
        interruptionObserver = null;
        routeChangeObserver?.Dispose();
        routeChangeObserver = null;

        if (inputNode is not null)
        {
            inputNode.RemoveTapOnBus(0);
            inputNode = null;
        }

        if (audioEngine is not null)
        {
            audioEngine.Stop();
            audioEngine.Dispose();
            audioEngine = null;
        }

        AVAudioSession.SharedInstance().SetActive(
            false,
            AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation);
        return Task.CompletedTask;
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
