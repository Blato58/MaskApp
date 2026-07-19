using System.Text;
using System.Text.Json;
using MaskApp.Core.Features.WatchRemote;

namespace MaskApp.Core.Tests.Features.WatchRemote;

public sealed class WatchRemoteMessageCodecTests
{
    private readonly WatchRemoteMessageCodec codec = new();

    [Fact]
    public void Envelope_RoundTripsAsStrictCamelCaseJson()
    {
        var envelope = CreateEnvelope(WatchRemoteActionKind.SetBrightness) with
        {
            Action = new WatchRemoteAction
            {
                Kind = WatchRemoteActionKind.SetBrightness,
                Brightness = 67
            }
        };

        var payload = codec.EncodeEnvelope(envelope);
        var decoded = codec.DecodeEnvelope(payload);

        Assert.Equal(envelope.MessageId, decoded.MessageId);
        Assert.Equal(WatchRemoteActionKind.SetBrightness, decoded.Action?.Kind);
        Assert.Equal(67, decoded.Action?.Brightness);
        Assert.Contains("\"senderInstanceId\"", Encoding.UTF8.GetString(payload), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{\"schemaVersion\":1,\"messageId\":\"11111111-1111-1111-1111-111111111111\",\"senderInstanceId\":\"watch\",\"sequence\":1,\"sentAt\":\"2026-01-01T00:00:00Z\",\"action\":{\"kind\":1}}")]
    [InlineData("{\"schemaVersion\":1,\"messageId\":\"11111111-1111-1111-1111-111111111111\",\"senderInstanceId\":\"watch\",\"sequence\":1,\"sentAt\":\"2026-01-01T00:00:00Z\",\"action\":{}}")]
    [InlineData("{\"schemaVersion\":1,\"messageId\":\"11111111-1111-1111-1111-111111111111\",\"senderInstanceId\":\"watch\",\"sequence\":1,\"sentAt\":\"2026-01-01T00:00:00Z\"}")]
    [InlineData("{\"schemaVersion\":1,\"messageId\":\"11111111-1111-1111-1111-111111111111\",\"senderInstanceId\":\"watch\",\"sequence\":1,\"sentAt\":\"2026-01-01T00:00:00Z\",\"action\":{\"kind\":\"Stop\"},\"unexpected\":true}")]
    public void DecodeEnvelope_RejectsNumericEnumsMissingFieldsAndUnknownFields(string json)
    {
        Assert.Throws<JsonException>(() => codec.DecodeEnvelope(Encoding.UTF8.GetBytes(json)));
    }

    [Fact]
    public void DecodeEnvelope_RejectsEmptyAndOversizedPayloads()
    {
        Assert.Throws<InvalidDataException>(() => codec.DecodeEnvelope([]));
        Assert.Throws<InvalidDataException>(() =>
            codec.DecodeEnvelope(new byte[WatchRemoteMessageCodec.MaximumMessageBytes + 1]));
    }

    private static WatchRemoteEnvelope CreateEnvelope(WatchRemoteActionKind kind) => new()
    {
        MessageId = Guid.NewGuid(),
        SenderInstanceId = "watch-installation",
        Sequence = 1,
        SentAt = DateTimeOffset.UtcNow,
        Action = new WatchRemoteAction { Kind = kind }
    };
}
