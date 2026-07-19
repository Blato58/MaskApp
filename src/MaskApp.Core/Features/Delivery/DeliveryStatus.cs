namespace MaskApp.Core.Features.Delivery;

public enum DeliveryState
{
    Idle,
    Sending,
    Preparing,
    Written,
    Confirmed,
    Failed,
    Unknown
}

public sealed record DeliveryStatus(DeliveryState State, string Label, string Detail)
{
    public static DeliveryStatus Idle { get; } = new(DeliveryState.Idle, "Idle", "Nothing is being sent.");

    public static DeliveryStatus Failed(string detail) =>
        new(DeliveryState.Failed, "Failed", detail);

    public static DeliveryStatus Unknown(string detail) =>
        new(DeliveryState.Unknown, "Unknown", detail);
}

public static class DeliveryStateMapper
{
    public static DeliveryStatus Sending(string itemName) =>
        new(DeliveryState.Sending, "Sending", $"Sending {itemName}.");

    public static DeliveryStatus Preparing(string itemName) =>
        new(DeliveryState.Preparing, "Preparing", $"Preparing {itemName} without playback.");

    public static DeliveryStatus FromResult(bool succeeded, string? message)
    {
        var detail = string.IsNullOrWhiteSpace(message) ? "No delivery detail was reported." : message.Trim();
        if (!succeeded)
        {
            return new DeliveryStatus(DeliveryState.Failed, "Failed", detail);
        }

        if (ContainsEvidence(detail, "confirmed", "acknowledged", " ack "))
        {
            return new DeliveryStatus(DeliveryState.Confirmed, "Confirmed", detail);
        }

        if (ContainsEvidence(detail, "written", "write-only", "sent", "uploaded", "play"))
        {
            return new DeliveryStatus(DeliveryState.Written, "Written", detail);
        }

        return new DeliveryStatus(DeliveryState.Unknown, "Unknown", detail);
    }

    private static bool ContainsEvidence(string value, params string[] terms)
    {
        var padded = $" {value} ";
        return terms.Any(term => padded.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
