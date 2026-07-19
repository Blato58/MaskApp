using MaskApp.Core.Features.Stage;
using Microsoft.Maui.Devices;
using MaskApp.Core.Features.Experience;

namespace MaskApp.App.Features.Stage;

public sealed class MauiStageDeviceFeedback : IStageDeviceFeedback
{
    private bool enabled;

    public MauiStageDeviceFeedback(IAppExperienceSettingsStore settingsStore)
    {
        enabled = settingsStore.LoadAsync().GetAwaiter().GetResult().HapticsEnabled;
    }

    public void Success() => PerformIfEnabled(HapticFeedbackType.Click);

    public void Failure() => PerformIfEnabled(HapticFeedbackType.LongPress);

    public void Warning() => PerformIfEnabled(HapticFeedbackType.Click);

    public void SetEnabled(bool value) => enabled = value;

    private void PerformIfEnabled(HapticFeedbackType type)
    {
        if (enabled)
        {
            Perform(type);
        }
    }

    private static void Perform(HapticFeedbackType type)
    {
        try
        {
            HapticFeedback.Default.Perform(type);
        }
        catch (Exception exception) when (
            exception is FeatureNotSupportedException or InvalidOperationException)
        {
            // Haptics are feedback only; Stage output must remain available without them.
        }
    }
}

public sealed class MauiStageDisplayControl : IStageDisplayControl
{
    public void SetKeepAwake(bool enabled)
    {
        try
        {
            DeviceDisplay.Current.KeepScreenOn = enabled;
        }
        catch (Exception exception) when (
            exception is FeatureNotSupportedException or InvalidOperationException)
        {
            // Unsupported keep-awake is surfaced by platform validation, not as a mask-output failure.
        }
    }
}
