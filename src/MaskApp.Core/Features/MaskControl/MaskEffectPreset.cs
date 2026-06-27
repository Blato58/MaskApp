using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.MaskControl;

public sealed record MaskEffectPreset(string Name, string KindText, int PresetId, AsyncRelayCommand ApplyCommand);
