using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.Gallery;

public sealed class PageFastSlotSnapshotFactoryTests
{
    [Fact]
    public void CreateTextSnapshot_RendersStaticFacePayloadInRequestedSlot()
    {
        var preset = CreatePreset("HELLO", new TextLedColor(0x12, 0x34, 0x56));

        var snapshot = PageFastSlotSnapshotFactory.CreateTextSnapshot(preset, 18);
        var payload = FaceUploadProtocol.BuildPayload(snapshot.Pattern);

        Assert.Equal(18, snapshot.Pattern.PreferredSlot);
        Assert.Equal(FaceUploadProtocol.PayloadLength, payload.Length);
        Assert.Equal("Static text snapshot", snapshot.ContentDescription);
        Assert.Equal(64, snapshot.ContentFingerprint.Length);
        Assert.Contains(snapshot.Pattern.Pixels, pixel => pixel.IsLit && pixel.Color == new FaceColor(0x12, 0x34, 0x56));
        Assert.DoesNotContain(
            snapshot.Pattern.Pixels.Select((pixel, index) => (pixel, row: index / FacePattern.Width)),
            item => item.pixel.IsLit && (item.row < 21 || item.row > 36));
    }

    [Fact]
    public void CreateTextSnapshot_ContentChangesInvalidateFingerprint()
    {
        var first = PageFastSlotSnapshotFactory.CreateTextSnapshot(CreatePreset("YES", new TextLedColor(0xFF, 0xFF, 0xFF)), 20);
        var changedText = PageFastSlotSnapshotFactory.CreateTextSnapshot(CreatePreset("NO", new TextLedColor(0xFF, 0xFF, 0xFF)), 20);
        var changedColor = PageFastSlotSnapshotFactory.CreateTextSnapshot(CreatePreset("YES", new TextLedColor(0xFF, 0x00, 0x00)), 20);

        Assert.NotEqual(first.ContentFingerprint, changedText.ContentFingerprint);
        Assert.NotEqual(first.ContentFingerprint, changedColor.ContentFingerprint);
    }

    [Fact]
    public void CreateFaceSnapshot_UsesFacePixelsWithoutChangingSourcePattern()
    {
        var source = FacePatternFactory.CreateBuiltIns()[0];

        var snapshot = PageFastSlotSnapshotFactory.CreateFaceSnapshot(source, 12);

        Assert.Equal(12, snapshot.Pattern.PreferredSlot);
        Assert.Equal("DIY face", snapshot.ContentDescription);
        Assert.Equal(
            Convert.ToHexString(FaceUploadProtocol.BuildPayload(source)),
            Convert.ToHexString(FaceUploadProtocol.BuildPayload(snapshot.Pattern)));
    }

    private static TextPreset CreatePreset(string text, TextLedColor color) =>
        new()
        {
            Id = TextPresetId.NewUserPreset(),
            InputText = text,
            MaskText = text,
            DisplayName = text,
            Style = TextPresetStyle.Default with
            {
                ForegroundColor = color,
                LayoutMode = TextPresetLayoutMode.FixedWidthCentered
            }
        };
}
