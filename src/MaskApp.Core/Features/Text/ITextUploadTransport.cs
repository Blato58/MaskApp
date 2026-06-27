namespace MaskApp.Core.Features.Text;

public interface ITextUploadTransport
{
    event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged;

    string TransportDisplayName { get; }

    bool IsSimulated { get; }

    bool IsReady { get; }

    bool SupportsAcknowledgements { get; }

    TextUploadTransportState State { get; }

    string StatusText { get; }

    Task<TextUploadResult> UploadAsync(
        TextUploadPackage package,
        TextUploadOptions options,
        CancellationToken cancellationToken = default);

    Task<TextUploadResult> UploadAsync(TextUploadPackage package, CancellationToken cancellationToken = default) =>
        UploadAsync(package, TextUploadOptions.RequireAcknowledgements, cancellationToken);
}
