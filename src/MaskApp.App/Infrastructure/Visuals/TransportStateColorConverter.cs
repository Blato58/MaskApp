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
            MaskCommandTransportState.Ready => Color.FromArgb("#22D3EE"),
            MaskCommandTransportState.Discovering => Color.FromArgb("#F59E0B"),
            MaskCommandTransportState.Failed => Color.FromArgb("#EF4444"),
            TextUploadTransportState.Ready or TextUploadTransportState.CompatibilityReady or TextUploadTransportState.Simulated => Color.FromArgb("#22D3EE"),
            TextUploadTransportState.Discovering => Color.FromArgb("#F59E0B"),
            TextUploadTransportState.Failed or TextUploadTransportState.Unavailable => Color.FromArgb("#EF4444"),
            BleConnectionState.Connected => Color.FromArgb("#22D3EE"),
            BleConnectionState.Scanning or BleConnectionState.Connecting => Color.FromArgb("#F59E0B"),
            BleConnectionState.Failed or BleConnectionState.Unavailable => Color.FromArgb("#EF4444"),
            _ => Color.FromArgb("#92949B")
        };

        return string.Equals(parameter as string, "soft", StringComparison.OrdinalIgnoreCase)
            ? color.WithAlpha(0.2f)
            : color;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
