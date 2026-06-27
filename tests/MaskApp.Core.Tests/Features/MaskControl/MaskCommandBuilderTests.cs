using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.MaskControl;

public sealed class MaskCommandBuilderTests
{
    [Theory]
    [InlineData(100, "064C4947485464000000000000000000", "D5706131FCA286F292398302ED7E4FB7")]
    [InlineData(1, "064C4947485401000000000000000000", "90E7002373DE97320164D3511751579E")]
    public void Brightness_BuildsEncryptedLightCommand(int brightness, string expectedPlaintextHex, string expectedEncryptedHex)
    {
        var command = MaskCommandBuilder.Brightness(brightness);

        Assert.Equal(MaskCommandKind.Brightness, command.Kind);
        Assert.Equal(Convert.FromHexString(expectedPlaintextHex), command.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString(expectedEncryptedHex), command.EncryptedPayload.ToArray());
    }

    [Fact]
    public void Brightness_ClampsToMaskSupportedRange()
    {
        var tooLow = MaskCommandBuilder.Brightness(-10);
        var tooHigh = MaskCommandBuilder.Brightness(200);

        Assert.Equal(Convert.FromHexString("064C4947485401000000000000000000"), tooLow.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString("064C4947485464000000000000000000"), tooHigh.Plaintext.ToArray());
    }

    [Fact]
    public void Animation_BuildsEncryptedAnimCommand()
    {
        var command = MaskCommandBuilder.Animation(1);

        Assert.Equal(MaskCommandKind.Animation, command.Kind);
        Assert.Equal(Convert.FromHexString("05414E494D0100000000000000000000"), command.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString("7413B642E985EBE42C45597D00631389"), command.EncryptedPayload.ToArray());
    }

    [Fact]
    public void Image_BuildsEncryptedImageCommand()
    {
        var command = MaskCommandBuilder.Image(1);

        Assert.Equal(MaskCommandKind.Image, command.Kind);
        Assert.Equal(Convert.FromHexString("05494D41470100000000000000000000"), command.Plaintext.ToArray());
        Assert.Equal(Convert.FromHexString("5E98F314C612E094B6D47669432AF7FC"), command.EncryptedPayload.ToArray());
    }
}
