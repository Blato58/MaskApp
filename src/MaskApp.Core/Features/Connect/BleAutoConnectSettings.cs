namespace MaskApp.Core.Features.Connect;

public sealed record BleAutoConnectSettings
{
    public static BleAutoConnectSettings Defaults { get; } = new();

    public bool AutoConnectEnabled { get; init; }

    public bool RememberLastDeviceEnabled { get; init; } = true;

    public KnownMaskDevice? LastKnownDevice { get; init; }

    public BleAutoConnectSettings Normalize() =>
        LastKnownDevice is null
            ? this with { AutoConnectEnabled = false, RememberLastDeviceEnabled = RememberLastDeviceEnabled }
            : this;
}
