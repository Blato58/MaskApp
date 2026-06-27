namespace MaskApp.Core.Features.Text;

public enum TextUploadTransportState
{
    Disconnected,
    Discovering,
    Ready,
    CompatibilityReady,
    Failed,
    Simulated,
    Unavailable
}
