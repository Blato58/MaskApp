using MaskApp.Core.Features.Preflight;
using Microsoft.Maui.ApplicationModel;
#if IOS
using CoreBluetooth;
using MaskApp.App.Infrastructure.Bluetooth;
#endif

namespace MaskApp.App.Features.Preflight;

public sealed class MauiPreflightRuntimeStateProvider : IPreflightRuntimeStateProvider
{
#if IOS
    private readonly IosBleAdapter bleAdapter;

    public MauiPreflightRuntimeStateProvider(IosBleAdapter bleAdapter)
    {
        this.bleAdapter = bleAdapter;
    }
#endif

    public async Task<PreflightRuntimeSnapshot> GetSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var microphone = await Permissions
                .CheckStatusAsync<Permissions.Microphone>()
                .ConfigureAwait(false);
#if IOS
            var bluetooth = bleAdapter.CentralState switch
            {
                CBManagerState.PoweredOn or CBManagerState.PoweredOff =>
                    PreflightRuntimeAccessStatus.Granted,
                CBManagerState.Unauthorized => PreflightRuntimeAccessStatus.Denied,
                CBManagerState.Unsupported => PreflightRuntimeAccessStatus.Unavailable,
                _ => PreflightRuntimeAccessStatus.Unknown
            };
            var bluetoothDetail = bleAdapter.CentralState switch
            {
                CBManagerState.PoweredOn => "Bluetooth permission is granted and Bluetooth is on.",
                CBManagerState.PoweredOff => "Bluetooth permission is granted, but Bluetooth is off.",
                CBManagerState.Unauthorized => "Bluetooth permission is not granted.",
                CBManagerState.Unsupported => "Bluetooth LE is unavailable on this device.",
                _ => "Bluetooth permission is not yet known."
            };
#elif ANDROID
            var bluetoothPermission = await Permissions
                .CheckStatusAsync<Permissions.Bluetooth>()
                .ConfigureAwait(false);
            var locationPermission = await Permissions
                .CheckStatusAsync<Permissions.LocationWhenInUse>()
                .ConfigureAwait(false);
            var bluetooth = bluetoothPermission == PermissionStatus.Granted
                && locationPermission == PermissionStatus.Granted
                    ? PreflightRuntimeAccessStatus.Granted
                    : bluetoothPermission is PermissionStatus.Denied
                        || locationPermission is PermissionStatus.Denied
                        ? PreflightRuntimeAccessStatus.Denied
                        : PreflightRuntimeAccessStatus.Unknown;
            var bluetoothDetail = bluetooth == PreflightRuntimeAccessStatus.Granted
                ? "Bluetooth and scan-location permissions are granted."
                : "Bluetooth or scan-location permission is not granted.";
#else
            var bluetooth = PreflightRuntimeAccessStatus.Unavailable;
            var bluetoothDetail = "Bluetooth runtime access is unavailable on this platform.";
#endif
            return new PreflightRuntimeSnapshot
            {
                BluetoothAccess = bluetooth,
                BluetoothDetail = bluetoothDetail,
                MicrophoneAccess = MapPermission(microphone),
                MicrophoneDetail = microphone == PermissionStatus.Granted
                    ? "Microphone permission is granted."
                    : "Microphone permission is not granted."
            };
        }
        catch (Exception exception) when (
            exception is NotSupportedException or InvalidOperationException or UnauthorizedAccessException)
        {
            return new PreflightRuntimeSnapshot
            {
                BluetoothAccess = PreflightRuntimeAccessStatus.Unknown,
                BluetoothDetail = $"Bluetooth permission check failed: {exception.Message}",
                MicrophoneAccess = PreflightRuntimeAccessStatus.Unknown,
                MicrophoneDetail = $"Microphone permission check failed: {exception.Message}"
            };
        }
    }

    private static PreflightRuntimeAccessStatus MapPermission(PermissionStatus status) => status switch
    {
        PermissionStatus.Granted or PermissionStatus.Limited => PreflightRuntimeAccessStatus.Granted,
        PermissionStatus.Denied => PreflightRuntimeAccessStatus.Denied,
        PermissionStatus.Disabled or PermissionStatus.Restricted => PreflightRuntimeAccessStatus.Unavailable,
        _ => PreflightRuntimeAccessStatus.Unknown
    };
}
