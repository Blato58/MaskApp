using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Preflight;

namespace MaskApp.Core.Tests.Features.Preflight;

public sealed class DiySlotAllocatorTests
{
    [Fact]
    public void Allocate_DeduplicatesFrames_AndRemapsPreferredSlotCollision()
    {
        var requirements = new[]
        {
            new DiySlotRequirement("a-1", "animation-a-1", "SAME", 7),
            new DiySlotRequirement("a-2", "animation-a-2", "SAME", 8),
            new DiySlotRequirement("b", "animation-b", "DIFFERENT", 7)
        };

        var result = new DiySlotAllocator().Allocate(requirements, profile: null, slotCapacity: 20);

        Assert.False(result.Succeeded);
        Assert.Equal(3, result.Allocations.Count);
        var sameAllocations = result.Allocations.Where(item => item.ContentFingerprint == "SAME").ToArray();
        Assert.Equal(2, sameAllocations.Length);
        Assert.Single(sameAllocations.Select(item => item.AssignedSlot).Distinct());
        Assert.NotEqual(
            sameAllocations[0].AssignedSlot,
            result.Allocations.Single(item => item.ContentFingerprint == "DIFFERENT").AssignedSlot);
        var collision = Assert.Single(result.Issues, issue => issue.Code == "diy-slot-remapped");
        Assert.Equal(PreflightIssueSeverity.Blocking, collision.Severity);
    }

    [Fact]
    public void Allocate_ReusesMatchingPreparedSlotAndVerification()
    {
        var profile = CreateProfile(
            new MaskPreparedSlot
            {
                Slot = 10,
                ContentFingerprint = "READY",
                SourceId = "old-source",
                InstalledAt = DateTimeOffset.Parse("2026-07-17T10:00:00Z"),
                Verification = MaskPreparedSlotVerification.Acknowledged
            });

        var result = new DiySlotAllocator().Allocate(
            [new DiySlotRequirement("frame", "new-source", "READY", 4)],
            profile,
            slotCapacity: 20);

        var allocation = Assert.Single(result.Allocations);
        Assert.True(allocation.IsPrepared);
        Assert.Equal(10, allocation.AssignedSlot);
        Assert.Equal(MaskPreparedSlotVerification.Acknowledged, allocation.Verification);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Allocate_ReportsCapacityOverrunWithoutPartialSuccess()
    {
        var requirements = Enumerable.Range(1, 3)
            .Select(index => new DiySlotRequirement($"r{index}", $"source-{index}", $"hash-{index}", index))
            .ToArray();

        var result = new DiySlotAllocator().Allocate(requirements, profile: null, slotCapacity: 2);

        Assert.False(result.Succeeded);
        Assert.Empty(result.Allocations);
        var issue = Assert.Single(result.Issues);
        Assert.Equal("diy-capacity-exceeded", issue.Code);
        Assert.Equal(PreflightIssueSeverity.Blocking, issue.Severity);
    }

    private static MaskProfile CreateProfile(params MaskPreparedSlot[] slots) =>
        new()
        {
            ProfileId = "mask-test",
            DisplayName = "Test Mask",
            FirstSeenAt = DateTimeOffset.Parse("2026-07-17T09:00:00Z"),
            LastSeenAt = DateTimeOffset.Parse("2026-07-17T10:00:00Z"),
            Capabilities = new MaskCapabilitySnapshot
            {
                CommandWriteAvailable = true,
                TextUploadAvailable = true,
                FaceUploadAvailable = true,
                AcknowledgementMode = MaskAcknowledgementMode.Acknowledged,
                DiySlotCapacity = 20,
                TransportName = "Fake",
                ObservedAt = DateTimeOffset.Parse("2026-07-17T10:00:00Z")
            },
            PreparedSlots = slots
        };
}
