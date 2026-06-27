using System.Globalization;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.App.Infrastructure.Visuals;

public sealed class TransportStateColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = value switch
        {
            MaskCommandTransportState.Ready => Color.FromArgb("#22C55E"),
            MaskCommandTransportState.Discovering => Color.FromArgb("#FACC15"),
            MaskCommandTransportState.Failed => Color.FromArgb("#FF5C54"),
            TextUploadTransportState.Ready or TextUploadTransportState.CompatibilityReady or TextUploadTransportState.Simulated => Color.FromArgb("#22C55E"),
            TextUploadTransportState.Discovering => Color.FromArgb("#FACC15"),
            TextUploadTransportState.Failed or TextUploadTransportState.Unavailable => Color.FromArgb("#FF5C54"),
            BleConnectionState.Connected => Color.FromArgb("#22C55E"),
            BleConnectionState.Scanning or BleConnectionState.Connecting => Color.FromArgb("#FACC15"),
            BleConnectionState.Failed or BleConnectionState.Unavailable => Color.FromArgb("#FF5C54"),
            _ => Color.FromArgb("#64748B")
        };

        return string.Equals(parameter as string, "soft", StringComparison.OrdinalIgnoreCase)
            ? color.WithAlpha(0.2f)
            : color;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
