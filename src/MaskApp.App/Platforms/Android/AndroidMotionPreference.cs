using Android.Animation;
using MaskApp.App.Infrastructure.Accessibility;

namespace MaskApp.App.Platforms.Android;

public sealed class AndroidMotionPreference : IMotionPreference
{
    public bool IsReducedMotionEnabled =>
        OperatingSystem.IsAndroidVersionAtLeast(26) && !ValueAnimator.AreAnimatorsEnabled();
}
