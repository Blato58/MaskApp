using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class FaceUploadProtocolTests
{
    [Fact]
    public void PackLedBits_UsesJavaColumnBitOrderForThirtySixByTwelveFaces()
    {
        var pattern = CreatePattern(
            (0, 0),
            (0, 7),
            (0, 8),
            (0, 11),
            (1, 1));

        var ledData = FaceUploadProtocol.PackLedBits(pattern);

        Assert.Equal(FaceUploadProtocol.LedDataLength, ledData.Length);
        Assert.Equal(0x81, ledData[0]);
        Assert.Equal(0x90, ledData[1]);
        Assert.Equal(0x40, ledData[2]);
        Assert.Equal(0x00, ledData[3]);
    }

    [Fact]
    public void BuildPayload_AppendsRgbTripletsForEveryGridCell()
    {
        var pattern = CreatePattern((2, 0));

        var payload = FaceUploadProtocol.BuildPayload(pattern);

        Assert.Equal(FaceUploadProtocol.PayloadLength, payload.Length);
        Assert.Equal(FaceUploadProtocol.LedDataLength, payload.Take(FaceUploadProtocol.LedDataLength).Count());
        var colorOffset = FaceUploadProtocol.LedDataLength + (2 * 3);
        Assert.Equal([0xFA, 0xCC, 0x15], payload.Skip(colorOffset).Take(3).ToArray());
        Assert.Equal([0x00, 0x00, 0x00], payload.Skip(FaceUploadProtocol.LedDataLength).Take(3).ToArray());
    }

    [Fact]
    public void CreatePackage_BuildsDatsFramesDatcpAndPlayCommand()
    {
        var pattern = FacePatternFactory.CreateBuiltIns().Single(face => face.Emotion == FaceEmotion.Happy);

        var package = FaceUploadProtocol.CreatePackage(pattern, slot: 4, finishTimestamp: 0x01020304);

        Assert.Equal(4, package.Slot);
        Assert.Equal(FaceUploadProtocol.PayloadLength, package.Payload.Length);
        Assert.Equal(76, package.Frames.Count);
        Assert.Equal(20, package.Frames[0].Data.Length);
        Assert.Equal(19, package.Frames[0].Data.Span[0]);
        Assert.Equal(0, package.Frames[0].Data.Span[1]);
        Assert.Equal(MaskCommandKind.FaceUploadStart, package.StartCommand.Kind);
        Assert.Equal(Convert.FromHexString("09444154530558000401000000000000"), package.StartCommand.Plaintext.ToArray());
        Assert.Equal(MaskCommandKind.FaceUploadFinish, package.FinishCommand.Kind);
        Assert.Equal(Convert.FromHexString("09444154435001020304000000000000"), package.FinishCommand.Plaintext.ToArray());
        Assert.Equal(MaskCommandKind.FacePlay, package.PlayCommand.Kind);
        Assert.Equal(Convert.FromHexString("06504C41590104000000000000000000"), package.PlayCommand.Plaintext.ToArray());
    }

    [Theory]
    [InlineData("07444154534F4B000000000000000000", FaceUploadAcknowledgement.StartAccepted)]
    [InlineData("0452454F4B0000000000000000000000", FaceUploadAcknowledgement.FrameAccepted)]
    [InlineData("0844415443504F4B0000000000000000", FaceUploadAcknowledgement.Complete)]
    [InlineData("07464143454F4B000000000000000000", FaceUploadAcknowledgement.Complete)]
    [InlineData("07504C41594F4B000000000000000000", FaceUploadAcknowledgement.PlayAccepted)]
    [InlineData("0744454C454F4B000000000000000000", FaceUploadAcknowledgement.DeleteAccepted)]
    [InlineData("054552524F5200000000000000000000", FaceUploadAcknowledgement.Error)]
    public void ParsePlaintextAcknowledgement_MapsKnownFaceResponses(string plaintextHex, FaceUploadAcknowledgement expected)
    {
        var acknowledgement = FaceUploadProtocol.ParsePlaintextAcknowledgement(Convert.FromHexString(plaintextHex));

        Assert.Equal(expected, acknowledgement);
    }

    private static FacePattern CreatePattern(params (int Column, int Row)[] litPixels)
    {
        var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        foreach (var (column, row) in litPixels)
        {
            pixels[(row * FacePattern.Width) + column] = new FacePixel(true, new FaceColor(0xFA, 0xCC, 0x15));
        }

        return new FacePattern
        {
            Id = "test-face",
            DisplayName = "Test Face",
            Pixels = pixels,
            PreferredSlot = 4
        }.Normalize();
    }
}
