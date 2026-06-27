namespace MaskApp.Core.Features.Text;

public interface ITextUploadTransport
{
    string TransportDisplayName { get; }

    bool IsSimulated { get; }

    bool IsReady { get; }

    string StatusText { get; }

    Task<TextUploadResult> UploadAsync(TextUploadPackage package, CancellationToken cancellationToken = default);
}
