using MaskApp.Core.Features.Audio;

namespace MaskApp.Core.Tests.Features.Audio;

public sealed class AudioVisualizationProtocolTests
{
    private static readonly byte[] PaletteLevels =
        [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 2, 1, 4, 3];

    [Fact]
    public void FirmwareLengthPalettePacket_MatchesDeterministicCipherFixture()
    {
        var packet = AudioVisualizationProtocol.BuildFromLevels(
            AudioVisualizationPackingMode.PaletteA,
            PaletteLevels,
            AudioVisualizationFraming.FirmwareLength);

        Assert.Equal("0D001032547698785634129012340000", Convert.ToHexString(packet.Plaintext));
        Assert.Equal("2F26D6EE2F3CAAC7A158F23B99DE8060", Convert.ToHexString(packet.EncryptedPayload));
        Assert.Equal(PaletteLevels, AudioVisualizationProtocol.ExpandForDiagnostics(packet));
    }

    [Fact]
    public void LegacyAndroidPacket_DeclaresFifteenBytes_AndHasStableCiphertext()
    {
        var packet = AudioVisualizationProtocol.BuildFromLevels(
            AudioVisualizationPackingMode.PaletteA,
            PaletteLevels,
            AudioVisualizationFraming.LegacyAndroidLength);

        Assert.Equal(15, packet.Plaintext[0]);
        Assert.Equal("0057951A44ED97176A824ACDED544260", Convert.ToHexString(packet.EncryptedPayload));
    }

    [Theory]
    [InlineData(AudioVisualizationPackingMode.PaletteA, 12, 13)]
    [InlineData(AudioVisualizationPackingMode.PaletteB, 12, 13)]
    [InlineData(AudioVisualizationPackingMode.DuplicatedPairs, 6, 7)]
    [InlineData(AudioVisualizationPackingMode.SpacedPairs, 4, 6)]
    public void FirmwarePackingModes_UseReverseEngineeredDeclaredLengths(
        AudioVisualizationPackingMode mode,
        int packedLength,
        int declaredLength)
    {
        var packet = AudioVisualizationProtocol.BuildPacked(
            mode,
            new byte[packedLength],
            AudioVisualizationFraming.FirmwareLength);

        Assert.Equal(declaredLength, packet.Plaintext[0]);
        Assert.Equal((byte)mode, packet.Plaintext[1]);
        Assert.Equal(16, packet.EncryptedPayload.Length);
    }

    [Fact]
    public void BuildPacked_RejectsFirmwareClampedNibbles()
    {
        var invalid = new byte[12];
        invalid[5] = 0xa0;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AudioVisualizationProtocol.BuildPacked(
                AudioVisualizationPackingMode.PaletteA,
                invalid,
                AudioVisualizationFraming.FirmwareLength));
    }

    [Fact]
    public void CompressedModes_ExpandToTwentyFourValues()
    {
        var duplicated = AudioVisualizationProtocol.BuildFromLevels(
            AudioVisualizationPackingMode.DuplicatedPairs,
            Enumerable.Range(0, 12).Select(index => (byte)(index % 10)).ToArray(),
            AudioVisualizationFraming.FirmwareLength);
        var spaced = AudioVisualizationProtocol.BuildFromLevels(
            AudioVisualizationPackingMode.SpacedPairs,
            [1, 2, 3, 4, 5, 6, 7, 8],
            AudioVisualizationFraming.FirmwareLength);

        Assert.Equal(24, AudioVisualizationProtocol.ExpandForDiagnostics(duplicated).Length);
        Assert.Equal(
            [1, 2, 0, 1, 2, 0, 3, 4, 0, 3, 4, 0, 5, 6, 0, 5, 6, 0, 7, 8, 0, 7, 8, 0],
            AudioVisualizationProtocol.ExpandForDiagnostics(spaced));
    }
}
