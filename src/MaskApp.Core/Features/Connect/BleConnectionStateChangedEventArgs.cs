namespace MaskApp.Core.Features.Connect;

public sealed record BleConnectionStateChangedEventArgs(BleConnectionState State, string Message);
