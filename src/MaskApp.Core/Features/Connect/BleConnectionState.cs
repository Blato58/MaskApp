namespace MaskApp.Core.Features.Connect;

public enum BleConnectionState
{
    Unavailable,
    Disconnected,
    Scanning,
    Connecting,
    Connected,
    Failed
}
