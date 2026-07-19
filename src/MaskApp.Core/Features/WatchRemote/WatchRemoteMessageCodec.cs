using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.WatchRemote;

public sealed class WatchRemoteMessageCodec
{
    public const int MaximumMessageBytes = 16 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public byte[] EncodeEnvelope(WatchRemoteEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        return EnsureBounded(JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions));
    }

    public WatchRemoteEnvelope DecodeEnvelope(ReadOnlySpan<byte> payload)
    {
        EnsureBounded(payload);
        return JsonSerializer.Deserialize<WatchRemoteEnvelope>(payload, JsonOptions)
            ?? throw new InvalidDataException("Watch action payload was null.");
    }

    public byte[] EncodeResult(WatchRemoteProcessResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return EnsureBounded(JsonSerializer.SerializeToUtf8Bytes(result, JsonOptions));
    }

    public WatchRemoteProcessResult DecodeResult(ReadOnlySpan<byte> payload)
    {
        EnsureBounded(payload);
        return JsonSerializer.Deserialize<WatchRemoteProcessResult>(payload, JsonOptions)
            ?? throw new InvalidDataException("Watch action result payload was null.");
    }

    public byte[] EncodeState(WatchRemoteState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return EnsureBounded(JsonSerializer.SerializeToUtf8Bytes(state, JsonOptions));
    }

    public WatchRemoteState DecodeState(ReadOnlySpan<byte> payload)
    {
        EnsureBounded(payload);
        return JsonSerializer.Deserialize<WatchRemoteState>(payload, JsonOptions)
            ?? throw new InvalidDataException("Watch state payload was null.");
    }

    private static byte[] EnsureBounded(byte[] payload)
    {
        EnsureBounded(payload.AsSpan());
        return payload;
    }

    private static void EnsureBounded(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0)
        {
            throw new InvalidDataException("Watch message payload is empty.");
        }

        if (payload.Length > MaximumMessageBytes)
        {
            throw new InvalidDataException(
                $"Watch message payload exceeds the {MaximumMessageBytes}-byte limit.");
        }
    }
}
