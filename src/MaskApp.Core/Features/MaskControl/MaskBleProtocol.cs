namespace MaskApp.Core.Features.MaskControl;

public static class MaskBleProtocol
{
    public const string ServiceUuid = "0000fff0-0000-1000-8000-00805f9b34fb";
    public const string CommandCharacteristicUuid = "d44bc439-abfd-45a2-b575-925416129600";
    public const string NotificationCharacteristicUuid = "d44bc439-abfd-45a2-b575-925416129601";
    public const string TextUploadCharacteristicUuid = "d44bc439-abfd-45a2-b575-92541612960a";
    public const string AudioVisualizationCharacteristicUuid = "d44bc439-abfd-45a2-b575-92541612960b";
    public const string GeneralWriteCharacteristicUuid = CommandCharacteristicUuid;
    public const string AesKeyHex = "32672f7974ad43451d9c6c894a0e8764";
    public const int CommandLength = 16;
}
