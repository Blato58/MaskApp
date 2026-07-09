using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryCatalogBuilder
{
    private readonly QuickActionCatalog quickActionCatalog;

    public GalleryCatalogBuilder(QuickActionCatalog quickActionCatalog)
    {
        this.quickActionCatalog = quickActionCatalog;
    }

    public IReadOnlyList<GalleryItem> Build(
        TextPresetStoreState textPresetState,
        BuiltInAssetArchive builtInArchive,
        FacePatternStoreState faceState,
        GalleryOrderState orderState)
    {
        var items = new List<GalleryItem>();
        var sortIndex = 0;

        items.AddRange(textPresetState.Normalize().Presets
            .Where(preset => preset.Category != TextPresetCategory.Legacy || preset.IsFavorite)
            .Select(preset => CreateTextPresetItem(preset, sortIndex++)));

        items.AddRange(GetBuiltInCatalogRecords(builtInArchive)
            .Select(record => CreateBuiltInItem(record, sortIndex++)));

        items.AddRange(faceState.Normalize().Patterns
            .Select(pattern => CreateFaceItem(pattern, sortIndex++)));

        items.AddRange(quickActionCatalog.Actions
            .Where(action => action.Kind is QuickActionKind.Text or QuickActionKind.Command or QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation or QuickActionKind.Random)
            .Select(action => CreateQuickActionItem(action, sortIndex++)));

        items.AddRange(CreateFutureItems(sortIndex));

        return items
            .Select(item => item with { SortIndex = orderState.GetItemSortIndex(item.Id, item.SortIndex) })
            .OrderBy(item => item.SortIndex)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static GalleryItem CreateTextPresetItem(TextPreset preset, int sortIndex) =>
        new()
        {
            Id = $"text:{preset.Id.Value}",
            Type = GalleryItemType.TextPreset,
            Title = preset.DisplayName,
            Subtitle = preset.InputText,
            GroupName = preset.PackName,
            IsFavorite = preset.IsFavorite,
            ColorHex = ToHex(preset.Style.ForegroundColor.Red, preset.Style.ForegroundColor.Green, preset.Style.ForegroundColor.Blue),
            IconKey = preset.Category == TextPresetCategory.CzechRave ? "rave" : "txt",
            SortIndex = sortIndex,
            LastSentAt = preset.LastSentAt,
            LastSendStatus = preset.LastSendStatus,
            ManageTarget = "text",
            TextPreset = preset
        };

    private static GalleryItem CreateBuiltInItem(BuiltInAssetRecord record, int sortIndex)
    {
        record = record.Normalize();
        var definition = BuiltInAssetCatalog.GetDefinitionOrFallback(record.Type, record.Id);
        var preview = definition.Preview;
        var isAnimation = record.Type == BuiltInAssetType.Animation;
        var isKnownCatalogItem = BuiltInAssetCatalog.IsKnown(record.Type, record.Id);
        return new GalleryItem
        {
            Id = $"built-in:{record.Type}:{record.Id}",
            Type = isAnimation ? GalleryItemType.BuiltInAnimation : GalleryItemType.BuiltInStaticImage,
            Title = record.DisplayName,
            Subtitle = isKnownCatalogItem
                ? $"{record.Status} / {record.HexId}"
                : $"{record.Status} / Archived unknown / {record.HexId}",
            GroupName = isAnimation ? "Built-in animations" : "Built-in faces",
            IsFavorite = record.IsFavorite || record.Status == BuiltInAssetStatus.Favorite,
            ColorHex = isAnimation ? "#FF3D8B" : "#52E3FF",
            IconKey = isAnimation ? "anim" : "face",
            SortIndex = sortIndex,
            LastSentAt = record.LastTestedAt,
            LastSendStatus = record.LastSendStatus,
            PreviewResourceName = preview.ResourceName,
            PreviewBadgeText = preview.BadgeText,
            PreviewSourceText = preview.Provenance,
            PreviewIsAnimated = preview.IsAnimated,
            PreviewFrameCount = preview.FrameCount,
            ManageTarget = "builtins",
            BuiltInAssetRecord = record
        };
    }

    private static IEnumerable<BuiltInAssetRecord> GetBuiltInCatalogRecords(BuiltInAssetArchive archive)
    {
        foreach (var record in archive.Records.Where(record => BuiltInAssetCatalog.IsKnown(record.Type, record.Id)))
        {
            yield return record;
        }
    }

    private static GalleryItem CreateFaceItem(FacePattern pattern, int sortIndex)
    {
        var normalized = pattern.Normalize();
        return new GalleryItem
        {
            Id = $"face:{normalized.Id}",
            Type = GalleryItemType.CustomStaticFace,
            Title = normalized.DisplayName,
            Subtitle = $"{normalized.SourceLabel} / Slot {normalized.PreferredSlot}",
            GroupName = normalized.IsBuiltIn ? "Built-in smileys" : "Custom faces",
            IsFavorite = normalized.IsFavorite,
            ColorHex = normalized.AccentColorHex,
            IconKey = normalized.Emotion switch
            {
                FaceEmotion.Happy => "lucide:smile",
                FaceEmotion.Meh => "lucide:meh",
                FaceEmotion.Wink => "lucide:laugh",
                _ => "face"
            },
            SortIndex = sortIndex,
            LastSentAt = normalized.LastUploadedAt,
            LastSendStatus = string.IsNullOrWhiteSpace(normalized.LastUploadStatus)
                ? "DIY upload needs real-mask test"
                : normalized.LastUploadStatus,
            ManageTarget = "faces",
            FacePattern = normalized
        };
    }

    private static GalleryItem CreateQuickActionItem(QuickActionDefinition action, int sortIndex) =>
        new()
        {
            Id = $"quick:{action.Id}",
            Type = GalleryItemType.QuickAction,
            Title = action.Label,
            Subtitle = action.Caption ?? action.Kind.ToString(),
            GroupName = GetQuickActionGroup(action),
            IsFavorite = action.Id is QuickActionId.Blackout or QuickActionId.Lol or QuickActionId.Nope or QuickActionId.Drop or QuickActionId.VibeCheck,
            ColorHex = GetQuickActionColor(action.Category),
            IconKey = action.Category == QuickActionCategory.Rave ? "rave" : "txt",
            SortIndex = sortIndex,
            LastSendStatus = "Built in",
            CanManage = false,
            ManageTarget = string.Empty,
            QuickActionId = action.Id,
            QuickActionKind = action.Kind
        };

    private static IEnumerable<GalleryItem> CreateFutureItems(int startSortIndex)
    {
        yield return new GalleryItem
        {
            Id = "future:custom-animation",
            Type = GalleryItemType.FutureCustomAnimation,
            Title = "Custom animation",
            Subtitle = "DIY playback remains future/Labs until physical validation.",
            GroupName = "Labs",
            ColorHex = "#475569",
            IconKey = "anim",
            SortIndex = startSortIndex,
            CanSend = false,
            CanManage = false,
            LastSendStatus = "Not implemented"
        };
    }

    private static string GetQuickActionGroup(QuickActionDefinition action) =>
        action.Category switch
        {
            QuickActionCategory.Meme => "Meme reactions",
            QuickActionCategory.Social => "Social reactions",
            QuickActionCategory.Rave => "RAVE reactions",
            QuickActionCategory.Welfare => "Welfare",
            QuickActionCategory.BuiltIn => "Built-in commands",
            _ => "Control"
        };

    private static string GetQuickActionColor(QuickActionCategory category) =>
        category switch
        {
            QuickActionCategory.Meme => "#FF3D8B",
            QuickActionCategory.Social => "#52E3FF",
            QuickActionCategory.Rave => "#FACC15",
            QuickActionCategory.Welfare => "#22C55E",
            QuickActionCategory.BuiltIn => "#A78BFA",
            _ => "#FFFFFF"
        };

    private static string ToHex(byte red, byte green, byte blue) =>
        $"#{red:X2}{green:X2}{blue:X2}";
}
