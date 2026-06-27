using MaskApp.Core.Bluetooth;

namespace MaskApp.Core.Tests.Bluetooth;

public sealed class BleAdvertisementMatcherTests
{
    [Fact]
    public void MatchProduct_ReturnsShiningMask_WhenManufacturerDataMatches()
    {
        byte[] advertisementData =
        [
            2, 0x01, 0x06,
            6, 0xFF, 84, 82, 0, 74, 1
        ];

        var product = BleAdvertisementMatcher.MatchProduct(advertisementData);

        Assert.Equal(BleAdvertisementMatcher.ShiningMaskProduct, product);
    }

    [Fact]
    public void MatchProduct_ReturnsUnknown_WhenManufacturerDataDoesNotMatch()
    {
        byte[] advertisementData =
        [
            2, 0x01, 0x06,
            6, 0xFF, 84, 82, 0, 73, 1
        ];

        var product = BleAdvertisementMatcher.MatchProduct(advertisementData);

        Assert.Equal(BleAdvertisementMatcher.UnknownProduct, product);
    }

    [Fact]
    public void MatchProduct_ReturnsUnknown_WhenFieldLengthExceedsPacket()
    {
        byte[] advertisementData = [10, 0xFF, 84];

        var product = BleAdvertisementMatcher.MatchProduct(advertisementData);

        Assert.Equal(BleAdvertisementMatcher.UnknownProduct, product);
    }
}
