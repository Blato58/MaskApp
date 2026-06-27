namespace MaskApp.Core.Features.Text;

public sealed record TextUploadTransportStateChangedEventArgs(
    TextUploadTransportState State,
    string Message,
    bool SupportsAcknowledgements,
    bool IsReady);
