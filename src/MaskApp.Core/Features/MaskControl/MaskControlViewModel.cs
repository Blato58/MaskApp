using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.MaskControl;

public sealed class MaskControlViewModel : INotifyPropertyChanged
{
    private readonly IMaskCommandTransport transport;
    private int brightness = 60;
    private int previewBrightness = 60;
    private int restoreBrightness = 60;
    private bool isDimmed;
    private string currentEffectName = "No effect selected";
    private string statusText = "Choose a control after the mask is ready.";
    private MaskCommandTransportState transportState;
    private string transportStatusText;
    private string lastCommandText = "None";
    private string lastPayloadHex = "None";
    private string lastTransportStatusText;
    private double previewOpacity = 0.6;

    public MaskControlViewModel(IMaskCommandTransport transport)
    {
        this.transport = transport;
        transportState = transport.TransportState;
        transportStatusText = transport.TransportStatusText;
        lastTransportStatusText = transport.TransportStatusText;
        transport.TransportStateChanged += OnTransportStateChanged;

        ApplyBrightnessCommand = new AsyncRelayCommand(ApplyBrightnessAsync, CanSendCommand);
        TogglePowerCommand = new AsyncRelayCommand(TogglePowerAsync, CanSendCommand);

        EffectPresets =
        [
            CreateImagePreset("Image 1", 1),
            CreateImagePreset("Image 2", 2),
            CreateAnimationPreset("Animation 1", 1),
            CreateAnimationPreset("Animation 2", 2)
        ];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand ApplyBrightnessCommand { get; }

    public AsyncRelayCommand TogglePowerCommand { get; }

    public IReadOnlyList<MaskEffectPreset> EffectPresets { get; }

    public string ActiveTransportText =>
        transport.IsSimulated
            ? $"{transport.TransportDisplayName} (simulated)"
            : $"{transport.TransportDisplayName} (real)";

    public int Brightness
    {
        get => brightness;
        set
        {
            var clamped = Math.Clamp(value, 1, 100);
            if (SetField(ref brightness, clamped) && clamped > 1)
            {
                restoreBrightness = clamped;
            }
        }
    }

    public int PreviewBrightness
    {
        get => previewBrightness;
        private set
        {
            if (SetField(ref previewBrightness, value))
            {
                PreviewOpacity = Math.Clamp(value / 100d, 0.08d, 1d);
            }
        }
    }

    public double PreviewOpacity
    {
        get => previewOpacity;
        private set => SetField(ref previewOpacity, value);
    }

    public bool IsDimmed
    {
        get => isDimmed;
        private set
        {
            if (SetField(ref isDimmed, value))
            {
                OnPropertyChanged(nameof(PowerButtonText));
            }
        }
    }

    public string PowerButtonText => IsDimmed ? "Restore" : "Dim";

    public string CurrentEffectName
    {
        get => currentEffectName;
        private set => SetField(ref currentEffectName, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public MaskCommandTransportState TransportState
    {
        get => transportState;
        private set
        {
            if (SetField(ref transportState, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string TransportStatusText
    {
        get => transportStatusText;
        private set => SetField(ref transportStatusText, value);
    }

    public string LastCommandText
    {
        get => lastCommandText;
        private set => SetField(ref lastCommandText, value);
    }

    public string LastPayloadHex
    {
        get => lastPayloadHex;
        private set => SetField(ref lastPayloadHex, value);
    }

    public string LastTransportStatusText
    {
        get => lastTransportStatusText;
        private set => SetField(ref lastTransportStatusText, value);
    }

    private MaskEffectPreset CreateImagePreset(string name, int presetId)
    {
        return new MaskEffectPreset(
            name,
            "Image",
            presetId,
            new AsyncRelayCommand(cancellationToken => SendEffectAsync(MaskCommandBuilder.Image(presetId, name), cancellationToken), CanSendCommand));
    }

    private MaskEffectPreset CreateAnimationPreset(string name, int presetId)
    {
        return new MaskEffectPreset(
            name,
            "Animation",
            presetId,
            new AsyncRelayCommand(cancellationToken => SendEffectAsync(MaskCommandBuilder.Animation(presetId, name), cancellationToken), CanSendCommand));
    }

    private async Task ApplyBrightnessAsync(CancellationToken cancellationToken)
    {
        var targetBrightness = Math.Clamp(Brightness, 1, 100);
        var result = await SendCommandAsync(MaskCommandBuilder.Brightness(targetBrightness), cancellationToken);
        if (!result.Succeeded)
        {
            return;
        }

        Brightness = targetBrightness;
        PreviewBrightness = targetBrightness;
        IsDimmed = targetBrightness <= 1;
        if (targetBrightness > 1)
        {
            restoreBrightness = targetBrightness;
        }
    }

    private async Task TogglePowerAsync(CancellationToken cancellationToken)
    {
        var targetBrightness = IsDimmed ? restoreBrightness : 1;
        var result = await SendCommandAsync(MaskCommandBuilder.Brightness(targetBrightness), cancellationToken);
        if (!result.Succeeded)
        {
            return;
        }

        Brightness = targetBrightness;
        PreviewBrightness = targetBrightness;
        IsDimmed = targetBrightness <= 1;
    }

    private async Task SendEffectAsync(MaskCommand command, CancellationToken cancellationToken)
    {
        var result = await SendCommandAsync(command, cancellationToken);
        if (result.Succeeded)
        {
            CurrentEffectName = command.DisplayName;
        }
    }

    private async Task<MaskCommandResult> SendCommandAsync(MaskCommand command, CancellationToken cancellationToken)
    {
        if (!CanSendCommand())
        {
            StatusText = "Mask controls are not ready.";
            LastTransportStatusText = StatusText;
            return MaskCommandResult.Failure(StatusText);
        }

        LastCommandText = $"{command.Kind}: {command.DisplayName}";
        LastPayloadHex = Convert.ToHexString(command.EncryptedPayload.Span);

        var result = await transport.SendAsync(command, cancellationToken);
        StatusText = result.Message;
        LastTransportStatusText = result.Message;
        if (!result.Succeeded)
        {
            TransportState = MaskCommandTransportState.Failed;
            TransportStatusText = result.Message;
        }

        return result;
    }

    private bool CanSendCommand() => TransportState == MaskCommandTransportState.Ready;

    private void OnTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e)
    {
        TransportState = e.State;
        TransportStatusText = e.Message;
        LastTransportStatusText = e.Message;
    }

    private void RaiseCommandStates()
    {
        ApplyBrightnessCommand.RaiseCanExecuteChanged();
        TogglePowerCommand.RaiseCanExecuteChanged();
        foreach (var preset in EffectPresets)
        {
            preset.ApplyCommand.RaiseCanExecuteChanged();
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
