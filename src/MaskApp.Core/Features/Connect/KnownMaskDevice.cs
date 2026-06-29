namespace MaskApp.Core.Features.Connect;

public sealed record KnownMaskDevice(
    string Id,
    string Name,
    DateTimeOffset LastSeenAt,
    DateTimeOffset LastConnectedAt)
{
    public static KnownMaskDevice FromDiscoveredDevice(DiscoveredMaskDevice device, DateTimeOffset timestamp) =>
        new(
            device.Id,
            string.IsNullOrWhiteSpace(device.Name) ? "Shining Mask" : device.Name,
            timestamp,
            timestamp);
}
