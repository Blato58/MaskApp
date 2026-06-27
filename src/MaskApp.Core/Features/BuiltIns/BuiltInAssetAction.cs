using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetAction(
    BuiltInAssetRecord Record,
    string Label,
    string Caption,
    string Description,
    AsyncRelayCommand SendCommand);
