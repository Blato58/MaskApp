using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Profiles;

namespace MaskApp.Core.Features.Preflight;

public sealed class DiySlotAllocator
{
    public DiySlotAllocationResult Allocate(
        IReadOnlyList<DiySlotRequirement> requirements,
        MaskProfile? profile,
        int slotCapacity)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        var capacity = Math.Clamp(slotCapacity, 0, FacePattern.MaxSlot);
        var issues = new List<PreflightIssue>();
        var normalizedRequirements = requirements
            .Where(requirement =>
                !string.IsNullOrWhiteSpace(requirement.RequirementId)
                && !string.IsNullOrWhiteSpace(requirement.ContentFingerprint))
            .ToArray();
        var uniqueContent = normalizedRequirements
            .GroupBy(requirement => requirement.ContentFingerprint, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        if (uniqueContent.Length == 0)
        {
            return new DiySlotAllocationResult(true, [], []);
        }

        if (capacity == 0)
        {
            issues.Add(new PreflightIssue(
                "diy-capacity-unavailable",
                PreflightIssueSeverity.Blocking,
                "The active mask has no verified DIY-slot capacity.",
                "Reconnect the mask and rerun capability discovery."));
            return new DiySlotAllocationResult(false, [], issues);
        }

        if (uniqueContent.Length > capacity)
        {
            issues.Add(new PreflightIssue(
                "diy-capacity-exceeded",
                PreflightIssueSeverity.Blocking,
                $"The show needs {uniqueContent.Length} unique DIY frames but the mask exposes {capacity} slots.",
                "Remove frames, deduplicate content, or split the show into separately prepared sets."));
            return new DiySlotAllocationResult(false, [], issues);
        }

        var preparedSlots = profile?.Normalize().PreparedSlots
            .Where(slot => slot.Slot <= capacity)
            .ToArray() ?? [];
        var preparedByFingerprint = preparedSlots
            .GroupBy(slot => slot.ContentFingerprint, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(slot => slot.InstalledAt).First(),
                StringComparer.OrdinalIgnoreCase);
        var preparedBySlot = preparedSlots.ToDictionary(slot => slot.Slot);
        var assignedByFingerprint = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var assignedFingerprintsBySlot = new Dictionary<int, string>();

        foreach (var requirement in uniqueContent)
        {
            if (preparedByFingerprint.TryGetValue(requirement.ContentFingerprint, out var prepared)
                && !assignedFingerprintsBySlot.ContainsKey(prepared.Slot))
            {
                assignedByFingerprint[requirement.ContentFingerprint] = prepared.Slot;
                assignedFingerprintsBySlot[prepared.Slot] = requirement.ContentFingerprint;
                continue;
            }

            var preferredSlot = requirement.PreferredSlot is >= FacePattern.MinSlot
                && requirement.PreferredSlot <= capacity
                    ? requirement.PreferredSlot
                    : 0;
            var assignedSlot = preferredSlot > 0 && !assignedFingerprintsBySlot.ContainsKey(preferredSlot)
                ? preferredSlot
                : FindAvailableSlot(capacity, assignedFingerprintsBySlot, preparedBySlot);
            if (assignedSlot == 0)
            {
                issues.Add(new PreflightIssue(
                    "diy-allocation-failed",
                    PreflightIssueSeverity.Blocking,
                    $"No DIY slot could be allocated for {requirement.SourceId}.",
                    "Remove content or prepare a smaller show.",
                    requirement.SourceId));
                continue;
            }

            assignedByFingerprint[requirement.ContentFingerprint] = assignedSlot;
            assignedFingerprintsBySlot[assignedSlot] = requirement.ContentFingerprint;
            if (preferredSlot > 0 && assignedSlot != preferredSlot)
            {
                issues.Add(new PreflightIssue(
                    "diy-slot-remapped",
                    PreflightIssueSeverity.Blocking,
                    $"{requirement.SourceId} was remapped from DIY slot {preferredSlot} to {assignedSlot} to avoid a collision.",
                    "Resolve the collision before preparation; the current playback paths still require their preferred slots.",
                    requirement.SourceId));
            }
        }

        var allocations = normalizedRequirements
            .Where(requirement => assignedByFingerprint.ContainsKey(requirement.ContentFingerprint))
            .Select(requirement =>
            {
                var assignedSlot = assignedByFingerprint[requirement.ContentFingerprint];
                var prepared = preparedSlots.FirstOrDefault(slot =>
                    slot.Slot == assignedSlot
                    && string.Equals(
                        slot.ContentFingerprint,
                        requirement.ContentFingerprint,
                        StringComparison.OrdinalIgnoreCase));
                var replacedSourceId = prepared is null
                    && preparedBySlot.TryGetValue(assignedSlot, out var replaced)
                        ? replaced.SourceId
                        : string.Empty;
                if (!string.IsNullOrWhiteSpace(replacedSourceId))
                {
                    issues.Add(new PreflightIssue(
                        "diy-slot-replacement",
                        PreflightIssueSeverity.Warning,
                        $"Preparing {requirement.SourceId} will replace {replacedSourceId} in DIY slot {assignedSlot}.",
                        "Confirm preparation for this mask; the previous slot content will no longer be marked prepared.",
                        requirement.SourceId));
                }

                return new DiySlotAllocation(
                    requirement.RequirementId,
                    requirement.SourceId,
                    requirement.ContentFingerprint,
                    requirement.PreferredSlot,
                    assignedSlot,
                    prepared is not null,
                    prepared?.Verification,
                    replacedSourceId);
            })
            .ToArray();

        return new DiySlotAllocationResult(
            allocations.Length == normalizedRequirements.Length
            && issues.All(issue => issue.Severity != PreflightIssueSeverity.Blocking),
            allocations,
            issues);
    }

    private static int FindAvailableSlot(
        int capacity,
        IReadOnlyDictionary<int, string> assigned,
        IReadOnlyDictionary<int, MaskPreparedSlot> prepared)
    {
        for (var slot = FacePattern.MinSlot; slot <= capacity; slot++)
        {
            if (!assigned.ContainsKey(slot) && !prepared.ContainsKey(slot))
            {
                return slot;
            }
        }

        for (var slot = FacePattern.MinSlot; slot <= capacity; slot++)
        {
            if (!assigned.ContainsKey(slot))
            {
                return slot;
            }
        }

        return 0;
    }
}
