using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Tests.Features.Connect;

public sealed class DeviceOrderingTests
{
    [Fact]
    public void OrderDevices_DeduplicatesAndPlacesRememberedMaskFirst()
    {
        var now = DateTimeOffset.UtcNow;
        var remembered = new KnownMaskDevice("remembered", "Backstage", now, now);
        DiscoveredMaskDevice[] devices =
        [
            new("other", "Front mask", -30),
            new("remembered", "Backstage", -70),
            new("other", "Front mask", -20)
        ];

        var ordered = ConnectViewModel.OrderDevices(devices, remembered);

        Assert.Equal(["remembered", "other"], ordered.Select(device => device.Id));
        Assert.Equal(-20, ordered[1].SignalStrength);
    }
}
