using MaskApp.Core.Features.Stage;
using Microsoft.Maui.Devices;

namespace MaskApp.App.Features.Stage;

public sealed class MauiStageDeviceFeedback : IStageDeviceFeedback
{
    public void Success() => Perform(HapticFeedbackType.Click);

    public void Failure() => Perform(HapticFeedbackType.LongPress);

    public void Warning() => Perform(HapticFeedbackType.Click);

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
