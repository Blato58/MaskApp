namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetDefinition(
    BuiltInAssetType Type,
    int Id,
    string DefaultName,
    int SortIndex,
    BuiltInAssetPreview Preview)
{
    public string HexId => BuiltInAssetRange.ToHexId(Id);
}
