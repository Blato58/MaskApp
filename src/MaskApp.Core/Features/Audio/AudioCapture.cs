namespace MaskApp.Core.Features.Audio;

public enum AudioCaptureState
{
    Stopped,
    RequestingPermission,
    Capturing,
    PermissionDenied,
    Interrupted,
    Unavailable,
    Failed
}

public sealed class AudioSamplesAvailableEventArgs(
    float[] samples,
    int sampleRate,
    DateTimeOffset capturedAt) : EventArgs
{
    public float[] Samples { get; } = samples;

    public int SampleRate { get; } = sampleRate;

    public DateTimeOffset CapturedAt { get; } = capturedAt;
}

public sealed class AudioCaptureStateChangedEventArgs(
    AudioCaptureState state,
    string statusText) : EventArgs
{
    public AudioCaptureState State { get; } = state;

    public string StatusText { get; } = statusText;
}

public sealed record AudioCaptureStartResult(bool Succeeded, AudioCaptureState State, string Message)
{
    public static AudioCaptureStartResult Success(string message) =>
        new(true, AudioCaptureState.Capturing, message);

    public static AudioCaptureStartResult Failure(AudioCaptureState state, string message) =>
        new(false, state, message);
}

public interface IAudioCaptureService
{
    event EventHandler<AudioSamplesAvailableEventArgs>? SamplesAvailable;

    event EventHandler<AudioCaptureStateChangedEventArgs>? StateChanged;

    AudioCaptureState State { get; }

    string StatusText { get; }

    Task<AudioCaptureStartResult> StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}

public sealed class UnavailableAudioCaptureService : IAudioCaptureService
{
    public event EventHandler<AudioSamplesAvailableEventArgs>? SamplesAvailable
    {
        add { }
        remove { }
    }

    public event EventHandler<AudioCaptureStateChangedEventArgs>? StateChanged
    {
        add { }
        remove { }
    }

    public AudioCaptureState State => AudioCaptureState.Unavailable;

    public string StatusText => "Microphone capture is unavailable on this platform.";

    public Task<AudioCaptureStartResult> StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AudioCaptureStartResult.Failure(State, StatusText));
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
