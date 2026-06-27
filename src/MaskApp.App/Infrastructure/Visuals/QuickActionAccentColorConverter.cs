using System.Globalization;
using MaskApp.Core.Features.QuickActions;

namespace MaskApp.App.Infrastructure.Visuals;

public sealed class QuickActionAccentColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = GetAccent(value);
        return string.Equals(parameter as string, "soft", StringComparison.OrdinalIgnoreCase)
            ? color.WithAlpha(0.18f)
            : color;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static Color GetAccent(object? value) =>
        value switch
        {
            QuickActionId.Blackout => Color.FromArgb("#FF5C54"),
            QuickActionId.RestoreBrightness => Color.FromArgb("#38BDF8"),
            QuickActionId.SetBrightness => Color.FromArgb("#FACC15"),
            QuickActionId.RandomReaction => Color.FromArgb("#A855F7"),
            QuickActionId.Drop or QuickActionId.WheelUp or QuickActionId.Reload or QuickActionId.Boh
                or QuickActionId.PullUp or QuickActionId.RunItBack or QuickActionId.BassFaceManual
                or QuickActionId.TooMuchBass or QuickActionId.NoThoughts or QuickActionId.ILiveHere => Color.FromArgb("#FACC15"),
            QuickActionId.Hydrate or QuickActionId.Water or QuickActionId.AllGood or QuickActionId.WhereWater => Color.FromArgb("#22C55E"),
            QuickActionId.TestImage1 or QuickActionId.TestImage2 or QuickActionId.TestAnimation1 or QuickActionId.TestAnimation2 => Color.FromArgb("#38BDF8"),
            QuickActionCategory.Meme => Color.FromArgb("#FACC15"),
            QuickActionCategory.Social => Color.FromArgb("#C084FC"),
            QuickActionCategory.Rave => Color.FromArgb("#FF3D8B"),
            QuickActionCategory.Welfare => Color.FromArgb("#22C55E"),
            QuickActionCategory.BuiltIn => Color.FromArgb("#38BDF8"),
            QuickActionCategory.General => Color.FromArgb("#A78BFA"),
            _ => Color.FromArgb("#52E3FF")
        };
}
