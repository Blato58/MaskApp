namespace MaskApp.Core.Bluetooth;

public static class BleAdvertisementMatcher
{
    public const int ShiningMaskProduct = 0;
    public const int UnknownProduct = -1;

    private const byte ManufacturerSpecificData = 0xFF;

    // Ported from android/base/app/BleConfig.java.
    // JSONB.Constants.BC_STR_ASCII_FIX_1 is 74 (0x4A).
    private static readonly byte[] BroadcastSpecificProduct = [84, 82, 0, 74];

    public static int MatchProduct(ReadOnlySpan<byte> advertisementData)
    {
        var index = 0;

        while (index < advertisementData.Length)
        {
            var fieldLength = advertisementData[index++];

            if (fieldLength <= 0)
            {
                continue;
            }

            if (fieldLength > 31 || index + fieldLength > advertisementData.Length)
            {
                return UnknownProduct;
            }

            var field = advertisementData.Slice(index, fieldLength);
            if (field.Length > BroadcastSpecificProduct.Length
                && field[0] == ManufacturerSpecificData
                && field[1..].StartsWith(BroadcastSpecificProduct))
            {
                return ShiningMaskProduct;
            }

            index += fieldLength;
        }

        return UnknownProduct;
    }
}
