using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.TextPresets;

public sealed class TextPresetCard
{
    public TextPresetCard(TextPreset preset, AsyncRelayCommand sendCommand)
    {
        Preset = preset;
        SendCommand = sendCommand;
    }

    public TextPreset Preset { get; }

    public TextPresetId Id => Preset.Id;

    public string DisplayName => Preset.DisplayName;

    public string InputText => Preset.InputText;

    public string MaskText => Preset.MaskText;

    public string MaskTextWarning => Preset.MaskTextWarning;

    public bool HasMaskTextDifference => Preset.HasMaskTextDifference;

    public string CategoryText => Preset.Category switch
    {
        TextPresetCategory.CzechBasic => "Czech Basic",
        TextPresetCategory.CzechMeme => "Czech Meme",
        TextPresetCategory.CzechPoliticalSatire => "Political/Satire",
        TextPresetCategory.CzechRave => "Czech RAVE",
        TextPresetCategory.Legacy => "Legacy",
        _ => "Custom"
    };

    public string ColorHex => Preset.Style.ForegroundHex;

    public string FavoriteText => Preset.IsFavorite ? "Favorite" : "Preset";

    public AsyncRelayCommand SendCommand { get; }
}

public sealed record TextPresetGroup(
    TextPresetCategory Category,
    string Title,
    IReadOnlyList<TextPresetCard> Cards);
