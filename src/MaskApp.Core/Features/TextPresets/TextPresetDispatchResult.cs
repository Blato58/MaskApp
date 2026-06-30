namespace MaskApp.Core.Features.TextPresets;

public sealed record TextPresetDispatchResult(
    bool Succeeded,
    TextPresetId PresetId,
    string Message,
    string Status)
{
    public static TextPresetDispatchResult Sent(TextPreset preset, string status)
    {
        var message = preset.HasMaskTextDifference
            ? $"Sent, confirm on mask. Transliteration used: {preset.MaskText}"
            : "Sent, confirm on mask";
        return new TextPresetDispatchResult(true, preset.Id, message, status);
    }

    public static TextPresetDispatchResult Failed(TextPreset preset, string message, string status = "failed") =>
        new(false, preset.Id, message, status);
}
