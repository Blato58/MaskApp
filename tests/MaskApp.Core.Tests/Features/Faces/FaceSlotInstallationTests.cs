using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class FaceSlotInstallationTests
{
    [Fact]
    public void SlotInstallation_SurvivesSourceDeletionAndPreferredSlotChanges()
    {
        var first = FacePatternFactory.CreateBlank("First", 12)
            .WithPixel(1, 1, new FacePixel(true, new FaceColor(0xFF, 0x00, 0x00)));
        var installedAt = DateTimeOffset.UtcNow;
        var state = new FacePatternStoreState { Patterns = [first] }
            .MarkSlotInstalled(12, FaceContentFingerprint.Compute(first), first.Id, installedAt);

        var changedLibrary = state with
        {
            Patterns =
            [
                FacePatternFactory.CreateBlank("Different", 18)
            ]
        };
        var installation = changedLibrary.Normalize().GetSlotInstallation(12);

        Assert.NotNull(installation);
        Assert.Equal(FaceContentFingerprint.Compute(first), installation.ContentFingerprint);
        Assert.Equal(first.Id, installation.SourceId);
        Assert.Equal(installedAt, installation.InstalledAt);
    }

    [Fact]
    public void MarkSlotInstalled_ReplacesPreviousContentAndClearRemovesIt()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var state = new FacePatternStoreState()
            .MarkSlotInstalled(20, "FIRST", "first", timestamp)
            .MarkSlotInstalled(20, "SECOND", "second", timestamp.AddSeconds(1));

        var installation = state.GetSlotInstallation(20);
        Assert.NotNull(installation);
        Assert.Equal("SECOND", installation.ContentFingerprint);
        Assert.Equal("second", installation.SourceId);

        Assert.Null(state.ClearSlotInstallation(20).GetSlotInstallation(20));
    }
}
