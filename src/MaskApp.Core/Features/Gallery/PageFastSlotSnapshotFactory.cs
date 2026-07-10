using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public static class PageFastSlotSnapshotFactory
{
    private const int TextCanvasHeight = 16;

    public static bool Supports(GalleryItem item) =>
        item.Type is GalleryItemType.TextPreset or GalleryItemType.CustomStaticFace;

    public static PageFastSlotSnapshot Create(GalleryItem item, int slot) =>
        item.Type switch
        {
            GalleryItemType.TextPreset when item.TextPreset is not null => CreateTextSnapshot(item.TextPreset, slot),
            GalleryItemType.CustomStaticFace when item.FacePattern is not null => CreateFaceSnapshot(item.FacePattern, slot),
            _ => throw new ArgumentException("Only text presets and custom faces can use a fast mask slot.", nameof(item))
        };

    public static PageFastSlotSnapshot CreateFaceSnapshot(FacePattern pattern, int slot)
    {
        var normalized = pattern.Normalize() with
        {
            PreferredSlot = Math.Clamp(slot, FacePattern.MinSlot, FacePattern.MaxSlot)
        };
        return CreateSnapshot(normalized, "DIY face");
    }

    public static PageFastSlotSnapshot CreateTextSnapshot(TextPreset preset, int slot)
    {
        var normalized = preset.Normalize();
        var sourceProfile = normalized.Style.ToTextSendProfile();
        var snapshotProfile = sourceProfile with
        {
            Name = "Pages fast slot",
            LayoutMode = normalized.Style.LayoutMode == TextPresetLayoutMode.ThreeLineCentered
                ? TextLayoutMode.ThreeLineCentered
                : TextLayoutMode.FixedWidthCentered,
            FixedWidthColumns = QuickCaptionLayout.VisibleColumns,
            DisplayMode = TextDisplayMode.Blink
        };
        var plan = TextSendPackageFactory.Create(normalized.MaskText, snapshotProfile, acknowledgementsAvailable: false);
        var pattern = CreateTextPattern(normalized, plan.Package.LedData, snapshotProfile.TextColor, slot);
        return CreateSnapshot(pattern, "Static text snapshot");
    }

    private static FacePattern CreateTextPattern(
        TextPreset preset,
        byte[] ledData,
        TextLedColor textColor,
        int slot)
    {
        var columnCount = ledData.Length / 2;
        var left = Math.Max(0, (FacePattern.Width - columnCount) / 2);
        var top = (FacePattern.Height - TextCanvasHeight) / 2;
        var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        var color = new FaceColor(textColor.Red, textColor.Green, textColor.Blue);

        for (var column = 0; column < Math.Min(columnCount, FacePattern.Width); column++)
        {
            var byteOffset = column * 2;
            var columnBits = (ledData[byteOffset] << 8) | ledData[byteOffset + 1];
            for (var row = 0; row < TextCanvasHeight; row++)
            {
                if ((columnBits & (1 << (15 - row))) == 0)
                {
                    continue;
                }

                pixels[((top + row) * FacePattern.Width) + left + column] = new FacePixel(true, color);
            }
        }

        return new FacePattern
        {
            Id = $"page-text-{preset.Id.Value}",
            DisplayName = preset.DisplayName,
            Source = FacePatternSource.Custom,
            PreferredSlot = Math.Clamp(slot, FacePattern.MinSlot, FacePattern.MaxSlot),
            Pixels = pixels
        }.Normalize();
    }

    private static PageFastSlotSnapshot CreateSnapshot(FacePattern pattern, string description)
    {
        return new PageFastSlotSnapshot(pattern, FaceContentFingerprint.Compute(pattern), description);
    }
}
