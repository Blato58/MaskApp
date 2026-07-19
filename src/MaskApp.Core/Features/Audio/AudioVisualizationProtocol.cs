using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Audio;

public enum AudioVisualizationPackingMode : byte
{
    PaletteA = 0,
    PaletteB = 1,
    DuplicatedPairs = 2,
    SpacedPairs = 3
}

public enum AudioVisualizationFraming
{
    FirmwareLength,
    LegacyAndroidLength
}

public sealed record AudioVisualizationPacket
{
    public required AudioVisualizationPackingMode PackingMode { get; init; }

    public required AudioVisualizationFraming Framing { get; init; }

    public required byte[] Plaintext { get; init; }

    public required byte[] EncryptedPayload { get; init; }
}

public static class AudioVisualizationProtocol
{
    public const int RenderValueCount = 24;
    public const byte MaximumLevel = 9;
    public const byte LegacyAndroidDeclaredLength = 15;

    public static AudioVisualizationPacket BuildFromLevels(
        AudioVisualizationPackingMode mode,
        ReadOnlySpan<byte> levels,
        AudioVisualizationFraming framing)
    {
        var expectedLevelCount = mode switch
        {
            AudioVisualizationPackingMode.PaletteA or AudioVisualizationPackingMode.PaletteB => 24,
            AudioVisualizationPackingMode.DuplicatedPairs => 12,
            AudioVisualizationPackingMode.SpacedPairs => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown audio packing mode.")
        };
        if (levels.Length != expectedLevelCount)
        {
            throw new ArgumentException(
                $"Audio packing mode {(byte)mode} requires exactly {expectedLevelCount} level values.",
                nameof(levels));
        }

        ValidateLevels(levels);
        var packed = new byte[expectedLevelCount / 2];
        for (var index = 0; index < packed.Length; index++)
        {
            packed[index] = (byte)(levels[index * 2] | (levels[(index * 2) + 1] << 4));
        }

        return BuildPacked(mode, packed, framing);
    }

    public static AudioVisualizationPacket BuildPacked(
        AudioVisualizationPackingMode mode,
        ReadOnlySpan<byte> packedValues,
        AudioVisualizationFraming framing)
    {
        var expectedPackedLength = mode switch
        {
            AudioVisualizationPackingMode.PaletteA or AudioVisualizationPackingMode.PaletteB => 12,
            AudioVisualizationPackingMode.DuplicatedPairs => 6,
            AudioVisualizationPackingMode.SpacedPairs => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown audio packing mode.")
        };
        if (packedValues.Length != expectedPackedLength)
        {
            throw new ArgumentException(
                $"Audio packing mode {(byte)mode} requires exactly {expectedPackedLength} packed bytes.",
                nameof(packedValues));
        }

        foreach (var value in packedValues)
        {
            if ((value & 0x0f) > MaximumLevel || (value >> 4) > MaximumLevel)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(packedValues),
                    "Audio visualizer nibbles must be in the firmware-supported range 0 through 9.");
            }
        }

        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = framing switch
        {
            AudioVisualizationFraming.FirmwareLength => mode switch
            {
                AudioVisualizationPackingMode.PaletteA or AudioVisualizationPackingMode.PaletteB => 13,
                AudioVisualizationPackingMode.DuplicatedPairs => 7,
                AudioVisualizationPackingMode.SpacedPairs => 6,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown audio packing mode.")
            },
            AudioVisualizationFraming.LegacyAndroidLength => LegacyAndroidDeclaredLength,
            _ => throw new ArgumentOutOfRangeException(nameof(framing), framing, "Unknown audio framing profile.")
        };
        plaintext[1] = (byte)mode;
        packedValues.CopyTo(plaintext.AsSpan(2));

        return new AudioVisualizationPacket
        {
            PackingMode = mode,
            Framing = framing,
            Plaintext = plaintext,
            EncryptedPayload = MaskProtocolCrypto.EncryptBlock(plaintext)
        };
    }

    public static byte[] ExpandForDiagnostics(AudioVisualizationPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        if (packet.Plaintext.Length != MaskBleProtocol.CommandLength)
        {
            throw new ArgumentException("Audio plaintext must be one 16-byte protocol block.", nameof(packet));
        }

        var output = new byte[RenderValueCount];
        var source = packet.Plaintext.AsSpan(2);
        switch (packet.PackingMode)
        {
            case AudioVisualizationPackingMode.PaletteA:
            case AudioVisualizationPackingMode.PaletteB:
                for (var index = 0; index < 12; index++)
                {
                    output[index * 2] = (byte)(source[index] & 0x0f);
                    output[(index * 2) + 1] = (byte)(source[index] >> 4);
                }
                break;
            case AudioVisualizationPackingMode.DuplicatedPairs:
                for (var index = 0; index < 6; index++)
                {
                    var low = (byte)(source[index] & 0x0f);
                    var high = (byte)(source[index] >> 4);
                    output[index * 4] = low;
                    output[(index * 4) + 1] = low;
                    output[(index * 4) + 2] = high;
                    output[(index * 4) + 3] = high;
                }
                break;
            case AudioVisualizationPackingMode.SpacedPairs:
                for (var index = 0; index < 4; index++)
                {
                    var offset = index * 6;
                    output[offset] = (byte)(source[index] & 0x0f);
                    output[offset + 1] = (byte)(source[index] >> 4);
                    output[offset + 2] = 0;
                    output[offset + 3] = output[offset];
                    output[offset + 4] = output[offset + 1];
                    output[offset + 5] = 0;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(packet), packet.PackingMode, "Unknown audio packing mode.");
        }

        return output;
    }

    private static void ValidateLevels(ReadOnlySpan<byte> levels)
    {
        foreach (var level in levels)
        {
            if (level > MaximumLevel)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(levels),
                    "Audio visualizer levels must be in the firmware-supported range 0 through 9.");
            }
        }
    }
}
