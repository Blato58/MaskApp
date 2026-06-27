namespace MaskApp.Core.Features.Connect;

public sealed record BleScannerStateChangedEventArgs(bool IsScanning, string Message);
