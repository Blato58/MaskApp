namespace MaskApp.Core.Features.Faces;

public sealed class SimulatedFaceUploadTransport : IFaceUploadTransport
{
    event EventHandler<FaceUploadTransportStateChangedEventArgs>? IFaceUploadTransport.StateChanged
    {
        add { }
        remove { }
    }

    public string TransportDisplayName => "Simulator";

    public bool IsSimulated => true;

    public bool IsReady => true;

    public bool SupportsAcknowledgements => true;

    public FaceUploadTransportState State => FaceUploadTransportState.Simulated;

    public string StatusText => "Simulator ready.";

    public FaceUploadPackage? LastPackage { get; private set; }

    public Task<FaceUploadResult> UploadAsync(
        FaceUploadPackage package,
        FaceUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastPackage = package;
        var suffix = options.CompatibilityWriteOnly ? " without ACK confirmation" : string.Empty;
        return Task.FromResult(FaceUploadResult.Success(
            $"Simulated face upload to slot {package.Slot}{suffix} ({package.Frames.Count} frame(s)).",
            package.Frames.Count));
    }
}
