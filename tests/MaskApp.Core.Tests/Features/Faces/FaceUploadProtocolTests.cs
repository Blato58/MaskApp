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
    public void BuildPayload_UsesJavaStaticImageCanvasWithoutLedPrefix()
    {
        var pattern = CreatePattern(
            (0, 0, new FaceColor(0x01, 0x02, 0x03)),
            (0, 1, new FaceColor(0x04, 0x05, 0x06)),
            (1, 0, new FaceColor(0x07, 0x08, 0x09)),
            (35, 11, new FaceColor(0xFA, 0xCC, 0x15)));

        var payload = FaceUploadProtocol.BuildPayload(pattern);

        Assert.Equal(FaceUploadProtocol.PayloadLength, payload.Length);
        Assert.Equal(FaceUploadProtocol.StaticImagePixelCount * 3, payload.Length);
        Assert.Equal([0x01, 0x02, 0x03], payload.Take(3).ToArray());
        Assert.Equal([0x04, 0x05, 0x06], payload.Skip(5 * 3).Take(3).ToArray());
        Assert.Equal([0x07, 0x08, 0x09], payload.Skip(FaceUploadProtocol.StaticImageHeight * 3).Take(3).ToArray());

        var lastPixelOffset = ((FaceUploadProtocol.StaticImageWidth - 1) * FaceUploadProtocol.StaticImageHeight
            + (FaceUploadProtocol.StaticImageHeight - 1)) * 3;
        Assert.Equal([0xFA, 0xCC, 0x15], payload.Skip(lastPixelOffset).Take(3).ToArray());
    }

    [Fact]
    public void CreatePackage_BuildsDatsFramesDatcpAndPlayCommand()
    {
        var pattern = FacePatternFactory.CreateBuiltIns().Single(face => face.Emotion == FaceEmotion.Happy);

        var package = FaceUploadProtocol.CreatePackage(pattern, slot: 4, finishTimestamp: 0x01020304);

        Assert.Equal(4, package.Slot);
        Assert.Equal(FaceUploadProtocol.PayloadLength, package.Payload.Length);
        Assert.Equal(82, package.Frames.Count);
        Assert.Equal(100, package.Frames[0].Data.Length);
        Assert.Equal(0x63, package.Frames[0].Data.Span[0]);
        Assert.Equal(0, package.Frames[0].Data.Span[1]);
        Assert.Equal(package.Payload.Take(98).ToArray(), package.Frames[0].Data.Span.Slice(2, 98).ToArray());
        var lastFrame = package.Frames[^1];
        Assert.Equal(81, lastFrame.Index);
        Assert.Equal(100, lastFrame.Data.Length);
        Assert.Equal(0x43, lastFrame.Data.Span[0]);
        Assert.Equal(81, lastFrame.Data.Span[1]);
        Assert.Equal(package.Payload.Skip(98 * 81).ToArray(), lastFrame.Data.Span.Slice(2, 66).ToArray());
        Assert.All(lastFrame.Data.Span.Slice(68).ToArray(), value => Assert.Equal(0, value));
        Assert.Equal(MaskCommandKind.FaceUploadStart, package.StartCommand.Kind);
        Assert.Equal(Convert.FromHexString("09444154531F44000401000000000000"), package.StartCommand.Plaintext.ToArray());
        Assert.Equal(MaskCommandKind.FaceUploadFinish, package.FinishCommand.Kind);
        Assert.Equal(Convert.FromHexString("09444154435001020304000000000000"), package.FinishCommand.Plaintext.ToArray());
        Assert.Equal(MaskCommandKind.FacePlay, package.PlayCommand.Kind);
        Assert.Equal(Convert.FromHexString("06504C41590104000000000000000000"), package.PlayCommand.Plaintext.ToArray());
    }

    [Fact]
    public void CreatePackage_DefaultsFinishTimestampToCurrentUnixTime()
    {
        var pattern = FacePatternFactory.CreateBuiltIns().Single(face => face.Emotion == FaceEmotion.Sad);
        var before = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var package = FaceUploadProtocol.CreatePackage(pattern, slot: 2);

        var after = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var plaintext = package.FinishCommand.Plaintext.Span;
        var timestamp = ((uint)plaintext[6] << 24)
            | ((uint)plaintext[7] << 16)
            | ((uint)plaintext[8] << 8)
            | plaintext[9];

        Assert.InRange(timestamp, before, after);
        Assert.NotEqual(0u, timestamp);
    }

    [Fact]
    public void FaceUploadOptions_WaitBeforeAutoPlayToAllowSlotCommit()
    {
        Assert.True(FaceUploadOptions.RequireAcknowledgements.DeleteSlotBeforeUpload);
        Assert.True(FaceUploadOptions.WriteOnlyCompatibility.DeleteSlotBeforeUpload);
        Assert.Equal(TimeSpan.FromMilliseconds(500), FaceUploadOptions.RequireAcknowledgements.PreUploadDeleteDelay);
        Assert.Equal(TimeSpan.FromMilliseconds(500), FaceUploadOptions.WriteOnlyCompatibility.PreUploadDeleteDelay);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), FaceUploadOptions.RequireAcknowledgements.DeleteAcknowledgementTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), FaceUploadOptions.WriteOnlyCompatibility.DeleteAcknowledgementTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(1000), FaceUploadOptions.RequireAcknowledgements.PostUploadDelay);
        Assert.Equal(TimeSpan.FromMilliseconds(1000), FaceUploadOptions.WriteOnlyCompatibility.PostUploadDelay);
    }

    [Fact]
    public void BuildDeleteCommand_UsesJavaDeleShapeForSelectedSlot()
    {
        var command = FaceUploadProtocol.BuildDeleteCommand([4]);

        Assert.Equal(MaskCommandKind.FaceDelete, command.Kind);
        Assert.Equal(Convert.FromHexString("0644454C450104000000000000000000"), command.Plaintext.ToArray());
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
        return CreatePattern(litPixels
            .Select(pixel => (pixel.Column, pixel.Row, new FaceColor(0xFA, 0xCC, 0x15)))
            .ToArray());
    }

    private static FacePattern CreatePattern(params (int Column, int Row, FaceColor Color)[] litPixels)
    {
        var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        foreach (var (column, row, color) in litPixels)
        {
            pixels[(row * FacePattern.Width) + column] = new FacePixel(true, color);
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
