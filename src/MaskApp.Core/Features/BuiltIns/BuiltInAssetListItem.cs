using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetListItem(
    BuiltInAssetRecord Record,
    string Title,
    string Subtitle,
    AsyncRelayCommand LoadCommand);
