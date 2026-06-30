namespace MaskApp.Core.Features.Faces;

public interface IFaceUploadTransport
{
    event EventHandler<FaceUploadTransportStateChangedEventArgs>? StateChanged;

    string TransportDisplayName { get; }

    bool IsSimulated { get; }

    bool IsReady { get; }

    bool SupportsAcknowledgements { get; }

    FaceUploadTransportState State { get; }

    string StatusText { get; }

    Task<FaceUploadResult> UploadAsync(
        FaceUploadPackage package,
        FaceUploadOptions options,
        CancellationToken cancellationToken = default);

    Task<FaceUploadResult> UploadAsync(FaceUploadPackage package, CancellationToken cancellationToken = default) =>
        UploadAsync(package, FaceUploadOptions.RequireAcknowledgements, cancellationToken);
}
