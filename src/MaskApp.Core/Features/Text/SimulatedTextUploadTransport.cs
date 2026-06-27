namespace MaskApp.Core.Features.Text;

public sealed class SimulatedTextUploadTransport : ITextUploadTransport
{
    event EventHandler<TextUploadTransportStateChangedEventArgs>? ITextUploadTransport.StateChanged
    {
        add { }
        remove { }
    }

    public string TransportDisplayName => "Simulator";

    public bool IsSimulated => true;

    public bool IsReady => true;

    public bool SupportsAcknowledgements => true;

    public TextUploadTransportState State => TextUploadTransportState.Simulated;

    public string StatusText => "Simulator ready.";

    public TextUploadPackage? LastPackage { get; private set; }

    public Task<TextUploadResult> UploadAsync(
        TextUploadPackage package,
        TextUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastPackage = package;

        if (options.CompatibilityWriteOnly)
        {
            return Task.FromResult(TextUploadResult.Success(
                $"Simulated write-only text upload complete ({package.Frames.Count} frame(s)).",
                package.Frames.Count));
        }

        var startAck = TextUploadProtocol.ParsePlaintextAcknowledgement(
            [7, (byte)'D', (byte)'A', (byte)'T', (byte)'S', (byte)'O', (byte)'K', 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        if (startAck != TextUploadAcknowledgement.StartAccepted)
        {
            return Task.FromResult(TextUploadResult.Failure("Simulator did not accept text upload.", 0));
        }

        foreach (var _ in package.Frames)
        {
            var frameAck = TextUploadProtocol.ParsePlaintextAcknowledgement(
                [4, (byte)'R', (byte)'E', (byte)'O', (byte)'K', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
            if (frameAck != TextUploadAcknowledgement.FrameAccepted)
            {
                return Task.FromResult(TextUploadResult.Failure("Simulator rejected a text frame.", 0));
            }
        }

        var finishAck = TextUploadProtocol.ParsePlaintextAcknowledgement(
            [8, (byte)'D', (byte)'A', (byte)'T', (byte)'C', (byte)'P', (byte)'O', (byte)'K', 0, 0, 0, 0, 0, 0, 0, 0]);
        return Task.FromResult(
            finishAck == TextUploadAcknowledgement.Complete
                ? TextUploadResult.Success($"Simulated text upload complete ({package.Frames.Count} frame(s)).", package.Frames.Count)
                : TextUploadResult.Failure("Simulator did not finish text upload.", package.Frames.Count));
    }
}
