namespace MaskApp.Core.Features.Faces;

public sealed class FaceUploadTransportStateChangedEventArgs : EventArgs
{
    public FaceUploadTransportStateChangedEventArgs(
        FaceUploadTransportState state,
        string message,
        bool supportsAcknowledgements,
        bool isReady)
    {
        State = state;
        Message = message;
        SupportsAcknowledgements = supportsAcknowledgements;
        IsReady = isReady;
    }

    public FaceUploadTransportState State { get; }

    public string Message { get; }

    public bool SupportsAcknowledgements { get; }

    public bool IsReady { get; }
}
