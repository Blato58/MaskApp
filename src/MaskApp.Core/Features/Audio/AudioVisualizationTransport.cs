namespace MaskApp.Core.Features.Audio;

public enum AudioVisualizationTransportState
{
    Disconnected,
    Discovering,
    Ready,
    Unsupported,
    Failed,
    Simulated
}

public sealed class AudioVisualizationTransportStateChangedEventArgs(
    AudioVisualizationTransportState state,
    string statusText,
    bool isReady) : EventArgs
{
    public AudioVisualizationTransportState State { get; } = state;

    public string StatusText { get; } = statusText;

    public bool IsReady { get; } = isReady;
}

public sealed record AudioVisualizationSendResult(bool Succeeded, string Message)
{
    public static AudioVisualizationSendResult Success(string message) => new(true, message);

    public static AudioVisualizationSendResult Failure(string message) => new(false, message);
}

public interface IAudioVisualizationTransport
{
    event EventHandler<AudioVisualizationTransportStateChangedEventArgs>? StateChanged;

    bool IsReady { get; }

    bool IsSimulated { get; }

    AudioVisualizationTransportState State { get; }

    string StatusText { get; }

    Task<AudioVisualizationSendResult> SendAsync(
        AudioVisualizationPacket packet,
        CancellationToken cancellationToken = default);
}

public sealed class SimulatedAudioVisualizationTransport : IAudioVisualizationTransport
{
    public event EventHandler<AudioVisualizationTransportStateChangedEventArgs>? StateChanged
    {
        add { }
        remove { }
    }

    public bool IsReady => true;

    public bool IsSimulated => true;

    public AudioVisualizationTransportState State => AudioVisualizationTransportState.Simulated;

    public string StatusText => "Audio visualizer simulator ready.";

    public AudioVisualizationPacket? LastPacket { get; private set; }

    public int SentPacketCount { get; private set; }

    public Task<AudioVisualizationSendResult> SendAsync(
        AudioVisualizationPacket packet,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packet);
        cancellationToken.ThrowIfCancellationRequested();
        LastPacket = packet;
        SentPacketCount++;
        return Task.FromResult(AudioVisualizationSendResult.Success("Simulated audio visualizer write complete."));
    }
}
