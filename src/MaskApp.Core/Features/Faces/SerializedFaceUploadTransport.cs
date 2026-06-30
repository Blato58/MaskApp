namespace MaskApp.Core.Features.Faces;

public sealed class SerializedFaceUploadTransport : IFaceUploadTransport, IDisposable
{
    private readonly IFaceUploadTransport inner;
    private readonly SemaphoreSlim uploadGate = new(1, 1);

    public SerializedFaceUploadTransport(IFaceUploadTransport inner)
    {
        this.inner = inner;
    }

    public event EventHandler<FaceUploadTransportStateChangedEventArgs>? StateChanged
    {
        add => inner.StateChanged += value;
        remove => inner.StateChanged -= value;
    }

    public string TransportDisplayName => inner.TransportDisplayName;

    public bool IsSimulated => inner.IsSimulated;

    public bool IsReady => inner.IsReady;

    public bool SupportsAcknowledgements => inner.SupportsAcknowledgements;

    public FaceUploadTransportState State => inner.State;

    public string StatusText => inner.StatusText;

    public async Task<FaceUploadResult> UploadAsync(
        FaceUploadPackage package,
        FaceUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        await uploadGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await inner.UploadAsync(package, options, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (options.PostUploadQuietPeriod > TimeSpan.Zero && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(options.PostUploadQuietPeriod, cancellationToken).ConfigureAwait(false);
            }

            uploadGate.Release();
        }
    }

    public void Dispose()
    {
        uploadGate.Dispose();
    }
}
