using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Text;

public sealed class TextComposerPresetCard
{
    public TextComposerPresetCard(
        TextPreset preset,
        AsyncRelayCommand openCommand,
        AsyncRelayCommand sendCommand,
        AsyncRelayCommand duplicateCommand,
        AsyncRelayCommand deleteCommand)
    {
        Preset = preset;
        OpenCommand = openCommand;
        SendCommand = sendCommand;
        DuplicateCommand = duplicateCommand;
        DeleteCommand = deleteCommand;
    }

    public TextPreset Preset { get; }

    public TextPresetId Id => Preset.Id;

    public string DisplayName => Preset.DisplayName;

    public string InputText => Preset.InputText;

    public string MaskText => Preset.MaskText;

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

    public string VisibilityText
    {
        get
        {
            var surfaces = new List<string>();
            if (Preset.ShowInControl)
            {
                surfaces.Add("Control");
            }

            if (Preset.ShowInReact)
            {
                surfaces.Add("React");
            }

            if (Preset.ShowInRave)
            {
                surfaces.Add("RAVE");
            }

            return surfaces.Count == 0 ? "Composer only" : string.Join(" / ", surfaces);
        }
    }

    public string StyleSummary =>
        $"{Preset.Style.LayoutMode} · {Preset.Style.DisplayMode} · Speed {Preset.Style.Speed} · {Preset.Style.WeightText}";

    public string LastSendStatusText => string.IsNullOrWhiteSpace(Preset.LastSendStatus)
        ? "Not sent yet"
        : Preset.LastSendStatus;

    public string MaskTextWarning => Preset.MaskTextWarning;

    public bool HasMaskTextDifference => Preset.HasMaskTextDifference;

    public AsyncRelayCommand OpenCommand { get; }

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand DuplicateCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }

    public void RaiseCommandStates()
    {
        OpenCommand.RaiseCanExecuteChanged();
        SendCommand.RaiseCanExecuteChanged();
        DuplicateCommand.RaiseCanExecuteChanged();
        DeleteCommand.RaiseCanExecuteChanged();
    }
}
