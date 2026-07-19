using MaskApp.Core.Features.Audio;

namespace MaskApp.Core.Features.Profiles;

public sealed record MaskProfile
{
    public string ProfileId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "LED Mask";

    public DateTimeOffset FirstSeenAt { get; init; }

    public DateTimeOffset LastSeenAt { get; init; }

    public MaskCapabilitySnapshot Capabilities { get; init; } = new();

    public IReadOnlyList<MaskPreparedSlot> PreparedSlots { get; init; } = [];

    public double? AverageCommandLatencyMilliseconds { get; init; }

    public double? SustainableCadenceHz { get; init; }

    public AudioVisualizationEvidence AudioVisualizationEvidence { get; init; } = new();

    public string PreparedStateStatus { get; init; } = "No prepared slots recorded.";

    public MaskProfile Normalize()
    {
        var normalizedCapabilities = (Capabilities ?? new MaskCapabilitySnapshot()).Normalize();
        var firstSeen = FirstSeenAt == default
            ? (LastSeenAt == default ? DateTimeOffset.UtcNow : LastSeenAt)
            : FirstSeenAt;
        var lastSeen = LastSeenAt == default ? firstSeen : LastSeenAt;

        return this with
        {
            ProfileId = ProfileId?.Trim() ?? string.Empty,
            DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? "LED Mask" : DisplayName.Trim(),
            FirstSeenAt = firstSeen,
            LastSeenAt = lastSeen,
            Capabilities = normalizedCapabilities,
            PreparedSlots = (PreparedSlots ?? [])
                .Select(slot => slot.Normalize())
                .Where(slot => !string.IsNullOrWhiteSpace(slot.ContentFingerprint))
                .GroupBy(slot => slot.Slot)
                .Select(group => group.OrderByDescending(slot => slot.InstalledAt).First())
                .OrderBy(slot => slot.Slot)
                .ToArray(),
            AverageCommandLatencyMilliseconds = NormalizePositiveMetric(AverageCommandLatencyMilliseconds),
            SustainableCadenceHz = NormalizePositiveMetric(SustainableCadenceHz),
            AudioVisualizationEvidence = (AudioVisualizationEvidence ?? new AudioVisualizationEvidence()).Normalize(),
            PreparedStateStatus = string.IsNullOrWhiteSpace(PreparedStateStatus)
                ? "No prepared slots recorded."
                : PreparedStateStatus.Trim()
        };
    }

    public MaskProfile ObserveCapabilities(MaskCapabilitySnapshot capabilities, DateTimeOffset timestamp)
    {
        var normalized = Normalize();
        var observed = capabilities.Normalize() with { ObservedAt = timestamp };
        var capabilitiesChanged = !string.IsNullOrWhiteSpace(normalized.Capabilities.TransportName)
            && !string.Equals(
                normalized.Capabilities.Fingerprint,
                observed.Fingerprint,
                StringComparison.Ordinal);

        return normalized with
        {
            Capabilities = observed,
            LastSeenAt = timestamp,
            PreparedSlots = capabilitiesChanged ? [] : normalized.PreparedSlots,
            PreparedStateStatus = capabilitiesChanged
                ? "Prepared slots invalidated because observed mask capabilities changed."
                : normalized.PreparedStateStatus
        };
    }

    private static double? NormalizePositiveMetric(double? value) =>
        value is > 0 and < double.MaxValue ? value : null;
}
