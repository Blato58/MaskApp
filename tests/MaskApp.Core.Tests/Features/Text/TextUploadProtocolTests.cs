using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Text;

public sealed class TextUploadProtocolTests
{
    [Fact]
    public void BuildPayload_AppendsRgbColorBytesForEachColumn()
    {
        byte[] ledData = [0x12, 0x34, 0xAB, 0xCD];
        TextLedColor[] colors = [new(1, 2, 3), new(4, 5, 6)];

        var payload = TextUploadProtocol.BuildPayload(ledData, colors);

        Assert.Equal(Convert.FromHexString("1234ABCD010203040506"), payload);
    }

    [Fact]
    public void BuildStartCommand_UsesJavaDatsShapeAndEncryptsBlock()
    {
        var command = TextUploadProtocol.BuildStartCommand(30, 12);

        Assert.Equal(MaskCommandKind.TextUploadStart, command.Kind);
        Assert.Equal(Convert.FromHexString("0944415453001E000C00000000000000"), command.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString("0BDB89227BE710A5B1DA48712C5F68AD"), command.EncryptedPayload.ToArray());
    }

    [Fact]
    public void BuildFinishCommand_UsesJavaDatcpShapeAndEncryptsBlock()
    {
        var command = TextUploadProtocol.BuildFinishCommand();

        Assert.Equal(MaskCommandKind.TextUploadFinish, command.Kind);
        Assert.Equal(Convert.FromHexString("05444154435000000000000000000000"), command.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString("E799AD01AA48AE0AEE0B7203E8EDE520"), command.EncryptedPayload.ToArray());
    }

    [Fact]
    public void SplitFrames_UsesDefaultEighteenByteChunksWithJavaHeader()
    {
        var payload = Enumerable.Range(1, 20).Select(value => (byte)value).ToArray();

        var frames = TextUploadProtocol.SplitFrames(payload, TextUploadProtocol.DefaultFramePayloadLength);

        Assert.Equal(2, frames.Count);
        Assert.Equal(0, frames[0].Index);
        Assert.Equal(20, frames[0].Data.Length);
        Assert.Equal(19, frames[0].Data.Span[0]);
        Assert.Equal(0, frames[0].Data.Span[1]);
        Assert.Equal(payload.Take(18).ToArray(), frames[0].Data.Span.Slice(2, 18).ToArray());
        Assert.Equal(3, frames[1].Data.Span[0]);
        Assert.Equal(1, frames[1].Data.Span[1]);
        Assert.Equal(payload.Skip(18).ToArray(), frames[1].Data.Span.Slice(2, 2).ToArray());
    }

    [Fact]
    public void SplitFrames_UsesLargeMtuChunksWhenRequested()
    {
        var payload = Enumerable.Range(0, 120).Select(value => (byte)value).ToArray();

        var frames = TextUploadProtocol.SplitFrames(payload, TextUploadProtocol.LargeMtuFramePayloadLength);

        Assert.Equal(2, frames.Count);
        Assert.Equal(100, frames[0].Data.Length);
        Assert.Equal(99, frames[0].Data.Span[0]);
        Assert.Equal(23, frames[1].Data.Span[0]);
    }

    [Theory]
    [InlineData("07444154534F4B000000000000000000", TextUploadAcknowledgement.StartAccepted)]
    [InlineData("0452454F4B0000000000000000000000", TextUploadAcknowledgement.FrameAccepted)]
    [InlineData("0844415443504F4B0000000000000000", TextUploadAcknowledgement.Complete)]
    [InlineData("054552524F5200000000000000000000", TextUploadAcknowledgement.Error)]
    public void ParsePlaintextAcknowledgement_MapsKnownResponses(string plaintextHex, TextUploadAcknowledgement expected)
    {
        var acknowledgement = TextUploadProtocol.ParsePlaintextAcknowledgement(Convert.FromHexString(plaintextHex));

        Assert.Equal(expected, acknowledgement);
    }

    [Fact]
    public void CreatePackage_BuildsPayloadFramesAndPostUploadCommands()
    {
        var package = TextUploadProtocol.CreatePackage("A", new TextLedColor(1, 2, 3), mode: 2, speed: 25);

        Assert.Equal("A", package.Text);
        Assert.Equal(6, package.ColumnCount);
        Assert.Equal(30, package.Payload.Length);
        Assert.Equal(2, package.Frames.Count);
        Assert.Equal(MaskCommandKind.TextMode, package.ModeCommand.Kind);
        Assert.Equal(Convert.FromHexString("05444F4E450200000000000000000000"), package.ModeCommand.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString("C0D1BA638B32C3B1199C52C4E453E65E"), package.ModeCommand.EncryptedPayload.ToArray());
        Assert.Equal(MaskCommandKind.TextSpeed, package.SpeedCommand.Kind);
        Assert.Equal(Convert.FromHexString("06535045454419000000000000000000"), package.SpeedCommand.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString("715837946F17DFFDBDEF98FAB3F47A51"), package.SpeedCommand.EncryptedPayload.ToArray());
    }
}
