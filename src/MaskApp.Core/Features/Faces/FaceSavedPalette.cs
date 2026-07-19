namespace MaskApp.Core.Features.Faces;

public sealed record FaceSavedPalette
{
    public const int MaxColors = 8;

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "Saved palette";

    public FaceColor[] Colors { get; init; } = [];

    public string ColorSummary => string.Join(" · ", Colors.Select(color => color.Hex));

    public FaceSavedPalette Normalize()
    {
        var colors = (Colors ?? [])
            .Distinct()
            .Take(MaxColors)
            .ToArray();
        return this with
        {
            Id = string.IsNullOrWhiteSpace(Id) ? $"palette-{Guid.NewGuid():N}" : Id.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? "Saved palette" : DisplayName.Trim(),
            Colors = colors
        };
    }
}
