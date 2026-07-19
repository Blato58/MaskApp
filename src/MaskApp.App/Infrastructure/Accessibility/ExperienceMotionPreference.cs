using MaskApp.Core.Features.Experience;

namespace MaskApp.App.Infrastructure.Accessibility;

public sealed class ExperienceMotionPreference : IMotionPreference
{
    private readonly IMotionPreference systemPreference;
    private bool? overrideValue;

    public ExperienceMotionPreference(
        IMotionPreference systemPreference,
        IAppExperienceSettingsStore settingsStore)
    {
        this.systemPreference = systemPreference;
        overrideValue = settingsStore.LoadAsync().GetAwaiter().GetResult().ReduceMotionOverride;
    }

    public bool IsReducedMotionEnabled => overrideValue ?? systemPreference.IsReducedMotionEnabled;

    public void SetOverride(bool? value) => overrideValue = value;
}
