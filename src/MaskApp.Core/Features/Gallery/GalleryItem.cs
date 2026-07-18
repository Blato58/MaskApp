using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryItem
{
    public string Id { get; init; } = string.Empty;

    public GalleryItemType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Subtitle { get; init; } = string.Empty;

    public string GroupName { get; init; } = "General";

    public bool IsFavorite { get; init; }

    public string ColorHex { get; init; } = "#A78BFA";

    public string IconKey { get; init; } = "face";

    public int SortIndex { get; init; }

    public DateTimeOffset? LastSentAt { get; init; }

    public string LastSendStatus { get; init; } = string.Empty;

    public string PreviewResourceName { get; init; } = string.Empty;

    public string PreviewBadgeText { get; init; } = string.Empty;

    public string PreviewSourceText { get; init; } = string.Empty;

    public bool PreviewIsAnimated { get; init; }

    public int PreviewFrameCount { get; init; }

    public bool HasResourcePreview => !string.IsNullOrWhiteSpace(PreviewResourceName);

    public bool HasFacePreview => FacePattern is not null;

    public bool HasAnyPreview => HasResourcePreview || HasFacePreview;

    public bool HasPreview => HasResourcePreview;

    public bool CanSend { get; init; } = true;

    public bool CanManage { get; init; } = true;

    public string ManageTarget { get; init; } = string.Empty;

    public QuickActionId? QuickActionId { get; init; }

    public QuickActionKind? QuickActionKind { get; init; }

    public TextPreset? TextPreset { get; init; }

    public BuiltInAssetRecord? BuiltInAssetRecord { get; init; }

    public FacePattern? FacePattern { get; init; }

    public AppBuiltInAnimation? AppAnimation { get; init; }

    public AnimationProject? AnimationProject { get; init; }

    public PerformanceAnimation? PerformanceAnimation { get; init; }

    public PerformanceScene? Scene { get; init; }

    public string TypeLabel => Type switch
    {
        GalleryItemType.TextPreset => "Text",
        GalleryItemType.BuiltInStaticImage => "Face",
        GalleryItemType.BuiltInAnimation => "Animation",
        GalleryItemType.AppBuiltInAnimation => "App Animation",
        GalleryItemType.CustomAnimation => "Animation",
        GalleryItemType.CustomStaticFace => "DIY Face",
        GalleryItemType.Scene => "Scene",
        GalleryItemType.QuickAction => "Quick",
        _ => "Import"
    };

    public string SearchText => $"{Title} {Subtitle} {GroupName} {TypeLabel} {PreviewBadgeText} {PreviewSourceText}".ToUpperInvariant();
}
