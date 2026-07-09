using MaskApp.App.Infrastructure.Accessibility;
using UIKit;

namespace MaskApp.App.Platforms.iOS;

public sealed class IosMotionPreference : IMotionPreference
{
    public bool IsReducedMotionEnabled => UIAccessibility.IsReduceMotionEnabled;
}
