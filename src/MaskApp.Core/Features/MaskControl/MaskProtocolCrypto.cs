using System.Security.Cryptography;

namespace MaskApp.Core.Features.MaskControl;

internal static class MaskProtocolCrypto
{
    private static readonly byte[] AesKey = Convert.FromHexString(MaskBleProtocol.AesKeyHex);

    public static byte[] EncryptBlock(ReadOnlySpan<byte> plaintext)
    {
        if (plaintext.Length != MaskBleProtocol.CommandLength)
        {
            throw new ArgumentException("Mask protocol blocks must be exactly 16 bytes.", nameof(plaintext));
        }

        using var aes = Aes.Create();
        aes.Key = AesKey;

#pragma warning disable CA5358 // The reverse-engineered mask protocol requires AES-128 ECB.
        return aes.EncryptEcb(plaintext, PaddingMode.None);
#pragma warning restore CA5358
    }

    public static byte[] DecryptBlock(ReadOnlySpan<byte> encrypted)
    {
        if (encrypted.Length != MaskBleProtocol.CommandLength)
        {
            throw new ArgumentException("Mask protocol blocks must be exactly 16 bytes.", nameof(encrypted));
        }

        using var aes = Aes.Create();
        aes.Key = AesKey;

#pragma warning disable CA5358 // The reverse-engineered mask protocol requires AES-128 ECB.
        return aes.DecryptEcb(encrypted, PaddingMode.None);
#pragma warning restore CA5358
    }
}
