namespace MaskApp.Core.Features.TextPresets;

public sealed record TextPreset
{
    public TextPresetId Id { get; init; }

    public string InputText { get; init; } = string.Empty;

    public string MaskText { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public TextPresetCategory Category { get; init; } = TextPresetCategory.Custom;

    public string PackName { get; init; } = "Custom";

    public IReadOnlyList<string> Tags { get; init; } = [];

    public TextPresetStyle Style { get; init; } = TextPresetStyle.Default;

    public bool IsFavorite { get; init; }

    public TextPresetVisibility Visibility { get; init; } = TextPresetVisibility.ReactDefault;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastSentAt { get; init; }

    public string LastSendStatus { get; init; } = string.Empty;

    public bool IsSeed { get; init; }

    public bool ShowInControl => Visibility.ShowInControl || IsFavorite;

    public bool ShowInReact => Visibility.ShowInReact;

    public bool ShowInRave => Visibility.ShowInRave || (IsFavorite && Category == TextPresetCategory.CzechPoliticalSatire);

    public bool HasMaskTextDifference => !string.Equals(InputText, MaskText, StringComparison.Ordinal);

    public string MaskTextWarning => HasMaskTextDifference
        ? $"Mask-safe: {MaskText}"
        : string.Empty;

    public TextPreset Normalize(DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var normalizedStyle = Style.Normalize();
        var preserveLineBreaks = normalizedStyle.LayoutMode == TextPresetLayoutMode.ThreeLineCentered;
        var normalized = CzechTextNormalizer.Normalize(InputText, preserveLineBreaks);
        var displayName = string.IsNullOrWhiteSpace(DisplayName)
            ? normalized.MaskText
            : DisplayName.Trim();

        return this with
        {
            Id = string.IsNullOrWhiteSpace(Id.Value) ? TextPresetId.NewUserPreset() : Id,
            InputText = string.IsNullOrWhiteSpace(InputText) ? normalized.MaskText : InputText.Trim(),
            MaskText = string.IsNullOrWhiteSpace(MaskText)
                ? normalized.MaskText
                : CzechTextNormalizer.Normalize(MaskText, preserveLineBreaks).MaskText,
            DisplayName = displayName,
            PackName = string.IsNullOrWhiteSpace(PackName) ? "Custom" : PackName.Trim(),
            Tags = Tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            Style = normalizedStyle,
            Visibility = Visibility ?? TextPresetVisibility.ReactDefault,
            CreatedAt = CreatedAt == default ? now : CreatedAt,
            UpdatedAt = now
        };
    }
}
