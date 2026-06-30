#if ANDROID
using Android;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Java.Util;
using MaskApp.Core.Bluetooth;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;

namespace MaskApp.App.Infrastructure.Bluetooth;

public sealed class AndroidBleAdapter : IBleScanner, IBleDeviceConnection, IMaskCommandTransport, ITextUploadTransport, IFaceUploadTransport
{
    private static readonly UUID MaskServiceUuid = UUID.FromString(MaskBleProtocol.ServiceUuid)!;
    private static readonly UUID CommandCharacteristicUuid = UUID.FromString(MaskBleProtocol.CommandCharacteristicUuid)!;
    private static readonly UUID TextUploadCharacteristicUuid = UUID.FromString(MaskBleProtocol.TextUploadCharacteristicUuid)!;
    private static readonly UUID NotificationCharacteristicUuid = UUID.FromString(MaskBleProtocol.NotificationCharacteristicUuid)!;
    private static readonly UUID ClientCharacteristicConfigurationUuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb")!;

    private readonly Dictionary<string, BluetoothDevice> devicesById = [];
    private readonly ILogger<AndroidBleAdapter> logger;
    private BluetoothAdapter? bluetoothAdapter;
    private BluetoothLeScanner? bluetoothLeScanner;
    private BleScanCallback? scanCallback;
    private BluetoothGatt? connectedGatt;
    private BluetoothGattCharacteristic? generalWriteCharacteristic;
    private BluetoothGattCharacteristic? textUploadWriteCharacteristic;
    private BluetoothGattCharacteristic? textNotifyCharacteristic;
    private AndroidGattCallback? gattCallback;
    private TaskCompletionSource<TextUploadAcknowledgement>? pendingTextAcknowledgement;
    private TaskCompletionSource<FaceUploadAcknowledgement>? pendingFaceAcknowledgement;
    private TextUploadTransportState textUploadState = TextUploadTransportState.Disconnected;
    private FaceUploadTransportState faceUploadState = FaceUploadTransportState.Disconnected;
    private event EventHandler<TextUploadTransportStateChangedEventArgs>? TextUploadStateChanged;
    private event EventHandler<FaceUploadTransportStateChangedEventArgs>? FaceUploadStateChanged;

    public AndroidBleAdapter(ILogger<AndroidBleAdapter> logger)
    {
        this.logger = logger;
    }

    public event EventHandler<DiscoveredMaskDevice>? DeviceDiscovered;
    public event EventHandler<BleScannerStateChangedEventArgs>? ScannerStateChanged;
    public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;
    public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;
    event EventHandler<TextUploadTransportStateChangedEventArgs>? ITextUploadTransport.StateChanged
    {
        add => TextUploadStateChanged += value;
        remove => TextUploadStateChanged -= value;
    }
    event EventHandler<FaceUploadTransportStateChangedEventArgs>? IFaceUploadTransport.StateChanged
    {
        add => FaceUploadStateChanged += value;
        remove => FaceUploadStateChanged -= value;
    }

    public bool IsScanning { get; private set; }
    public BleConnectionState State { get; private set; } = BleConnectionState.Disconnected;
    public MaskCommandTransportState TransportState { get; private set; } = MaskCommandTransportState.Disconnected;
    public string TransportDisplayName => "Android Bluetooth LE";
    public bool IsSimulated => false;
    public string TransportStatusText { get; private set; } = "Connect to a mask to enable controls.";
    public bool IsReady => connectedGatt is not null
        && generalWriteCharacteristic is not null
        && TransportState == MaskCommandTransportState.Ready;
    public bool SupportsAcknowledgements => textNotifyCharacteristic is not null;
    TextUploadTransportState ITextUploadTransport.State => textUploadState;
    FaceUploadTransportState IFaceUploadTransport.State => faceUploadState;
    public string StatusText => textUploadState switch
    {
        TextUploadTransportState.Ready => "Text upload ready with ACK confirmation.",
        TextUploadTransportState.CompatibilityReady => "Text upload write-only compatibility ready. ACK notifications were not found.",
        _ => TransportStatusText
    };

    public async Task StartScanningAsync(CancellationToken cancellationToken = default)
    {
        if (!await EnsurePermissionsAsync(cancellationToken).ConfigureAwait(false))
        {
            SetScannerState(false, "Bluetooth or location permission is not granted.");
            return;
        }

        if (!EnsureBluetoothAdapter())
        {
            SetScannerState(false, "Bluetooth LE is not available on this device.");
            return;
        }

        if (bluetoothAdapter?.IsEnabled != true)
        {
            SetScannerState(false, "Bluetooth is off.");
            return;
        }

        bluetoothLeScanner = bluetoothAdapter.BluetoothLeScanner;
        if (bluetoothLeScanner is null)
        {
            SetScannerState(false, "Android BLE scanner is unavailable.");
            return;
        }

        devicesById.Clear();
        scanCallback = new BleScanCallback(this);
        IsScanning = true;
        SetScannerState(true, "Scanning for masks...");
        bluetoothLeScanner.StartScan(scanCallback);
    }

    public Task StopScanningAsync(CancellationToken cancellationToken = default)
    {
        if (scanCallback is not null)
        {
            bluetoothLeScanner?.StopScan(scanCallback);
            scanCallback.Dispose();
            scanCallback = null;
        }

        IsScanning = false;
        SetScannerState(false, "Scan stopped.");
        return Task.CompletedTask;
    }

    public async Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
    {
        if (!await EnsurePermissionsAsync(cancellationToken).ConfigureAwait(false))
        {
            State = BleConnectionState.Failed;
            RaiseConnectionState(State, "Bluetooth or location permission is not granted.");
            SetTransportState(MaskCommandTransportState.Failed, "Bluetooth or location permission is not granted.");
            return;
        }

        if (!devicesById.TryGetValue(device.Id, out var bluetoothDevice))
        {
            State = BleConnectionState.Failed;
            RaiseConnectionState(State, "Device is no longer available.");
            SetTransportState(MaskCommandTransportState.Failed, "Device is no longer available.");
            return;
        }

        await StopScanningAsync(cancellationToken).ConfigureAwait(false);
        ResetConnectedGatt(closeGatt: true);
        State = BleConnectionState.Connecting;
        SetTransportState(MaskCommandTransportState.Disconnected, "Connecting to mask...");
        RaiseConnectionState(State, $"Connecting to {device.Name}...");

        gattCallback = new AndroidGattCallback(this);
        connectedGatt = bluetoothDevice.ConnectGatt(
            Platform.AppContext,
            false,
            gattCallback,
            BluetoothTransports.Le);
        if (connectedGatt is null)
        {
            State = BleConnectionState.Failed;
            SetTransportState(MaskCommandTransportState.Failed, "Android GATT connection could not be created.");
            RaiseConnectionState(State, "Android GATT connection could not be created.");
        }
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ResetConnectedGatt(closeGatt: true);
        State = BleConnectionState.Disconnected;
        SetTransportState(MaskCommandTransportState.Disconnected, "Disconnected.");
        RaiseConnectionState(State, "Disconnected.");
        return Task.CompletedTask;
    }

    public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (connectedGatt is null || generalWriteCharacteristic is null || TransportState != MaskCommandTransportState.Ready)
        {
            return Task.FromResult(MaskCommandResult.Failure("Mask controls are not ready."));
        }

        try
        {
            WriteEncryptedCommand(command);
            return Task.FromResult(MaskCommandResult.Success($"Sent {command.DisplayName}."));
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to write Android mask command {CommandKind}.", command.Kind);
            SetTransportState(MaskCommandTransportState.Failed, ex.Message);
            return Task.FromResult(MaskCommandResult.Failure(ex.Message));
        }
    }

    public async Task<TextUploadResult> UploadAsync(
        TextUploadPackage package,
        TextUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsReady)
        {
            return TextUploadResult.Failure(StatusText, 0);
        }

        await ResetTextDisplayIfRequestedAsync(options, cancellationToken).ConfigureAwait(false);
        await SendPreUploadCommandsAsync(package, options, cancellationToken).ConfigureAwait(false);

        if (options.CompatibilityWriteOnly || !options.AckRequired)
        {
            return await UploadWriteOnlyAsync(package, options, cancellationToken).ConfigureAwait(false);
        }

        if (!SupportsAcknowledgements)
        {
            return TextUploadResult.Failure(
                "Text upload ACK notifications are unavailable. Enable write-only compatibility mode to send without confirmation.",
                0);
        }

        try
        {
            var startAck = await WriteCommandAndWaitAsync(
                package.StartCommand,
                TextUploadAcknowledgement.StartAccepted,
                cancellationToken).ConfigureAwait(false);
            if (startAck != TextUploadAcknowledgement.StartAccepted)
            {
                return TextUploadResult.Failure($"Text upload start failed: {startAck}.", 0);
            }

            var framesSent = 0;
            foreach (var frame in package.Frames)
            {
                var frameAckTask = WaitForTextAcknowledgementAsync(
                    TextUploadAcknowledgement.FrameAccepted,
                    cancellationToken);
                WriteTextFrame(frame);
                var frameAck = await frameAckTask.ConfigureAwait(false);
                if (frameAck != TextUploadAcknowledgement.FrameAccepted)
                {
                    return TextUploadResult.Failure($"Text frame {frame.Index} failed: {frameAck}.", framesSent);
                }

                framesSent++;
            }

            var finishAck = await WriteCommandAndWaitAsync(
                package.FinishCommand,
                TextUploadAcknowledgement.Complete,
                cancellationToken).ConfigureAwait(false);
            if (finishAck != TextUploadAcknowledgement.Complete)
            {
                return TextUploadResult.Failure($"Text upload finish failed: {finishAck}.", framesSent);
            }

            var postUploadStatus = await SendPostUploadCommandsAsync(package, options, cancellationToken).ConfigureAwait(false);

            return TextUploadResult.Success($"Sent text upload ({framesSent} frame(s)).{postUploadStatus}", framesSent);
        }
        catch (System.OperationCanceledException)
        {
            return TextUploadResult.Failure("Text upload was cancelled.", 0);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to upload Android text payload.");
            return TextUploadResult.Failure(ex.Message, 0);
        }
    }

    private async Task<TextUploadResult> UploadWriteOnlyAsync(
        TextUploadPackage package,
        TextUploadOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            WriteEncryptedCommand(package.StartCommand);
            await DelayBetweenTextWritesAsync(options, cancellationToken).ConfigureAwait(false);

            var framesSent = 0;
            foreach (var frame in package.Frames)
            {
                WriteTextFrame(frame);
                framesSent++;
                await DelayBetweenTextWritesAsync(options, cancellationToken).ConfigureAwait(false);
            }

            WriteEncryptedCommand(package.FinishCommand);
            var postUploadStatus = await SendPostUploadCommandsAsync(package, options, cancellationToken).ConfigureAwait(false);

            return TextUploadResult.Success(
                $"Sent text upload without ACK confirmation ({framesSent} frame(s)).{postUploadStatus}",
                framesSent);
        }
        catch (System.OperationCanceledException)
        {
            return TextUploadResult.Failure("Text upload was cancelled.", 0);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to upload Android text payload without ACK confirmation.");
            return TextUploadResult.Failure(ex.Message, 0);
        }
    }

    public async Task<FaceUploadResult> UploadAsync(
        FaceUploadPackage package,
        FaceUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsReady)
        {
            return FaceUploadResult.Failure(StatusText, 0);
        }

        if (options.CompatibilityWriteOnly || !options.AckRequired)
        {
            return await UploadFaceWriteOnlyAsync(package, options, cancellationToken).ConfigureAwait(false);
        }

        if (!SupportsAcknowledgements)
        {
            return FaceUploadResult.Failure(
                "Face upload ACK notifications are unavailable. Enable write-only compatibility mode to send without confirmation.",
                0);
        }

        try
        {
            var startAck = await WriteFaceCommandAndWaitAsync(
                package.StartCommand,
                FaceUploadAcknowledgement.StartAccepted,
                cancellationToken).ConfigureAwait(false);
            if (startAck != FaceUploadAcknowledgement.StartAccepted)
            {
                return FaceUploadResult.Failure($"Face upload start failed: {startAck}.", 0);
            }

            var framesSent = 0;
            foreach (var frame in package.Frames)
            {
                var frameAckTask = WaitForFaceAcknowledgementAsync(
                    FaceUploadAcknowledgement.FrameAccepted,
                    cancellationToken);
                WriteFaceFrame(frame);
                var frameAck = await frameAckTask.ConfigureAwait(false);
                if (frameAck != FaceUploadAcknowledgement.FrameAccepted)
                {
                    return FaceUploadResult.Failure($"Face frame {frame.Index} failed: {frameAck}.", framesSent);
                }

                framesSent++;
            }

            var finishAck = await WriteFaceCommandAndWaitAsync(
                package.FinishCommand,
                FaceUploadAcknowledgement.Complete,
                cancellationToken).ConfigureAwait(false);
            if (finishAck != FaceUploadAcknowledgement.Complete)
            {
                return FaceUploadResult.Failure($"Face upload finish failed: {finishAck}.", framesSent);
            }

            await DelayFaceWriteAsync(options.PostUploadDelay, cancellationToken).ConfigureAwait(false);
            var playAck = await WriteFaceCommandAndWaitAsync(
                package.PlayCommand,
                FaceUploadAcknowledgement.PlayAccepted,
                cancellationToken).ConfigureAwait(false);
            if (playAck != FaceUploadAcknowledgement.PlayAccepted)
            {
                return FaceUploadResult.Failure($"Face play failed: {playAck}.", framesSent);
            }

            return FaceUploadResult.Success($"Uploaded and played DIY face slot {package.Slot} ({framesSent} frame(s)).", framesSent);
        }
        catch (System.OperationCanceledException)
        {
            return FaceUploadResult.Failure("Face upload was cancelled.", 0);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to upload Android face payload.");
            return FaceUploadResult.Failure(ex.Message, 0);
        }
    }

    private async Task<FaceUploadResult> UploadFaceWriteOnlyAsync(
        FaceUploadPackage package,
        FaceUploadOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            WriteEncryptedCommand(package.StartCommand);
            await DelayBetweenFaceWritesAsync(options, cancellationToken).ConfigureAwait(false);

            var framesSent = 0;
            foreach (var frame in package.Frames)
            {
                WriteFaceFrame(frame);
                framesSent++;
                await DelayBetweenFaceWritesAsync(options, cancellationToken).ConfigureAwait(false);
            }

            WriteEncryptedCommand(package.FinishCommand);
            await DelayFaceWriteAsync(options.PostUploadDelay, cancellationToken).ConfigureAwait(false);
            WriteEncryptedCommand(package.PlayCommand);

            return FaceUploadResult.Success(
                $"Sent DIY face slot {package.Slot} without ACK confirmation ({framesSent} frame(s)).",
                framesSent);
        }
        catch (System.OperationCanceledException)
        {
            return FaceUploadResult.Failure("Face upload was cancelled.", 0);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to upload Android face payload without ACK confirmation.");
            return FaceUploadResult.Failure(ex.Message, 0);
        }
    }

    private async Task ResetTextDisplayIfRequestedAsync(TextUploadOptions options, CancellationToken cancellationToken)
    {
        if (!options.ResetDisplayBeforeUpload)
        {
            return;
        }

        WriteEncryptedCommand(MaskCommandBuilder.TextMode(1));
        await DelayTextWriteAsync(options.DisplayResetDelay, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> SendPostUploadCommandsAsync(
        TextUploadPackage package,
        TextUploadOptions options,
        CancellationToken cancellationToken)
    {
        await DelayTextWriteAsync(options.PostUploadDelay, cancellationToken).ConfigureAwait(false);

        var styleSkipped = false;
        var steps = TextUploadCommandSequence.CreatePostUploadSteps(package, options);
        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            try
            {
                WriteEncryptedCommand(step.Command);
            }
            catch when (step.FailSoft)
            {
                styleSkipped = true;
            }

            if (i < steps.Count - 1)
            {
                await DelayTextWriteAsync(options.CommandDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        return styleSkipped ? " Style skipped." : string.Empty;
    }

    private async Task SendPreUploadCommandsAsync(
        TextUploadPackage package,
        TextUploadOptions options,
        CancellationToken cancellationToken)
    {
        foreach (var step in TextUploadCommandSequence.CreatePreUploadSteps(package, options))
        {
            WriteEncryptedCommand(step.Command);
            await DelayTextWriteAsync(options.CommandDelay, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> EnsurePermissionsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var bluetoothStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>().ConfigureAwait(false);
        if (bluetoothStatus != PermissionStatus.Granted)
        {
            bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>().ConfigureAwait(false);
        }

        if (bluetoothStatus != PermissionStatus.Granted)
        {
            return false;
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            return true;
        }

        var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
        if (locationStatus != PermissionStatus.Granted)
        {
            locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
        }

        return locationStatus == PermissionStatus.Granted;
    }

    private bool EnsureBluetoothAdapter()
    {
        if (bluetoothAdapter is not null)
        {
            return true;
        }

        var bluetoothManager = Platform.AppContext.GetSystemService(Context.BluetoothService) as BluetoothManager;
        bluetoothAdapter = bluetoothManager?.Adapter;
        return bluetoothAdapter is not null;
    }

    private void HandleScanResult(ScanResult? result)
    {
        var device = result?.Device;
        var advertisementData = result?.ScanRecord?.GetBytes();
        if (device?.Address is null || advertisementData is null)
        {
            return;
        }

        if (BleAdvertisementMatcher.MatchProduct(advertisementData) == BleAdvertisementMatcher.UnknownProduct)
        {
            return;
        }

        var id = device.Address;
        var name = result?.ScanRecord?.DeviceName;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = device.Name;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Shining Mask";
        }

        devicesById[id] = device;
        var discoveredDevice = new DiscoveredMaskDevice(id, name, result?.Rssi ?? 0);
        MainThread.BeginInvokeOnMainThread(() => DeviceDiscovered?.Invoke(this, discoveredDevice));
    }

    private void HandleScanFailed(ScanFailure errorCode)
    {
        IsScanning = false;
        SetScannerState(false, $"Android BLE scan failed: {errorCode}.");
    }

    private void HandleConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
    {
        if (newState == ProfileState.Connected && status == GattStatus.Success && gatt is not null)
        {
            connectedGatt = gatt;
            State = BleConnectionState.Connected;
            SetTransportState(MaskCommandTransportState.Discovering, "Connected. Discovering mask controls...");
            RaiseConnectionState(State, $"Connected to {gatt.Device?.Name ?? "mask"}.");
            gatt.DiscoverServices();
            return;
        }

        if (newState == ProfileState.Disconnected)
        {
            ResetConnectedGatt(closeGatt: true);
            State = status == GattStatus.Success ? BleConnectionState.Disconnected : BleConnectionState.Failed;
            var message = status == GattStatus.Success ? "Disconnected." : $"Disconnected with Android GATT status {status}.";
            SetTransportState(
                State == BleConnectionState.Failed ? MaskCommandTransportState.Failed : MaskCommandTransportState.Disconnected,
                message);
            RaiseConnectionState(State, message);
            return;
        }

        if (status != GattStatus.Success)
        {
            ResetConnectedGatt(closeGatt: true);
            State = BleConnectionState.Failed;
            var message = $"Android GATT connection failed: {status}.";
            SetTransportState(MaskCommandTransportState.Failed, message);
            RaiseConnectionState(State, message);
        }
    }

    private void HandleServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
    {
        if (gatt is null)
        {
            SetTransportState(MaskCommandTransportState.Failed, "Android GATT connection is unavailable.");
            return;
        }

        if (status != GattStatus.Success)
        {
            SetTransportState(MaskCommandTransportState.Failed, $"Android service discovery failed: {status}.");
            return;
        }

        var service = gatt.GetService(MaskServiceUuid);
        if (service is null)
        {
            SetTransportState(MaskCommandTransportState.Failed, "Mask BLE service was not found.");
            return;
        }

        generalWriteCharacteristic = service.GetCharacteristic(CommandCharacteristicUuid);
        if (generalWriteCharacteristic is null)
        {
            SetTransportState(MaskCommandTransportState.Failed, "Mask write characteristic was not found.");
            return;
        }

        textUploadWriteCharacteristic = service.GetCharacteristic(TextUploadCharacteristicUuid);
        var notifyCharacteristic = service.GetCharacteristic(NotificationCharacteristicUuid);
        if (notifyCharacteristic is null || !CanNotify(notifyCharacteristic))
        {
            notifyCharacteristic = service.Characteristics?.FirstOrDefault(CanNotify);
        }

        if (notifyCharacteristic is not null && EnableNotifications(gatt, notifyCharacteristic))
        {
            textNotifyCharacteristic = notifyCharacteristic;
        }

        SetTransportState(
            MaskCommandTransportState.Ready,
            textNotifyCharacteristic is null
                ? "Mask controls ready. Text upload ACK notifications were not found."
                : textUploadWriteCharacteristic is null
                    ? "Mask controls and text ACK ready. Text frames will use command-characteristic compatibility."
                : "Mask controls and text upload ready.");
    }

    private void HandleCharacteristicChanged(BluetoothGattCharacteristic? characteristic, byte[]? value)
    {
        if (characteristic is null || textNotifyCharacteristic is null || !UuidEquals(characteristic.Uuid, textNotifyCharacteristic.Uuid))
        {
            return;
        }

        if (value is null)
        {
            return;
        }

        var acknowledgement = TextUploadProtocol.ParseEncryptedAcknowledgement(value);
        if (acknowledgement != TextUploadAcknowledgement.Unknown)
        {
            pendingTextAcknowledgement?.TrySetResult(acknowledgement);
        }

        var faceAcknowledgement = FaceUploadProtocol.ParseEncryptedAcknowledgement(value);
        if (faceAcknowledgement != FaceUploadAcknowledgement.Unknown)
        {
            pendingFaceAcknowledgement?.TrySetResult(faceAcknowledgement);
        }
    }

    private bool EnableNotifications(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
    {
        if (!gatt.SetCharacteristicNotification(characteristic, true))
        {
            logger.LogDebug("Android BLE notification routing did not start.");
            return false;
        }

        var descriptor = characteristic.GetDescriptor(ClientCharacteristicConfigurationUuid);
        if (descriptor is null)
        {
            logger.LogDebug("Text ACK characteristic has no CCCD descriptor.");
            return false;
        }

        var value = BluetoothGattDescriptor.EnableNotificationValue?.ToArray() ?? [];
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var status = gatt.WriteDescriptor(descriptor, value);
            if (status != (int)CurrentBluetoothStatusCodes.Success)
            {
                logger.LogDebug("Android BLE CCCD descriptor write did not start: {Status}.", status);
                return false;
            }

            return true;
        }

        descriptor.SetValue(value);
        if (!gatt.WriteDescriptor(descriptor))
        {
            logger.LogDebug("Android BLE CCCD descriptor write did not start.");
            return false;
        }

        return true;
    }

    private void WriteEncryptedCommand(MaskCommand command)
    {
        WriteBytes(command.EncryptedPayload.ToArray(), $"mask command {command.Kind}");
    }

    private void WriteTextFrame(TextUploadFrame frame)
    {
        WriteBytes(frame.Data.ToArray(), $"text frame {frame.Index}", textUploadWriteCharacteristic ?? generalWriteCharacteristic);
    }

    private void WriteFaceFrame(FaceUploadFrame frame)
    {
        WriteBytes(frame.Data.ToArray(), $"face frame {frame.Index}", textUploadWriteCharacteristic ?? generalWriteCharacteristic);
    }

    private void WriteBytes(byte[] payload, string description) =>
        WriteBytes(payload, description, generalWriteCharacteristic);

    private void WriteBytes(byte[] payload, string description, BluetoothGattCharacteristic? characteristic)
    {
        if (connectedGatt is null || characteristic is null)
        {
            throw new InvalidOperationException("Mask controls are not ready.");
        }

        logger.LogDebug(
            "Writing Android BLE {Description} with payload {PayloadHex}.",
            description,
            Convert.ToHexString(payload));

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var status = connectedGatt.WriteCharacteristic(
                characteristic,
                payload,
                (int)GattWriteType.NoResponse);
            if (status != (int)CurrentBluetoothStatusCodes.Success)
            {
                throw new InvalidOperationException($"Android BLE write did not start for {description}: {status}.");
            }

            return;
        }

        characteristic.WriteType = GattWriteType.NoResponse;
        characteristic.SetValue(payload);
        if (!connectedGatt.WriteCharacteristic(characteristic))
        {
            throw new InvalidOperationException($"Android BLE write did not start for {description}.");
        }
    }

    private async Task<TextUploadAcknowledgement> WriteCommandAndWaitAsync(
        MaskCommand command,
        TextUploadAcknowledgement expectedAcknowledgement,
        CancellationToken cancellationToken)
    {
        var acknowledgementTask = WaitForTextAcknowledgementAsync(expectedAcknowledgement, cancellationToken);
        WriteEncryptedCommand(command);
        return await acknowledgementTask.ConfigureAwait(false);
    }

    private async Task<TextUploadAcknowledgement> WaitForTextAcknowledgementAsync(
        TextUploadAcknowledgement expectedAcknowledgement,
        CancellationToken cancellationToken)
    {
        var acknowledgementSource = new TaskCompletionSource<TextUploadAcknowledgement>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        pendingTextAcknowledgement = acknowledgementSource;

        using var cancellationRegistration = cancellationToken.Register(() =>
            acknowledgementSource.TrySetCanceled(cancellationToken));
        var completedTask = await Task.WhenAny(
            acknowledgementSource.Task,
            Task.Delay(TimeSpan.FromSeconds(5), cancellationToken)).ConfigureAwait(false);

        if (completedTask != acknowledgementSource.Task)
        {
            pendingTextAcknowledgement = null;
            return TextUploadAcknowledgement.Unknown;
        }

        var acknowledgement = await acknowledgementSource.Task.ConfigureAwait(false);
        pendingTextAcknowledgement = null;
        return acknowledgement == TextUploadAcknowledgement.Error
            ? acknowledgement
            : expectedAcknowledgement == acknowledgement
                ? acknowledgement
                : TextUploadAcknowledgement.Unknown;
    }

    private async Task<FaceUploadAcknowledgement> WriteFaceCommandAndWaitAsync(
        MaskCommand command,
        FaceUploadAcknowledgement expectedAcknowledgement,
        CancellationToken cancellationToken)
    {
        var acknowledgementTask = WaitForFaceAcknowledgementAsync(expectedAcknowledgement, cancellationToken);
        WriteEncryptedCommand(command);
        return await acknowledgementTask.ConfigureAwait(false);
    }

    private async Task<FaceUploadAcknowledgement> WaitForFaceAcknowledgementAsync(
        FaceUploadAcknowledgement expectedAcknowledgement,
        CancellationToken cancellationToken)
    {
        var acknowledgementSource = new TaskCompletionSource<FaceUploadAcknowledgement>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        pendingFaceAcknowledgement = acknowledgementSource;

        using var cancellationRegistration = cancellationToken.Register(() =>
            acknowledgementSource.TrySetCanceled(cancellationToken));
        var completedTask = await Task.WhenAny(
            acknowledgementSource.Task,
            Task.Delay(TimeSpan.FromSeconds(5), cancellationToken)).ConfigureAwait(false);

        if (completedTask != acknowledgementSource.Task)
        {
            pendingFaceAcknowledgement = null;
            return FaceUploadAcknowledgement.Unknown;
        }

        var acknowledgement = await acknowledgementSource.Task.ConfigureAwait(false);
        pendingFaceAcknowledgement = null;
        return acknowledgement == FaceUploadAcknowledgement.Error
            ? acknowledgement
            : expectedAcknowledgement == acknowledgement
                ? acknowledgement
                : FaceUploadAcknowledgement.Unknown;
    }

    private void ResetConnectedGatt(bool closeGatt)
    {
        pendingTextAcknowledgement?.TrySetCanceled();
        pendingTextAcknowledgement = null;
        pendingFaceAcknowledgement?.TrySetCanceled();
        pendingFaceAcknowledgement = null;
        generalWriteCharacteristic = null;
        textUploadWriteCharacteristic = null;
        textNotifyCharacteristic = null;

        if (connectedGatt is not null)
        {
            connectedGatt.Disconnect();
            if (closeGatt)
            {
                connectedGatt.Close();
                connectedGatt.Dispose();
            }
        }

        connectedGatt = null;
        gattCallback?.Dispose();
        gattCallback = null;
    }

    private void SetScannerState(bool isScanning, string message)
    {
        IsScanning = isScanning;
        MainThread.BeginInvokeOnMainThread(() =>
            ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(isScanning, message)));
    }

    private void RaiseConnectionState(BleConnectionState state, string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(state, message)));
    }

    private void SetTransportState(MaskCommandTransportState state, string message)
    {
        TransportState = state;
        TransportStatusText = message;
        RefreshTextUploadState(message);
        RefreshFaceUploadState(message);
        MainThread.BeginInvokeOnMainThread(() =>
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message)));
    }

    private void RefreshTextUploadState(string fallbackMessage)
    {
        var state = TransportState switch
        {
            MaskCommandTransportState.Ready when IsReady && SupportsAcknowledgements => TextUploadTransportState.Ready,
            MaskCommandTransportState.Ready when IsReady => TextUploadTransportState.CompatibilityReady,
            MaskCommandTransportState.Discovering => TextUploadTransportState.Discovering,
            MaskCommandTransportState.Failed => TextUploadTransportState.Failed,
            _ => TextUploadTransportState.Disconnected
        };

        var message = state switch
        {
            TextUploadTransportState.Ready => "Text upload ready with ACK confirmation.",
            TextUploadTransportState.CompatibilityReady => "Text upload write-only compatibility ready. ACK notifications were not found.",
            _ => fallbackMessage
        };

        textUploadState = state;
        MainThread.BeginInvokeOnMainThread(() =>
            TextUploadStateChanged?.Invoke(
                this,
                new TextUploadTransportStateChangedEventArgs(
                    state,
                    message,
                    SupportsAcknowledgements,
                    IsReady)));
    }

    private void RefreshFaceUploadState(string fallbackMessage)
    {
        var state = TransportState switch
        {
            MaskCommandTransportState.Ready when IsReady && SupportsAcknowledgements => FaceUploadTransportState.Ready,
            MaskCommandTransportState.Ready when IsReady => FaceUploadTransportState.CompatibilityReady,
            MaskCommandTransportState.Discovering => FaceUploadTransportState.Discovering,
            MaskCommandTransportState.Failed => FaceUploadTransportState.Failed,
            _ => FaceUploadTransportState.Disconnected
        };

        var message = state switch
        {
            FaceUploadTransportState.Ready => "Face upload ready with ACK confirmation.",
            FaceUploadTransportState.CompatibilityReady => "Face upload write-only compatibility ready. ACK notifications were not found.",
            _ => fallbackMessage
        };

        faceUploadState = state;
        MainThread.BeginInvokeOnMainThread(() =>
            FaceUploadStateChanged?.Invoke(
                this,
                new FaceUploadTransportStateChangedEventArgs(
                    state,
                    message,
                    SupportsAcknowledgements,
                    IsReady)));
    }

    private static Task DelayBetweenTextWritesAsync(TextUploadOptions options, CancellationToken cancellationToken) =>
        DelayTextWriteAsync(options.InterFrameDelay, cancellationToken);

    private static Task DelayTextWriteAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        delay > TimeSpan.Zero
            ? Task.Delay(delay, cancellationToken)
            : Task.CompletedTask;

    private static Task DelayBetweenFaceWritesAsync(FaceUploadOptions options, CancellationToken cancellationToken) =>
        DelayFaceWriteAsync(options.InterFrameDelay, cancellationToken);

    private static Task DelayFaceWriteAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        delay > TimeSpan.Zero
            ? Task.Delay(delay, cancellationToken)
            : Task.CompletedTask;

    private static bool CanNotify(BluetoothGattCharacteristic characteristic) =>
        characteristic.Properties.HasFlag(GattProperty.Notify) ||
        characteristic.Properties.HasFlag(GattProperty.Indicate);

    private static bool UuidEquals(UUID? actual, UUID? expected) =>
        actual is not null
        && expected is not null
        && string.Equals(actual.ToString(), expected.ToString(), StringComparison.OrdinalIgnoreCase);

    private sealed class BleScanCallback : ScanCallback
    {
        private readonly AndroidBleAdapter owner;

        public BleScanCallback(AndroidBleAdapter owner)
        {
            this.owner = owner;
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
        {
            owner.HandleScanResult(result);
        }

        public override void OnBatchScanResults(IList<ScanResult>? results)
        {
            if (results is null)
            {
                return;
            }

            foreach (var result in results)
            {
                owner.HandleScanResult(result);
            }
        }

        public override void OnScanFailed(ScanFailure errorCode)
        {
            owner.HandleScanFailed(errorCode);
        }
    }

    private sealed class AndroidGattCallback : BluetoothGattCallback
    {
        private readonly AndroidBleAdapter owner;

        public AndroidGattCallback(AndroidBleAdapter owner)
        {
            this.owner = owner;
        }

        public override void OnConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
        {
            owner.HandleConnectionStateChange(gatt, status, newState);
        }

        public override void OnServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
        {
            owner.HandleServicesDiscovered(gatt, status);
        }

        public override void OnCharacteristicChanged(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                owner.HandleCharacteristicChanged(characteristic, characteristic?.GetValue());
            }
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value)
        {
            owner.HandleCharacteristicChanged(characteristic, value);
        }
    }
}
#endif
