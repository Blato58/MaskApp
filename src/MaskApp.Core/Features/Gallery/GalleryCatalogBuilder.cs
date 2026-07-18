using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryCatalogBuilder
{
    private readonly QuickActionCatalog quickActionCatalog;
    private readonly AnimationProjectCompiler animationCompiler;
    private readonly SceneValidator sceneValidator;

    public GalleryCatalogBuilder(QuickActionCatalog quickActionCatalog)
    {
        this.quickActionCatalog = quickActionCatalog;
        animationCompiler = new AnimationProjectCompiler();
        sceneValidator = new SceneValidator();
    }

    public IReadOnlyList<GalleryItem> Build(
        TextPresetStoreState textPresetState,
        BuiltInAssetArchive builtInArchive,
        FacePatternStoreState faceState,
        GalleryOrderState orderState,
        AnimationProjectStoreState? animationProjectState = null,
        SceneShowState? sceneShowState = null)
    {
        var items = new List<GalleryItem>();
        var sortIndex = 0;
        var normalizedFaceState = faceState.Normalize();

        items.AddRange(textPresetState.Normalize().Presets
            .Where(preset => preset.Category != TextPresetCategory.Legacy || preset.IsFavorite)
            .Select(preset => CreateTextPresetItem(preset, sortIndex++)));

        items.AddRange(GetBuiltInCatalogRecords(builtInArchive)
            .Select(record => CreateBuiltInItem(record, sortIndex++)));

        items.AddRange(normalizedFaceState.Patterns
            .Select(pattern => CreateFaceItem(pattern, normalizedFaceState, sortIndex++)));

        items.AddRange(AppBuiltInAnimationCatalog.CreateBuiltIns()
            .Select(animation => CreateAppBuiltInAnimationItem(animation, normalizedFaceState, sortIndex++)));

        items.AddRange((animationProjectState ?? new AnimationProjectStoreState()).Normalize().Projects
            .Select(project => CreateCustomAnimationItem(project, normalizedFaceState, sortIndex++)));

        items.AddRange(quickActionCatalog.Actions
            .Where(action => action.Kind is QuickActionKind.Text or QuickActionKind.Command or QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation or QuickActionKind.Random)
            .Select(action => CreateQuickActionItem(action, sortIndex++)));

        var contentCatalog = items.ToDictionary(item => item.Id, StringComparer.Ordinal);
        items.AddRange((sceneShowState ?? new SceneShowState()).Normalize().Scenes
            .Select(scene => CreateSceneItem(scene, contentCatalog, sortIndex++)));

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

    private static GalleryItem CreateFaceItem(
        FacePattern pattern,
        FacePatternStoreState faceState,
        int sortIndex)
    {
        var normalized = pattern.Normalize();
        var isHolyPriest = normalized.Id.StartsWith("built-in-face-holy-priest-", StringComparison.Ordinal);
        var isPrepared = DiySlotPlaybackCoordinator.IsFacePrepared(normalized, faceState);
        return new GalleryItem
        {
            Id = $"face:{normalized.Id}",
            Type = GalleryItemType.CustomStaticFace,
            Title = normalized.DisplayName,
            Subtitle = isHolyPriest
                ? $"App-built artist face / DIY slot {normalized.PreferredSlot}"
                : $"{normalized.SourceLabel} / Slot {normalized.PreferredSlot}",
            GroupName = isHolyPriest
                ? "Holy Priest"
                : normalized.IsBuiltIn ? "Pixel face collection" : "Custom faces",
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
            LastSendStatus = isPrepared
                ? $"Prepared DIY slot {normalized.PreferredSlot} · PLAY only"
                : string.IsNullOrWhiteSpace(normalized.LastUploadStatus)
                    ? "DIY upload needs real-mask test"
                    : normalized.LastUploadStatus,
            ManageTarget = "faces",
            FacePattern = normalized
        };
    }

    private static GalleryItem CreateAppBuiltInAnimationItem(
        AppBuiltInAnimation animation,
        FacePatternStoreState faceState,
        int sortIndex)
    {
        var normalized = animation.Normalize();
        var slotLabel = string.Join(", ", normalized.ReservedSlots);
        var isPrepared = DiySlotPlaybackCoordinator.IsAnimationPrepared(normalized, faceState);
        return new GalleryItem
        {
            Id = $"app-animation:{normalized.Id}",
            Type = GalleryItemType.AppBuiltInAnimation,
            Title = normalized.DisplayName,
            Subtitle = $"{normalized.Frames.Count} app-built frames / DIY slots {slotLabel} / Experimental",
            GroupName = normalized.ArtistName,
            IsFavorite = normalized.IsFavorite,
            ColorHex = normalized.ColorHex,
            IconKey = "anim",
            SortIndex = sortIndex,
            LastSendStatus = isPrepared
                ? "Prepared on active mask profile · continuous 75 ms PLAY"
                : "Prepare once, then loop with continuous 75 ms PLAY",
            PreviewBadgeText = $"DIY · {normalized.Frames.Count} frames",
            PreviewSourceText = normalized.Description,
            CanManage = false,
            FacePattern = normalized.PreviewPattern,
            AppAnimation = normalized
        };
    }

    private GalleryItem CreateCustomAnimationItem(
        AnimationProject project,
        FacePatternStoreState faceState,
        int sortIndex)
    {
        var normalized = project.Normalize();
        var compilation = animationCompiler.Compile(normalized);
        var animation = compilation.Animation;
        var isPrepared = animation is not null &&
            DiySlotPlaybackCoordinator.IsAnimationPrepared(animation, faceState);
        return new GalleryItem
        {
            Id = $"animation:{normalized.Id}",
            Type = GalleryItemType.CustomAnimation,
            Title = normalized.DisplayName,
            Subtitle = compilation.Succeeded
                ? $"{normalized.Frames.Count} timeline frames / {compilation.UniqueFrameCount} unique DIY slots"
                : compilation.Message,
            GroupName = normalized.Source is AnimationProjectSource.GifImport or AnimationProjectSource.VideoImport
                ? "Imported animations"
                : "Custom animations",
            IsFavorite = normalized.IsFavorite,
            ColorHex = "#FF3D8B",
            IconKey = "anim",
            SortIndex = sortIndex,
            LastSendStatus = compilation.Succeeded
                ? isPrepared
                    ? "Prepared on active mask profile · production timed PLAY"
                    : "Prepare unique frames once, then preview through the production engine"
                : compilation.Message,
            PreviewBadgeText = compilation.Succeeded
                ? $"DIY · {compilation.UniqueFrameCount}/{PerformanceAnimation.MaxUniqueFrames} slots"
                : "Slot budget exceeded",
            PreviewSourceText = normalized.Source.ToString(),
            PreviewIsAnimated = normalized.Frames.Count > 1,
            PreviewFrameCount = normalized.Frames.Count,
            CanSend = compilation.Succeeded,
            CanManage = true,
            ManageTarget = "animation-studio",
            FacePattern = normalized.Frames[0].Pattern,
            AnimationProject = normalized,
            PerformanceAnimation = animation
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

    private GalleryItem CreateSceneItem(
        PerformanceScene source,
        IReadOnlyDictionary<string, GalleryItem> catalog,
        int sortIndex)
    {
        var scene = source.Normalize();
        var validation = sceneValidator.Validate(scene, catalog);
        var preview = scene.Steps
            .Where(step => !string.IsNullOrWhiteSpace(step.GalleryItemId))
            .Select(step => catalog.GetValueOrDefault(step.GalleryItemId))
            .FirstOrDefault(item => item?.HasAnyPreview == true);
        var blockingCount = validation.Issues.Count(issue => issue.Severity == SceneValidationSeverity.Blocking);
        return new GalleryItem
        {
            Id = $"scene:{scene.Id}",
            Type = GalleryItemType.Scene,
            Title = scene.DisplayName,
            Subtitle = validation.IsValid
                ? $"{scene.Steps.Count} steps / {validation.ExpandedStepCount} executed / {scene.FailurePolicy}"
                : $"{blockingCount} blocking validation issue(s)",
            GroupName = "Scenes",
            IsFavorite = scene.IsFavorite,
            ColorHex = scene.ColorHex,
            IconKey = "lucide:clapperboard",
            SortIndex = sortIndex,
            LastSendStatus = validation.IsValid
                ? "Validate dependencies in Preflight before live use"
                : string.Join(" ", validation.Issues
                    .Where(issue => issue.Severity == SceneValidationSeverity.Blocking)
                    .Select(issue => issue.Message)),
            PreviewResourceName = preview?.PreviewResourceName ?? string.Empty,
            PreviewBadgeText = $"SCENE · {validation.ExpandedStepCount} steps",
            PreviewSourceText = "Bounded typed cue",
            PreviewIsAnimated = true,
            PreviewFrameCount = validation.ExpandedStepCount,
            CanSend = validation.IsValid,
            CanManage = true,
            ManageTarget = "scene-studio",
            FacePattern = preview?.FacePattern,
            Scene = scene
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
