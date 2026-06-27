using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Text;

public static class TextUploadProtocol
{
    public const int DefaultFramePayloadLength = 18;
    public const int LargeMtuFramePayloadLength = 98;

    public static TextUploadPackage CreatePackage(
        string text,
        TextLedColor color,
        int mode,
        int speed,
        bool useLargeMtu = false)
    {
        var ledData = TextGlyphRasterizer.Render(text);
        var columnCount = ledData.Length / 2;
        var payload = BuildPayload(ledData, Enumerable.Repeat(color, columnCount));
        var framePayloadLength = useLargeMtu ? LargeMtuFramePayloadLength : DefaultFramePayloadLength;
        var frames = SplitFrames(payload, framePayloadLength);

        return new TextUploadPackage(
            text,
            columnCount,
            ledData,
            payload,
            frames,
            BuildStartCommand(payload.Length, ledData.Length),
            BuildFinishCommand(),
            MaskCommandBuilder.TextMode(mode),
            MaskCommandBuilder.TextSpeed(speed));
    }

    public static byte[] BuildPayload(ReadOnlySpan<byte> ledData, IEnumerable<TextLedColor> colors)
    {
        if (ledData.Length % 2 != 0)
        {
            throw new ArgumentException("LED text data must contain two bytes per column.", nameof(ledData));
        }

        var colorArray = colors.ToArray();
        if (colorArray.Length != ledData.Length / 2)
        {
            throw new ArgumentException("The color count must match the LED column count.", nameof(colors));
        }

        var payload = new byte[ledData.Length + (colorArray.Length * 3)];
        ledData.CopyTo(payload);

        var offset = ledData.Length;
        foreach (var color in colorArray)
        {
            payload[offset++] = color.Red;
            payload[offset++] = color.Green;
            payload[offset++] = color.Blue;
        }

        return payload;
    }

    public static MaskCommand BuildStartCommand(int payloadLength, int textDataLength)
    {
        var payloadLengthBytes = ToBigEndianUInt16(payloadLength);
        var textDataLengthBytes = ToBigEndianUInt16(textDataLength);
        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = 9;
        plaintext[1] = (byte)'D';
        plaintext[2] = (byte)'A';
        plaintext[3] = (byte)'T';
        plaintext[4] = (byte)'S';
        plaintext[5] = payloadLengthBytes[0];
        plaintext[6] = payloadLengthBytes[1];
        plaintext[7] = textDataLengthBytes[0];
        plaintext[8] = textDataLengthBytes[1];
        return new MaskCommand(
            MaskCommandKind.TextUploadStart,
            $"Text upload {payloadLength} bytes",
            plaintext,
            MaskProtocolCrypto.EncryptBlock(plaintext));
    }

    public static MaskCommand BuildFinishCommand()
    {
        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = 5;
        plaintext[1] = (byte)'D';
        plaintext[2] = (byte)'A';
        plaintext[3] = (byte)'T';
        plaintext[4] = (byte)'C';
        plaintext[5] = (byte)'P';
        return new MaskCommand(
            MaskCommandKind.TextUploadFinish,
            "Text upload finish",
            plaintext,
            MaskProtocolCrypto.EncryptBlock(plaintext));
    }

    public static IReadOnlyList<TextUploadFrame> SplitFrames(ReadOnlySpan<byte> payload, int framePayloadLength)
    {
        if (framePayloadLength <= 0 || framePayloadLength > 253)
        {
            throw new ArgumentOutOfRangeException(nameof(framePayloadLength), "Frame payload length must be between 1 and 253.");
        }

        var frames = new List<TextUploadFrame>();
        var offset = 0;
        var index = 0;

        while (offset < payload.Length)
        {
            var bytesInFrame = Math.Min(framePayloadLength, payload.Length - offset);
            var frame = new byte[framePayloadLength + 2];
            frame[0] = checked((byte)(bytesInFrame + 1));
            frame[1] = checked((byte)index);
            payload.Slice(offset, bytesInFrame).CopyTo(frame.AsSpan(2));
            frames.Add(new TextUploadFrame(index, frame));
            offset += bytesInFrame;
            index++;
        }

        return frames;
    }

    public static TextUploadAcknowledgement ParseEncryptedAcknowledgement(ReadOnlySpan<byte> encrypted)
    {
        if (encrypted.Length != MaskBleProtocol.CommandLength)
        {
            return TextUploadAcknowledgement.Unknown;
        }

        return ParsePlaintextAcknowledgement(MaskProtocolCrypto.DecryptBlock(encrypted));
    }

    public static TextUploadAcknowledgement ParsePlaintextAcknowledgement(ReadOnlySpan<byte> plaintext)
    {
        if (plaintext.Length < MaskBleProtocol.CommandLength)
        {
            return TextUploadAcknowledgement.Unknown;
        }

        if (Matches(plaintext, "DATSOK"))
        {
            return TextUploadAcknowledgement.StartAccepted;
        }

        if (Matches(plaintext, "REOK"))
        {
            return TextUploadAcknowledgement.FrameAccepted;
        }

        if (Matches(plaintext, "DATCPOK"))
        {
            return TextUploadAcknowledgement.Complete;
        }

        if (Matches(plaintext, "ERROR"))
        {
            return TextUploadAcknowledgement.Error;
        }

        return TextUploadAcknowledgement.Unknown;
    }

    private static bool Matches(ReadOnlySpan<byte> plaintext, string command)
    {
        if (plaintext.Length < command.Length + 1)
        {
            return false;
        }

        for (var i = 0; i < command.Length; i++)
        {
            if (plaintext[i + 1] != (byte)command[i])
            {
                return false;
            }
        }

        return true;
    }

    private static byte[] ToBigEndianUInt16(int value)
    {
        if (value < 0 || value > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Text payload lengths must fit in two bytes.");
        }

        return [(byte)(value / 256), (byte)(value % 256)];
    }
}
