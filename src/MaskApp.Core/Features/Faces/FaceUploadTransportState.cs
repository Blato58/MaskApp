namespace MaskApp.Core.Features.Faces;

public enum FaceUploadTransportState
{
    Disconnected,
    Discovering,
    Ready,
    CompatibilityReady,
    Simulated,
    Failed
}
