namespace MaskApp.Core.Features.Text;

public sealed class SerializedTextUploadTransport : ITextUploadTransport, IDisposable
{
    private readonly ITextUploadTransport inner;
    private readonly SemaphoreSlim uploadGate = new(1, 1);

    public SerializedTextUploadTransport(ITextUploadTransport inner)
    {
        this.inner = inner;
    }

    public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged
    {
        add => inner.StateChanged += value;
        remove => inner.StateChanged -= value;
    }

    public string TransportDisplayName => inner.TransportDisplayName;

    public bool IsSimulated => inner.IsSimulated;

    public bool IsReady => inner.IsReady;

    public bool SupportsAcknowledgements => inner.SupportsAcknowledgements;

    public TextUploadTransportState State => inner.State;

    public string StatusText => inner.StatusText;

    public async Task<TextUploadResult> UploadAsync(
        TextUploadPackage package,
        TextUploadOptions options,
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
                await Task.Delay(options.PostUploadQuietPeriod).ConfigureAwait(false);
            }

            uploadGate.Release();
        }
    }

    public void Dispose()
    {
        uploadGate.Dispose();
    }
}
