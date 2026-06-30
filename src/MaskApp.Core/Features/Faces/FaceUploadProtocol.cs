using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Faces;

public static class FaceUploadProtocol
{
    public const int StaticImageWidth = 46;
    public const int StaticImageHeight = 58;
    public const int StaticImagePixelCount = StaticImageWidth * StaticImageHeight;
    public const int LedDataLength = FacePattern.Width * 2;
    public const int ColorDataLength = StaticImagePixelCount * 3;
    public const int PayloadLength = ColorDataLength;
    public const int DefaultFramePayloadLength = 98;
    public const int LargeMtuFramePayloadLength = 98;

    public static FaceUploadPackage CreatePackage(
        FacePattern pattern,
        int slot,
        bool useLargeMtu = false,
        uint? finishTimestamp = null)
    {
        pattern = pattern.Normalize();
        slot = Math.Clamp(slot, FacePattern.MinSlot, FacePattern.MaxSlot);
        var ledData = PackLedBits(pattern);
        var payload = BuildPayload(pattern);
        var framePayloadLength = useLargeMtu ? LargeMtuFramePayloadLength : DefaultFramePayloadLength;
        var frames = SplitFrames(payload, framePayloadLength);
        var timestamp = finishTimestamp ?? GetCurrentUnixTimestamp();

        return new FaceUploadPackage(
            pattern,
            slot,
            ledData,
            payload,
            frames,
            BuildStartCommand(payload.Length, slot),
            BuildFinishCommand(timestamp),
            BuildPlayCommand([slot]));
    }

    public static byte[] PackLedBits(FacePattern pattern)
    {
        pattern = pattern.Normalize();
        var ledData = new byte[LedDataLength];
        var offset = 0;

        for (var column = 0; column < FacePattern.Width; column++)
        {
            byte top = 0;
            byte bottom = 0;
            for (var row = 0; row < FacePattern.Height; row++)
            {
                if (!pattern.GetPixel(column, row).IsLit)
                {
                    continue;
                }

                if (row < 8)
                {
                    top |= (byte)(0x80 >> row);
                }
                else
                {
                    bottom |= (byte)(0x80 >> (row - 8));
                }
            }

            ledData[offset++] = top;
            ledData[offset++] = bottom;
        }

        return ledData;
    }

    public static byte[] BuildPayload(FacePattern pattern)
    {
        pattern = pattern.Normalize();
        var payload = new byte[PayloadLength];
        var offset = 0;

        for (var imageColumn = 0; imageColumn < StaticImageWidth; imageColumn++)
        {
            var column = ScaleCoordinate(imageColumn, StaticImageWidth, FacePattern.Width);
            for (var imageRow = 0; imageRow < StaticImageHeight; imageRow++)
            {
                var row = ScaleCoordinate(imageRow, StaticImageHeight, FacePattern.Height);
                var pixel = pattern.GetPixel(column, row);
                var color = pixel.IsLit ? pixel.Color : FaceColor.Black;
                payload[offset++] = color.Red;
                payload[offset++] = color.Green;
                payload[offset++] = color.Blue;
            }
        }

        return payload;
    }

    private static int ScaleCoordinate(int coordinate, int sourceSize, int targetSize) =>
        Math.Clamp((int)Math.Floor((coordinate + 0.5) * targetSize / sourceSize), 0, targetSize - 1);

    public static IReadOnlyList<FaceUploadFrame> SplitFrames(ReadOnlySpan<byte> payload, int framePayloadLength)
    {
        if (framePayloadLength <= 0 || framePayloadLength > 253)
        {
            throw new ArgumentOutOfRangeException(nameof(framePayloadLength), "Frame payload length must be between 1 and 253.");
        }

        var frames = new List<FaceUploadFrame>();
        var offset = 0;
        var index = 0;

        while (offset < payload.Length)
        {
            var bytesInFrame = Math.Min(framePayloadLength, payload.Length - offset);
            var frame = new byte[framePayloadLength + 2];
            frame[0] = checked((byte)(bytesInFrame + 1));
            frame[1] = checked((byte)index);
            payload.Slice(offset, bytesInFrame).CopyTo(frame.AsSpan(2));
            frames.Add(new FaceUploadFrame(index, frame));
            offset += bytesInFrame;
            index++;
        }

        return frames;
    }

    public static MaskCommand BuildStartCommand(int payloadLength, int slot)
    {
        var payloadLengthBytes = ToBigEndianUInt16(payloadLength);
        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = 9;
        plaintext[1] = (byte)'D';
        plaintext[2] = (byte)'A';
        plaintext[3] = (byte)'T';
        plaintext[4] = (byte)'S';
        plaintext[5] = payloadLengthBytes[0];
        plaintext[6] = payloadLengthBytes[1];
        plaintext[7] = 0;
        plaintext[8] = checked((byte)Math.Clamp(slot, FacePattern.MinSlot, FacePattern.MaxSlot));
        plaintext[9] = 1;
        return new MaskCommand(
            MaskCommandKind.FaceUploadStart,
            $"Face upload slot {slot} ({payloadLength} bytes)",
            plaintext,
            MaskProtocolCrypto.EncryptBlock(plaintext));
    }

    public static MaskCommand BuildFinishCommand(uint timestamp)
    {
        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = 9;
        plaintext[1] = (byte)'D';
        plaintext[2] = (byte)'A';
        plaintext[3] = (byte)'T';
        plaintext[4] = (byte)'C';
        plaintext[5] = (byte)'P';
        plaintext[6] = (byte)(timestamp >> 24);
        plaintext[7] = (byte)(timestamp >> 16);
        plaintext[8] = (byte)(timestamp >> 8);
        plaintext[9] = (byte)timestamp;
        return new MaskCommand(
            MaskCommandKind.FaceUploadFinish,
            "Face upload finish",
            plaintext,
            MaskProtocolCrypto.EncryptBlock(plaintext));
    }

    public static MaskCommand BuildPlayCommand(IReadOnlyList<int> slots)
    {
        if (slots.Count == 0 || slots.Count > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(slots), "Face play command supports 1 to 10 slots in v1.");
        }

        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = checked((byte)(slots.Count + 5));
        plaintext[1] = (byte)'P';
        plaintext[2] = (byte)'L';
        plaintext[3] = (byte)'A';
        plaintext[4] = (byte)'Y';
        plaintext[5] = checked((byte)slots.Count);
        for (var i = 0; i < slots.Count; i++)
        {
            plaintext[6 + i] = checked((byte)Math.Clamp(slots[i], FacePattern.MinSlot, FacePattern.MaxSlot));
        }

        return new MaskCommand(
            MaskCommandKind.FacePlay,
            slots.Count == 1 ? $"Play DIY face slot {slots[0]}" : $"Play {slots.Count} DIY face slots",
            plaintext,
            MaskProtocolCrypto.EncryptBlock(plaintext));
    }

    public static MaskCommand BuildDeleteCommand(IReadOnlyList<int> slots)
    {
        if (slots.Count == 0 || slots.Count > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(slots), "Face delete command supports 1 to 10 slots in v1.");
        }

        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = checked((byte)(slots.Count + 5));
        plaintext[1] = (byte)'D';
        plaintext[2] = (byte)'E';
        plaintext[3] = (byte)'L';
        plaintext[4] = (byte)'E';
        plaintext[5] = checked((byte)slots.Count);
        for (var i = 0; i < slots.Count; i++)
        {
            plaintext[6 + i] = checked((byte)Math.Clamp(slots[i], FacePattern.MinSlot, FacePattern.MaxSlot));
        }

        return new MaskCommand(
            MaskCommandKind.FaceDelete,
            slots.Count == 1 ? $"Delete DIY face slot {slots[0]}" : $"Delete {slots.Count} DIY face slots",
            plaintext,
            MaskProtocolCrypto.EncryptBlock(plaintext));
    }

    public static MaskCommand BuildCheckCommand()
    {
        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = 4;
        plaintext[1] = (byte)'C';
        plaintext[2] = (byte)'H';
        plaintext[3] = (byte)'E';
        plaintext[4] = (byte)'C';
        return new MaskCommand(
            MaskCommandKind.FaceCheck,
            "Check DIY face slots",
            plaintext,
            MaskProtocolCrypto.EncryptBlock(plaintext));
    }

    public static FaceUploadAcknowledgement ParseEncryptedAcknowledgement(ReadOnlySpan<byte> encrypted)
    {
        if (encrypted.Length != MaskBleProtocol.CommandLength)
        {
            return ParsePlaintextAcknowledgement(encrypted);
        }

        var decrypted = ParsePlaintextAcknowledgement(MaskProtocolCrypto.DecryptBlock(encrypted));
        return decrypted == FaceUploadAcknowledgement.Unknown
            ? ParsePlaintextAcknowledgement(encrypted)
            : decrypted;
    }

    public static FaceUploadAcknowledgement ParsePlaintextAcknowledgement(ReadOnlySpan<byte> plaintext)
    {
        if (plaintext.IsEmpty)
        {
            return FaceUploadAcknowledgement.Unknown;
        }

        if (Matches(plaintext, "DATOK") || Matches(plaintext, "DATSOK"))
        {
            return FaceUploadAcknowledgement.StartAccepted;
        }

        if (Matches(plaintext, "REOK") || Matches(plaintext, "REOKOK"))
        {
            return FaceUploadAcknowledgement.FrameAccepted;
        }

        if (Matches(plaintext, "DATCPOK") || Matches(plaintext, "FACEOK"))
        {
            return FaceUploadAcknowledgement.Complete;
        }

        if (Matches(plaintext, "PLAYOK"))
        {
            return FaceUploadAcknowledgement.PlayAccepted;
        }

        if (Matches(plaintext, "DELEOK"))
        {
            return FaceUploadAcknowledgement.DeleteAccepted;
        }

        if (Matches(plaintext, "CHEC"))
        {
            return FaceUploadAcknowledgement.CheckResponse;
        }

        if (Matches(plaintext, "ERROR"))
        {
            return FaceUploadAcknowledgement.Error;
        }

        return FaceUploadAcknowledgement.Unknown;
    }

    private static bool Matches(ReadOnlySpan<byte> plaintext, string command) =>
        MatchesAt(plaintext, command, 1) || MatchesAt(plaintext, command, 0);

    private static bool MatchesAt(ReadOnlySpan<byte> plaintext, string command, int offset)
    {
        if (plaintext.Length < command.Length + offset)
        {
            return false;
        }

        for (var i = 0; i < command.Length; i++)
        {
            if (plaintext[i + offset] != (byte)command[i])
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
            throw new ArgumentOutOfRangeException(nameof(value), "Face payload lengths must fit in two bytes.");
        }

        return [(byte)(value / 256), (byte)(value % 256)];
    }

    private static uint GetCurrentUnixTimestamp() =>
        checked((uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
}
