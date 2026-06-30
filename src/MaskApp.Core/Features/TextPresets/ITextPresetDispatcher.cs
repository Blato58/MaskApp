namespace MaskApp.Core.Features.TextPresets;

public interface ITextPresetDispatcher
{
    Task<TextPresetDispatchResult> SendAsync(TextPreset preset, CancellationToken cancellationToken = default);
}
