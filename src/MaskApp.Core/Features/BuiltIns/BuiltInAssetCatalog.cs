namespace MaskApp.Core.Features.BuiltIns;

public static class BuiltInAssetCatalog
{
    private static readonly int[] StaticImageIds = Enumerable.Range(0, 70).ToArray();

    private static readonly int[] AnimationIds =
        Enumerable.Range(0, 46).Where(id => id != 4).ToArray();

    private static readonly BuiltInAssetDefinition[] StaticImageDefinitions =
        StaticImageIds.Select((id, index) => CreateDefinition(BuiltInAssetType.StaticImage, id, index)).ToArray();

    private static readonly BuiltInAssetDefinition[] AnimationDefinitions =
        AnimationIds.Select((id, index) => CreateDefinition(BuiltInAssetType.Animation, id, index)).ToArray();

    private static readonly BuiltInAssetDefinition[] AllDefinitions =
        StaticImageDefinitions.Concat(AnimationDefinitions).ToArray();

    public static IReadOnlyList<BuiltInAssetDefinition> StaticImages => StaticImageDefinitions;

    public static IReadOnlyList<BuiltInAssetDefinition> Animations => AnimationDefinitions;

    public static IReadOnlyList<BuiltInAssetDefinition> Definitions => AllDefinitions;

    public static IReadOnlyList<int> GetKnownIds(BuiltInAssetType type) =>
        GetDefinitions(type).Select(definition => definition.Id).ToArray();

    public static IReadOnlyList<BuiltInAssetDefinition> GetDefinitions(BuiltInAssetType type) =>
        type == BuiltInAssetType.StaticImage ? StaticImageDefinitions : AnimationDefinitions;

    public static BuiltInAssetDefinition? TryGetDefinition(BuiltInAssetType type, int id) =>
        GetDefinitions(type).FirstOrDefault(definition => definition.Id == id);

    public static BuiltInAssetDefinition GetDefinitionOrFallback(BuiltInAssetType type, int id) =>
        TryGetDefinition(type, id) ?? CreateUnknownDefinition(type, id);

    public static bool IsKnown(BuiltInAssetType type, int id) =>
        TryGetDefinition(type, id) is not null;

    public static int Count(BuiltInAssetType type) => GetDefinitions(type).Count;

    public static int FirstId(BuiltInAssetType type) => GetDefinitions(type)[0].Id;

    public static int LastId(BuiltInAssetType type) => GetDefinitions(type)[^1].Id;

    public static int ClampToKnownId(BuiltInAssetType type, int id)
    {
        var ids = GetKnownIds(type);
        if (ids.Contains(id))
        {
            return id;
        }

        return ids.OrderBy(candidate => Math.Abs(candidate - id)).ThenBy(candidate => candidate).First();
    }

    public static int GetNextKnownId(BuiltInAssetType type, int id)
    {
        var ids = GetKnownIds(type).ToArray();
        var index = Array.IndexOf(ids, id);
        if (index >= 0)
        {
            return index < ids.Length - 1 ? ids[index + 1] : id;
        }

        return ids.FirstOrDefault(candidate => candidate > id, id);
    }

    public static int GetPreviousKnownId(BuiltInAssetType type, int id)
    {
        var ids = GetKnownIds(type).ToArray();
        var index = Array.IndexOf(ids, id);
        if (index >= 0)
        {
            return index > 0 ? ids[index - 1] : id;
        }

        return ids.LastOrDefault(candidate => candidate < id, id);
    }

    public static int GetPosition(BuiltInAssetType type, int id)
    {
        var ids = GetKnownIds(type).ToArray();
        var index = Array.IndexOf(ids, id);
        return index < 0 ? 0 : index + 1;
    }

    public static string GetDefaultName(BuiltInAssetType type, int id) =>
        GetDefinitionOrFallback(type, id).DefaultName;

    public static bool IsGeneratedDefaultName(BuiltInAssetType type, int id, string? displayName)
    {
        var trimmed = displayName?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || string.Equals(trimmed, GetDefaultName(type, id), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var oldPrefixes = type == BuiltInAssetType.StaticImage
            ? new[] { "Image", "Android Image" }
            : new[] { "Animation", "Android Animation" };
        return oldPrefixes.Any(prefix => string.Equals(trimmed, $"{prefix} {id}", StringComparison.OrdinalIgnoreCase));
    }

    private static BuiltInAssetDefinition CreateDefinition(BuiltInAssetType type, int id, int sortIndex)
    {
        var name = type == BuiltInAssetType.StaticImage
            ? $"Face {id:00}"
            : $"Animation {id:00}";
        return new BuiltInAssetDefinition(type, id, name, sortIndex, GetPreview(type, id));
    }

    private static BuiltInAssetDefinition CreateUnknownDefinition(BuiltInAssetType type, int id)
    {
        var name = type == BuiltInAssetType.StaticImage
            ? $"Unknown face {id}"
            : $"Unknown animation {id}";
        return new BuiltInAssetDefinition(type, id, name, int.MaxValue, BuiltInAssetPreview.Empty);
    }

    private static BuiltInAssetPreview GetPreview(BuiltInAssetType type, int id) =>
        BuiltInPreviewCatalogGenerated.All.TryGetValue((type, id), out var preview)
            ? preview
            : BuiltInAssetPreview.Empty;
}
