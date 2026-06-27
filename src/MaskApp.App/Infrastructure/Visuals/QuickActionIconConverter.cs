using System.Globalization;
using MaskApp.Core.Features.QuickActions;

namespace MaskApp.App.Infrastructure.Visuals;

public sealed class QuickActionIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            QuickActionId.Blackout => "OFF",
            QuickActionId.RestoreBrightness => "RST",
            QuickActionId.SetBrightness => "SUN",
            QuickActionId.RandomReaction => "RND",
            QuickActionId.Drop => "DRP",
            QuickActionId.WheelUp => "UP",
            QuickActionId.Reload => "RLD",
            QuickActionId.Boh => "BOH",
            QuickActionId.PullUp => "PUL",
            QuickActionId.RunItBack => "RUN",
            QuickActionId.BassFaceManual => "BSS",
            QuickActionId.Hydrate => "H2O",
            QuickActionId.Water => "WTR",
            QuickActionId.AllGood => "OK",
            QuickActionId.NiceMoves => "MOV",
            QuickActionId.TooMuchBass => "MAX",
            QuickActionId.NoThoughts => "NO",
            QuickActionId.WhereWater => "H2?",
            QuickActionId.ILiveHere => "LV",
            QuickActionId.TestImage1 or QuickActionId.TestImage2 => "IMG",
            QuickActionId.TestAnimation1 or QuickActionId.TestAnimation2 => "ANI",
            _ => "TXT"
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
