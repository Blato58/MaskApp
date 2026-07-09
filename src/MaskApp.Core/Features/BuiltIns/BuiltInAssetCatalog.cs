namespace MaskApp.Core.Features.BuiltIns;

public static class BuiltInAssetCatalog
{
    private const int PreviewWidth = 24;
    private const int PreviewHeight = 9;

    private static readonly IReadOnlyDictionary<int, int[]> StaticImagePreviewData = new Dictionary<int, int[]>
    {
        [0] = [0, 16320, 1012, 53244, 15600, 4080, 4080, 15612, 53235, 1008, 16320, 0, 0, 16320, 1012, 53244, 15600, 4080, 4080, 15612, 53235, 1008, 16320, 0],
        [1] = [0, 4112, 29812, 130557, 32756, 8144, 32756, 130557, 29812, 4112, 0, 0, 0, 0, 4112, 29812, 65021, 32756, 8144, 32756, 130557, 29812, 4112, 0],
        [2] = [0, 4092, 15408, 64764, 65520, 16380, 65520, 64572, 15600, 4092, 0, 0, 0, 0, 4092, 15408, 64764, 65520, 16380, 65520, 64572, 15600, 4092, 0],
        [3] = [0, 16368, 49164, 212163, 196851, 196851, 196851, 212163, 49164, 16368, 0, 0, 0, 0, 16368, 49164, 212163, 196851, 196851, 196851, 212163, 49164, 16368, 0],
        [4] = [0, 16368, 49164, 196611, 196611, 212739, 197379, 197379, 49164, 16368, 0, 0, 0, 0, 16368, 49164, 196611, 196611, 212739, 197379, 197379, 49164, 16368, 0],
        [5] = [5376, 27200, 114576, 114580, 28644, 7161, 28644, 114580, 114576, 27200, 5376, 0, 0, 5376, 27200, 114576, 114580, 28644, 7161, 28644, 114580, 114576, 27200, 5376],
        [6] = [0, 64514, 13096, 16298, 13098, 64554, 10, 2, 340, 0, 0, 0, 0, 0, 0, 64514, 13096, 16298, 13098, 64554, 10, 2, 340, 0],
        [7] = [0, 15360, 15363, 65535, 1008, 1008, 1008, 1012, 1023, 3072, 0, 0, 0, 0, 15360, 15363, 65535, 1008, 1008, 1008, 1012, 1023, 3072, 0],
        [8] = [0, 3072, 13296, 49164, 52227, 49155, 15363, 2100, 972, 240, 0, 0, 0, 0, 3072, 13296, 49164, 52227, 49155, 15363, 2100, 972, 240, 0],
        [9] = [0, 0, 0, 14352, 49668, 260757, 49668, 12432, 0, 0, 0, 0, 0, 0, 0, 0, 14352, 49668, 260757, 49668, 12432, 0, 0, 0],
        [10] = [4080, 12348, 62271, 258111, 262143, 262143, 258111, 62271, 12348, 4080, 0, 0, 0, 0, 4080, 12348, 62271, 258111, 262143, 262143, 258111, 62271, 12348, 4080]
    };

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
        var ids = GetDefinitions(type).Select(definition => definition.Id).ToArray();
        if (ids.Contains(id))
        {
            return id;
        }

        return ids.OrderBy(candidate => Math.Abs(candidate - id)).ThenBy(candidate => candidate).First();
    }

    public static int GetNextKnownId(BuiltInAssetType type, int id)
    {
        var ids = GetDefinitions(type).Select(definition => definition.Id).ToArray();
        var index = Array.IndexOf(ids, id);
        if (index >= 0)
        {
            return index < ids.Length - 1 ? ids[index + 1] : id;
        }

        return ids.FirstOrDefault(candidate => candidate > id, id);
    }

    public static int GetPreviousKnownId(BuiltInAssetType type, int id)
    {
        var ids = GetDefinitions(type).Select(definition => definition.Id).ToArray();
        var index = Array.IndexOf(ids, id);
        if (index >= 0)
        {
            return index > 0 ? ids[index - 1] : id;
        }

        return ids.LastOrDefault(candidate => candidate < id, id);
    }

    public static int GetPosition(BuiltInAssetType type, int id)
    {
        var ids = GetDefinitions(type).Select(definition => definition.Id).ToArray();
        var index = Array.IndexOf(ids, id);
        return index < 0 ? 0 : index + 1;
    }

    public static string GetDefaultName(BuiltInAssetType type, int id) =>
        GetDefinitionOrFallback(type, id).DefaultName;

    public static bool IsGeneratedDefaultName(BuiltInAssetType type, int id, string? displayName)
    {
        var trimmed = displayName?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return true;
        }

        if (string.Equals(trimmed, GetDefaultName(type, id), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var oldPrefix = type == BuiltInAssetType.StaticImage ? "Image" : "Animation";
        return string.Equals(trimmed, $"{oldPrefix} {id}", StringComparison.OrdinalIgnoreCase);
    }

    private static BuiltInAssetDefinition CreateDefinition(BuiltInAssetType type, int id, int sortIndex)
    {
        var name = type == BuiltInAssetType.StaticImage
            ? $"Android Image {id:00}"
            : $"Android Animation {id:00}";
        return new BuiltInAssetDefinition(type, id, name, sortIndex, CreatePreview(type, id));
    }

    private static BuiltInAssetDefinition CreateUnknownDefinition(BuiltInAssetType type, int id)
    {
        var name = type == BuiltInAssetType.StaticImage
            ? $"Unknown Image {id}"
            : $"Unknown Animation {id}";
        return new BuiltInAssetDefinition(type, id, name, int.MaxValue, CreateFallbackPreview(type, id));
    }

    private static BuiltInAssetPreview CreatePreview(BuiltInAssetType type, int id)
    {
        if (type == BuiltInAssetType.StaticImage && StaticImagePreviewData.TryGetValue(id, out var encoded))
        {
            return CreateEncodedPreview(encoded, "ImageData.java");
        }

        return CreateFallbackPreview(type, id);
    }

    private static BuiltInAssetPreview CreateEncodedPreview(int[] encodedColumns, string sourceLabel)
    {
        var rows = new string[PreviewHeight];
        for (var row = 0; row < PreviewHeight; row++)
        {
            var chars = new char[encodedColumns.Length];
            for (var column = 0; column < encodedColumns.Length; column++)
            {
                var intensity = (encodedColumns[column] >> (row * 2)) & 3;
                chars[column] = intensity switch
                {
                    1 => 'o',
                    2 => 'O',
                    3 => '#',
                    _ => '.'
                };
            }

            rows[row] = new string(chars);
        }

        var frame = new BuiltInAssetPreviewFrame(encodedColumns.Length, PreviewHeight, rows);
        return new BuiltInAssetPreview(encodedColumns.Length, PreviewHeight, [frame], isDataBacked: true, sourceLabel);
    }

    private static BuiltInAssetPreview CreateFallbackPreview(BuiltInAssetType type, int id)
    {
        var rows = new string[PreviewHeight];
        for (var row = 0; row < PreviewHeight; row++)
        {
            var chars = new char[PreviewWidth];
            for (var column = 0; column < PreviewWidth; column++)
            {
                var border = row is 0 or PreviewHeight - 1 || column is 0 or PreviewWidth - 1;
                var seed = (id * 31) + (row * 7) + (column * 13) + (type == BuiltInAssetType.Animation ? 17 : 0);
                var accent = seed % 11 == 0 || (column + id) % 17 == row % 7;
                chars[column] = border ? '.' : accent ? '#' : '.';
            }

            rows[row] = new string(chars);
        }

        var frame = new BuiltInAssetPreviewFrame(PreviewWidth, PreviewHeight, rows);
        return new BuiltInAssetPreview(PreviewWidth, PreviewHeight, [frame], isDataBacked: false, "Deterministic fallback");
    }
}
