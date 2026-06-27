using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.BuiltIns;

public static class BuiltInAssetCommandFactory
{
    public static MaskCommand CreateCommand(BuiltInAssetRecord record) =>
        record.Type == BuiltInAssetType.Animation
            ? MaskCommandBuilder.Animation(record.Id, record.DisplayName)
            : MaskCommandBuilder.Image(record.Id, record.DisplayName);
}
