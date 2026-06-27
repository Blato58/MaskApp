#if IOS
using CoreBluetooth;
using Foundation;
using MaskApp.Core.Bluetooth;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;
using Microsoft.Maui.ApplicationModel;
using System.Runtime.InteropServices;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace MaskApp.App.Infrastructure.Bluetooth;

public sealed class IosBleAdapter : CBCentralManagerDelegate, IBleScanner, IBleDeviceConnection, IMaskCommandTransport, ITextUploadTransport
{
    private static readonly CBUUID MaskServiceUuid = CBUUID.FromString(MaskBleProtocol.ServiceUuid);
    private static readonly CBUUID GeneralWriteCharacteristicUuid = CBUUID.FromString(MaskBleProtocol.GeneralWriteCharacteristicUuid);

    private readonly Dictionary<string, CBPeripheral> peripheralsById = [];
    private CBCentralManager? centralManager;
    private CBPeripheral? connectedPeripheral;
    private CBCharacteristic? generalWriteCharacteristic;
    private CBCharacteristic? textNotifyCharacteristic;
    private MaskPeripheralDelegate? connectedPeripheralDelegate;
    private TaskCompletionSource<TextUploadAcknowledgement>? pendingTextAcknowledgement;
#if DEBUG
    private readonly ILogger<IosBleAdapter> logger;

    public IosBleAdapter(ILogger<IosBleAdapter> logger)
    {
        this.logger = logger;
    }
#endif

    public event EventHandler<DiscoveredMaskDevice>? DeviceDiscovered;
    public event EventHandler<BleScannerStateChangedEventArgs>? ScannerStateChanged;
    public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;
    public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

    public bool IsScanning { get; private set; }
    public BleConnectionState State { get; private set; } = BleConnectionState.Disconnected;
    public MaskCommandTransportState TransportState { get; private set; } = MaskCommandTransportState.Disconnected;
    public string TransportDisplayName => "iOS CoreBluetooth";
    public bool IsSimulated => false;
    public string TransportStatusText { get; private set; } = "Connect to a mask to enable controls.";
    public bool IsReady => connectedPeripheral is not null
        && generalWriteCharacteristic is not null
        && textNotifyCharacteristic is not null
        && TransportState == MaskCommandTransportState.Ready;
    public string StatusText => IsReady
        ? "Text upload ready."
        : textNotifyCharacteristic is null
            ? "Text upload ACK notifications were not found."
            : TransportStatusText;

    public Task StartScanningAsync(CancellationToken cancellationToken = default)
    {
        EnsureCentralManager();

        if (centralManager is null || centralManager.State != CBManagerState.PoweredOn)
        {
            ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(false, "Bluetooth is not ready."));
            return Task.CompletedTask;
        }

        IsScanning = true;
        ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(true, "Scanning for masks..."));
        centralManager.ScanForPeripherals(peripheralUuids: null, options: (PeripheralScanningOptions?)null);

        return Task.CompletedTask;
    }

    public Task StopScanningAsync(CancellationToken cancellationToken = default)
    {
        centralManager?.StopScan();
        IsScanning = false;
        ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(false, "Scan stopped."));
        return Task.CompletedTask;
    }

    public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
    {
        EnsureCentralManager();

        if (!peripheralsById.TryGetValue(device.Id, out var peripheral))
        {
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(BleConnectionState.Failed, "Device is no longer available."));
            SetTransportState(MaskCommandTransportState.Failed, "Device is no longer available.");
            return Task.CompletedTask;
        }

        generalWriteCharacteristic = null;
        textNotifyCharacteristic = null;
        connectedPeripheral = null;
        connectedPeripheralDelegate = null;
        pendingTextAcknowledgement?.TrySetCanceled();
        pendingTextAcknowledgement = null;
        State = BleConnectionState.Connecting;
        SetTransportState(MaskCommandTransportState.Disconnected, "Connecting to mask...");
        ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, $"Connecting to {device.Name}..."));
        centralManager?.ConnectPeripheral(peripheral);

        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        foreach (var peripheral in peripheralsById.Values)
        {
            centralManager?.CancelPeripheralConnection(peripheral);
        }

        generalWriteCharacteristic = null;
        textNotifyCharacteristic = null;
        connectedPeripheral = null;
        connectedPeripheralDelegate = null;
        pendingTextAcknowledgement?.TrySetCanceled();
        pendingTextAcknowledgement = null;
        State = BleConnectionState.Disconnected;
        SetTransportState(MaskCommandTransportState.Disconnected, "Disconnected.");
        ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, "Disconnected."));
        return Task.CompletedTask;
    }

    public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (connectedPeripheral is null || generalWriteCharacteristic is null || TransportState != MaskCommandTransportState.Ready)
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
#if DEBUG
            logger.LogDebug(ex, "Failed to write mask command {CommandKind}.", command.Kind);
#endif
            SetTransportState(MaskCommandTransportState.Failed, ex.Message);
            return Task.FromResult(MaskCommandResult.Failure(ex.Message));
        }
    }

    public async Task<TextUploadResult> UploadAsync(TextUploadPackage package, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsReady)
        {
            return TextUploadResult.Failure(StatusText, 0);
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

            WriteEncryptedCommand(package.ModeCommand);
            WriteEncryptedCommand(package.SpeedCommand);

            return TextUploadResult.Success($"Sent text upload ({framesSent} frame(s)).", framesSent);
        }
        catch (OperationCanceledException)
        {
            return TextUploadResult.Failure("Text upload was cancelled.", 0);
        }
        catch (Exception ex)
        {
#if DEBUG
            logger.LogDebug(ex, "Failed to upload text payload.");
#endif
            return TextUploadResult.Failure(ex.Message, 0);
        }
    }

    private void WriteEncryptedCommand(MaskCommand command)
    {
        if (connectedPeripheral is null || generalWriteCharacteristic is null)
        {
            throw new InvalidOperationException("Mask controls are not ready.");
        }

        var encryptedPayload = command.EncryptedPayload.ToArray();
#if DEBUG
        logger.LogDebug(
            "Writing mask command {CommandKind} with encrypted payload {PayloadHex}.",
            command.Kind,
            Convert.ToHexString(encryptedPayload));
#endif
        using var payload = NSData.FromArray(encryptedPayload);
        connectedPeripheral.WriteValue(payload, generalWriteCharacteristic, CBCharacteristicWriteType.WithoutResponse);
    }

    private void WriteTextFrame(TextUploadFrame frame)
    {
        if (connectedPeripheral is null || generalWriteCharacteristic is null)
        {
            throw new InvalidOperationException("Mask controls are not ready.");
        }

        using var payload = NSData.FromArray(frame.Data.ToArray());
        connectedPeripheral.WriteValue(payload, generalWriteCharacteristic, CBCharacteristicWriteType.WithoutResponse);
    }

    public override void UpdatedState(CBCentralManager central)
    {
        var message = central.State switch
        {
            CBManagerState.PoweredOn => "Bluetooth is ready.",
            CBManagerState.PoweredOff => "Bluetooth is off.",
            CBManagerState.Unauthorized => "Bluetooth permission is not granted.",
            CBManagerState.Unsupported => "Bluetooth LE is not supported on this device.",
            _ => "Bluetooth is not ready."
        };

        MainThread.BeginInvokeOnMainThread(() =>
            ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(IsScanning, message)));
    }

    public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber rssi)
    {
        var manufacturerData = advertisementData[CBAdvertisement.DataManufacturerDataKey] as NSData;
        if (manufacturerData is null)
        {
            return;
        }

        var advertisementPacket = BuildAdvertisementPacket(manufacturerData);
        if (BleAdvertisementMatcher.MatchProduct(advertisementPacket) == BleAdvertisementMatcher.UnknownProduct)
        {
            return;
        }

        var id = peripheral.Identifier.ToString();
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var localName = advertisementData[CBAdvertisement.DataLocalNameKey]?.ToString();
        var name = string.IsNullOrWhiteSpace(peripheral.Name) ? localName : peripheral.Name;
        name = string.IsNullOrWhiteSpace(name) ? "Shining Mask" : name;

        peripheralsById[id] = peripheral;

        var device = new DiscoveredMaskDevice(id, name, rssi.Int32Value);
        MainThread.BeginInvokeOnMainThread(() => DeviceDiscovered?.Invoke(this, device));
    }

    public override void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
    {
#if DEBUG
        logger.LogDebug(
            "Connected to BLE peripheral {PeripheralName} ({PeripheralId}).",
            peripheral.Name ?? "mask",
            peripheral.Identifier.ToString());
#endif
        State = BleConnectionState.Connected;
        BeginMaskServiceDiscovery(peripheral);
        MainThread.BeginInvokeOnMainThread(() =>
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, $"Connected to {peripheral.Name ?? "mask"}.")));
    }

#pragma warning disable CS8765
    public override void FailedToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError? error)
    {
        State = BleConnectionState.Failed;
        var message = error?.LocalizedDescription ?? "Failed to connect.";
        generalWriteCharacteristic = null;
        textNotifyCharacteristic = null;
        connectedPeripheral = null;
        connectedPeripheralDelegate = null;
        pendingTextAcknowledgement?.TrySetCanceled();
        pendingTextAcknowledgement = null;
        SetTransportState(MaskCommandTransportState.Failed, message);
        MainThread.BeginInvokeOnMainThread(() =>
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, message)));
    }

    public override void DisconnectedPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError? error)
    {
        State = BleConnectionState.Disconnected;
        var message = error?.LocalizedDescription ?? "Disconnected.";
        generalWriteCharacteristic = null;
        textNotifyCharacteristic = null;
        connectedPeripheral = null;
        connectedPeripheralDelegate = null;
        pendingTextAcknowledgement?.TrySetCanceled();
        pendingTextAcknowledgement = null;
        SetTransportState(MaskCommandTransportState.Disconnected, message);
        MainThread.BeginInvokeOnMainThread(() =>
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, message)));
    }
#pragma warning restore CS8765

    private void EnsureCentralManager()
    {
        centralManager ??= new CBCentralManager(this, null);
    }

    private void BeginMaskServiceDiscovery(CBPeripheral peripheral)
    {
        connectedPeripheral = peripheral;
        generalWriteCharacteristic = null;
        textNotifyCharacteristic = null;
        connectedPeripheralDelegate = new MaskPeripheralDelegate(this);
        peripheral.Delegate = connectedPeripheralDelegate;
        SetTransportState(MaskCommandTransportState.Discovering, "Connected. Discovering mask controls...");
#if DEBUG
        logger.LogDebug("Discovering mask service {ServiceUuid}.", MaskBleProtocol.ServiceUuid);
#endif
        peripheral.DiscoverServices([MaskServiceUuid]);
    }

    private void DiscoverGeneralWriteCharacteristic(CBPeripheral peripheral, NSError? error)
    {
        if (error is not null)
        {
            SetTransportState(MaskCommandTransportState.Failed, error.LocalizedDescription);
            return;
        }

        var services = peripheral.Services;
        if (services is null || services.Length == 0)
        {
            SetTransportState(MaskCommandTransportState.Failed, "Mask BLE service was not found.");
            return;
        }

#if DEBUG
        logger.LogDebug(
            "Discovered BLE services for {PeripheralId}: {ServiceUuids}.",
            peripheral.Identifier.ToString(),
            string.Join(", ", services.Select(service => service.UUID.ToString())));
#endif

        foreach (var service in services)
        {
            if (IsUuid(service.UUID, MaskServiceUuid))
            {
#if DEBUG
                logger.LogDebug(
                    "Discovering write characteristic {CharacteristicUuid} on service {ServiceUuid}.",
                    MaskBleProtocol.GeneralWriteCharacteristicUuid,
                    service.UUID.ToString());
#endif
                peripheral.DiscoverCharacteristics(null, service);
                return;
            }
        }

        SetTransportState(MaskCommandTransportState.Failed, "Mask BLE service was not found.");
    }

    private void CacheGeneralWriteCharacteristic(CBService service, NSError? error)
    {
        if (error is not null)
        {
            SetTransportState(MaskCommandTransportState.Failed, error.LocalizedDescription);
            return;
        }

        var characteristics = service.Characteristics;
        if (characteristics is null || characteristics.Length == 0)
        {
            SetTransportState(MaskCommandTransportState.Failed, "Mask write characteristic was not found.");
            return;
        }

#if DEBUG
        logger.LogDebug(
            "Discovered BLE characteristics for service {ServiceUuid}: {CharacteristicUuids}.",
            service.UUID.ToString(),
            string.Join(", ", characteristics.Select(characteristic => characteristic.UUID.ToString())));
#endif

        foreach (var characteristic in characteristics)
        {
            if (IsUuid(characteristic.UUID, GeneralWriteCharacteristicUuid))
            {
                generalWriteCharacteristic = characteristic;
#if DEBUG
                logger.LogDebug("Selected write characteristic {CharacteristicUuid}.", characteristic.UUID.ToString());
#endif
            }

            if (textNotifyCharacteristic is null && CanNotify(characteristic))
            {
                textNotifyCharacteristic = characteristic;
#if DEBUG
                logger.LogDebug("Selected text ACK characteristic {CharacteristicUuid}.", characteristic.UUID.ToString());
#endif
                connectedPeripheral?.SetNotifyValue(true, characteristic);
            }
        }

        if (generalWriteCharacteristic is null)
        {
            SetTransportState(MaskCommandTransportState.Failed, "Mask write characteristic was not found.");
            return;
        }

        SetTransportState(
            MaskCommandTransportState.Ready,
            textNotifyCharacteristic is null
                ? "Mask controls ready. Text upload ACK notifications were not found."
                : "Mask controls and text upload ready.");
    }

    private void SetTransportState(MaskCommandTransportState state, string message)
    {
        TransportState = state;
        TransportStatusText = message;
        MainThread.BeginInvokeOnMainThread(() =>
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message)));
    }

    private static byte[] BuildAdvertisementPacket(NSData manufacturerData)
    {
        var payload = ToByteArray(manufacturerData);
        var packet = new byte[payload.Length + 2];
        packet[0] = (byte)(payload.Length + 1);
        packet[1] = 0xFF;
        payload.CopyTo(packet, 2);
        return packet;
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
        return acknowledgement == TextUploadAcknowledgement.Error ? acknowledgement : expectedAcknowledgement == acknowledgement ? acknowledgement : TextUploadAcknowledgement.Unknown;
    }

    private void HandleTextAcknowledgement(CBCharacteristic characteristic, NSError? error)
    {
        if (error is not null || textNotifyCharacteristic is null || !IsUuid(characteristic.UUID, textNotifyCharacteristic.UUID) || characteristic.Value is null)
        {
            return;
        }

        var acknowledgement = TextUploadProtocol.ParseEncryptedAcknowledgement(ToByteArray(characteristic.Value));
        if (acknowledgement != TextUploadAcknowledgement.Unknown)
        {
            pendingTextAcknowledgement?.TrySetResult(acknowledgement);
        }
    }

    private static bool IsUuid(CBUUID actual, CBUUID expected) =>
        string.Equals(actual.ToString(), expected.ToString(), StringComparison.OrdinalIgnoreCase);

    private static bool CanNotify(CBCharacteristic characteristic) =>
        characteristic.Properties.HasFlag(CBCharacteristicProperties.Notify) ||
        characteristic.Properties.HasFlag(CBCharacteristicProperties.Indicate);

    private static byte[] ToByteArray(NSData data)
    {
        var payload = new byte[(int)data.Length];
        Marshal.Copy(data.Bytes, payload, 0, payload.Length);
        return payload;
    }

    private sealed class MaskPeripheralDelegate : CBPeripheralDelegate
    {
        private readonly IosBleAdapter owner;

        public MaskPeripheralDelegate(IosBleAdapter owner)
        {
            this.owner = owner;
        }

#pragma warning disable CS8765
        public override void DiscoveredService(CBPeripheral peripheral, NSError? error)
        {
            owner.DiscoverGeneralWriteCharacteristic(peripheral, error);
        }

        public override void DiscoveredCharacteristics(CBPeripheral peripheral, CBService service, NSError? error)
        {
            owner.CacheGeneralWriteCharacteristic(service, error);
        }

        public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError? error)
        {
            owner.HandleTextAcknowledgement(characteristic, error);
        }

        public override void UpdatedNotificationState(CBPeripheral peripheral, CBCharacteristic characteristic, NSError? error)
        {
#if DEBUG
            if (error is not null)
            {
                owner.logger.LogDebug(
                    "Text ACK notification subscription failed for {CharacteristicUuid}: {Error}.",
                    characteristic.UUID.ToString(),
                    error.LocalizedDescription);
            }
#endif
        }
#pragma warning restore CS8765
    }
}
#endif
