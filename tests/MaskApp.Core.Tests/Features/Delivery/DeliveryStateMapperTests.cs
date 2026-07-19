using MaskApp.Core.Features.Delivery;

namespace MaskApp.Core.Tests.Features.Delivery;

public sealed class DeliveryStateMapperTests
{
    [Theory]
    [InlineData("Write-only transport completed.")]
    [InlineData("Command sent, confirm on mask")]
    [InlineData("Uploaded to slot 4")]
    public void SuccessfulWriteWithoutAck_IsWritten(string message)
    {
        var result = DeliveryStateMapper.FromResult(true, message);

        Assert.Equal(DeliveryState.Written, result.State);
    }

    [Theory]
    [InlineData("ACK confirmed by device")]
    [InlineData("Observable response acknowledged")]
    public void AckEvidence_IsConfirmed(string message)
    {
        var result = DeliveryStateMapper.FromResult(true, message);

        Assert.Equal(DeliveryState.Confirmed, result.State);
    }

    [Fact]
    public void SuccessfulResultWithoutEvidence_IsUnknown()
    {
        var result = DeliveryStateMapper.FromResult(true, "Operation complete");

        Assert.Equal(DeliveryState.Unknown, result.State);
    }

    [Fact]
    public void FailedResult_IsFailedEvenWhenMessageMentionsAck()
    {
        var result = DeliveryStateMapper.FromResult(false, "ACK timeout");

        Assert.Equal(DeliveryState.Failed, result.State);
    }
}
