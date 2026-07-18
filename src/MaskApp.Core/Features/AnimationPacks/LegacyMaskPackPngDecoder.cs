using System.Buffers.Binary;
using System.IO.Compression;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.AnimationPacks;

public static class LegacyMaskPackPngDecoder
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private const int MaxChunks = 256;
    private const int MaxCompressedBytes = 8 * 1024 * 1024;

    public static FacePattern Decode44x58(
        ReadOnlySpan<byte> png,
        string id,
        string displayName,
        int preferredSlot)
    {
        if (png.Length > MaxCompressedBytes || !png.StartsWith(Signature))
        {
            throw new InvalidDataException("Legacy MaskPack frame is not a bounded PNG file.");
        }

        var offset = Signature.Length;
        var width = 0;
        var height = 0;
        var bitDepth = 0;
        var colorType = 0;
        byte[] palette = [];
        byte[] transparency = [];
        using var compressed = new MemoryStream();
        var chunkCount = 0;
        var sawHeader = false;
        var sawPalette = false;
        var sawData = false;
        var sawEnd = false;
        while (offset < png.Length)
        {
            if (++chunkCount > MaxChunks || png.Length - offset < 12)
            {
                throw new InvalidDataException("Legacy PNG chunk structure exceeds the safe limit.");
            }

            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.Slice(offset, 4)));
            offset += 4;
            var type = png.Slice(offset, 4);
            offset += 4;
            if (length < 0 || length > MaxCompressedBytes || png.Length - offset < length + 4)
            {
                throw new InvalidDataException("Legacy PNG chunk length is invalid.");
            }

            var data = png.Slice(offset, length);
            var storedCrc = BinaryPrimitives.ReadUInt32BigEndian(png.Slice(offset + length, 4));
            if (ComputeCrc(type, data) != storedCrc)
            {
                throw new InvalidDataException("Legacy PNG chunk checksum is invalid.");
            }

            offset += length + 4;
            if (type.SequenceEqual("IHDR"u8))
            {
                if (sawHeader || chunkCount != 1 || length != 13)
                {
                    throw new InvalidDataException("Legacy PNG has an invalid IHDR chunk.");
                }

                width = checked((int)BinaryPrimitives.ReadUInt32BigEndian(data[..4]));
                height = checked((int)BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4)));
                bitDepth = data[8];
                colorType = data[9];
                if (data[10] != 0 || data[11] != 0 || data[12] != 0)
                {
                    throw new InvalidDataException("Legacy PNG compression, filter, or interlace mode is unsupported.");
                }

                sawHeader = true;
            }
            else if (type.SequenceEqual("PLTE"u8))
            {
                if (!sawHeader || sawPalette || sawData)
                {
                    throw new InvalidDataException("Legacy PNG palette is out of order or duplicated.");
                }

                palette = data.ToArray();
                sawPalette = true;
            }
            else if (type.SequenceEqual("tRNS"u8))
            {
                if (!sawHeader || sawData)
                {
                    throw new InvalidDataException("Legacy PNG transparency data is out of order.");
                }

                transparency = data.ToArray();
            }
            else if (type.SequenceEqual("IDAT"u8))
            {
                if (!sawHeader)
                {
                    throw new InvalidDataException("Legacy PNG image data appears before its header.");
                }

                sawData = true;
                compressed.Write(data);
                if (compressed.Length > MaxCompressedBytes)
                {
                    throw new InvalidDataException("Legacy PNG compressed data exceeds the safe limit.");
                }
            }
            else if (type.SequenceEqual("IEND"u8))
            {
                if (!sawHeader || !sawData || length != 0)
                {
                    throw new InvalidDataException("Legacy PNG has an invalid end chunk.");
                }

                sawEnd = true;
                break;
            }
        }

        if (!sawHeader || !sawData || !sawEnd || offset != png.Length
            || width != MaskPackManifestParser.RequiredWidth
            || height != MaskPackManifestParser.RequiredHeight || bitDepth != 8)
        {
            throw new InvalidDataException(
                $"Legacy MaskPack frames must be non-interlaced {MaskPackManifestParser.RequiredWidth}x{MaskPackManifestParser.RequiredHeight} 8-bit PNG files.");
        }

        var bytesPerPixel = colorType switch
        {
            0 => 1,
            2 => 3,
            3 => 1,
            4 => 2,
            6 => 4,
            _ => throw new InvalidDataException($"Legacy PNG color type {colorType} is unsupported.")
        };
        if (colorType == 3 && (palette.Length is < 3 or > 768 || palette.Length % 3 != 0))
        {
            throw new InvalidDataException("Legacy indexed PNG palette is invalid.");
        }

        var rowBytes = checked(width * bytesPerPixel);
        var expectedBytes = checked(height * (rowBytes + 1));
        var filtered = DecompressBounded(compressed.ToArray(), expectedBytes);
        var decoded = Unfilter(filtered, width, height, bytesPerPixel);
        var target = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        var left = (FacePattern.Width - width) / 2;
        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var sourceOffset = (row * rowBytes) + (column * bytesPerPixel);
                var (red, green, blue, alpha) = ReadColor(
                    decoded,
                    sourceOffset,
                    colorType,
                    palette,
                    transparency);
                if (alpha >= 128 && (red != 0 || green != 0 || blue != 0))
                {
                    target[(row * FacePattern.Width) + left + column] =
                        new FacePixel(true, new FaceColor(red, green, blue));
                }
            }
        }

        return new FacePattern
        {
            Id = id,
            DisplayName = displayName,
            Source = FacePatternSource.ImportedPhoto,
            PreferredSlot = preferredSlot,
            Pixels = target
        }.Normalize();
    }

    private static byte[] DecompressBounded(byte[] compressed, int expectedBytes)
    {
        using var input = new MemoryStream(compressed, writable: false);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        var result = new byte[expectedBytes];
        var offset = 0;
        while (offset < result.Length)
        {
            var read = zlib.Read(result, offset, result.Length - offset);
            if (read == 0)
            {
                break;
            }

            offset += read;
        }

        if (offset != expectedBytes || zlib.ReadByte() != -1)
        {
            throw new InvalidDataException("Legacy PNG decompressed size does not match its dimensions.");
        }

        return result;
    }

    private static byte[] Unfilter(byte[] source, int width, int height, int bytesPerPixel)
    {
        var rowBytes = checked(width * bytesPerPixel);
        var result = new byte[checked(rowBytes * height)];
        var sourceOffset = 0;
        for (var row = 0; row < height; row++)
        {
            var filter = source[sourceOffset++];
            var rowOffset = row * rowBytes;
            var previousOffset = rowOffset - rowBytes;
            for (var index = 0; index < rowBytes; index++)
            {
                var raw = source[sourceOffset++];
                var left = index >= bytesPerPixel ? result[rowOffset + index - bytesPerPixel] : 0;
                var up = row > 0 ? result[previousOffset + index] : 0;
                var upperLeft = row > 0 && index >= bytesPerPixel
                    ? result[previousOffset + index - bytesPerPixel]
                    : 0;
                result[rowOffset + index] = filter switch
                {
                    0 => raw,
                    1 => unchecked((byte)(raw + left)),
                    2 => unchecked((byte)(raw + up)),
                    3 => unchecked((byte)(raw + ((left + up) / 2))),
                    4 => unchecked((byte)(raw + Paeth(left, up, upperLeft))),
                    _ => throw new InvalidDataException($"Legacy PNG filter {filter} is unsupported.")
                };
            }
        }

        return result;
    }

    private static int Paeth(int left, int up, int upperLeft)
    {
        var estimate = left + up - upperLeft;
        var leftDistance = Math.Abs(estimate - left);
        var upDistance = Math.Abs(estimate - up);
        var upperLeftDistance = Math.Abs(estimate - upperLeft);
        return leftDistance <= upDistance && leftDistance <= upperLeftDistance
            ? left
            : upDistance <= upperLeftDistance ? up : upperLeft;
    }

    private static (byte Red, byte Green, byte Blue, byte Alpha) ReadColor(
        byte[] decoded,
        int offset,
        int colorType,
        byte[] palette,
        byte[] transparency) => colorType switch
    {
        0 => (decoded[offset], decoded[offset], decoded[offset], 255),
        2 => (decoded[offset], decoded[offset + 1], decoded[offset + 2], 255),
        3 => ReadPalette(decoded[offset], palette, transparency),
        4 => (decoded[offset], decoded[offset], decoded[offset], decoded[offset + 1]),
        6 => (decoded[offset], decoded[offset + 1], decoded[offset + 2], decoded[offset + 3]),
        _ => throw new InvalidDataException("Legacy PNG color type is unsupported.")
    };

    private static (byte Red, byte Green, byte Blue, byte Alpha) ReadPalette(
        int index,
        byte[] palette,
        byte[] transparency)
    {
        var offset = checked(index * 3);
        if (offset + 2 >= palette.Length)
        {
            throw new InvalidDataException("Legacy PNG palette index is out of range.");
        }

        return (
            palette[offset],
            palette[offset + 1],
            palette[offset + 2],
            index < transparency.Length ? transparency[index] : (byte)255);
    }

    private static uint ComputeCrc(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        crc = UpdateCrc(crc, type);
        crc = UpdateCrc(crc, data);
        return crc ^ uint.MaxValue;
    }

    private static uint UpdateCrc(uint crc, ReadOnlySpan<byte> bytes)
    {
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }

        return crc;
    }
}
