namespace MaskApp.Core.Features.TextPresets;

public sealed record TextPresetVisibility
{
    public static TextPresetVisibility ReactDefault { get; } = new()
    {
        ShowInReact = true
    };

    public bool ShowInControl { get; init; }

    public bool ShowInReact { get; init; } = true;

    public bool ShowInRave { get; init; }
}
