namespace MaskApp.Core.Features.TextPresets;

public readonly record struct TextPresetId(string Value)
{
    public static TextPresetId NewUserPreset() => new($"user-{Guid.NewGuid():N}");

    public static TextPresetId Seed(string packName, string text)
    {
        var normalizedPack = NormalizePart(packName);
        var normalizedText = NormalizePart(text);
        return new TextPresetId($"seed-{normalizedPack}-{normalizedText}");
    }

    public override string ToString() => Value;

    private static string NormalizePart(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character => character is >= 'a' and <= 'z' or >= '0' and <= '9'
                ? character
                : '-')
            .ToArray();
        var compact = new string(chars);
        while (compact.Contains("--", StringComparison.Ordinal))
        {
            compact = compact.Replace("--", "-", StringComparison.Ordinal);
        }

        return compact.Trim('-');
    }
}
