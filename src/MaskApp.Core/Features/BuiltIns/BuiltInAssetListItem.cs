using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetListItem(
    BuiltInAssetRecord Record,
    string Title,
    string Subtitle,
    string TypeLabel,
    string IdLabel,
    string TagsLabel,
    string StatusLabel,
    string FavoriteLabel,
    string PreviewText,
    string PreviewBadgeText,
    string PreviewSourceText,
    AsyncRelayCommand SendCommand,
    AsyncRelayCommand EditCommand);
