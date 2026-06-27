using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.BuiltIns;

public sealed class BuiltInsViewModel : INotifyPropertyChanged
{
    private readonly IMaskCommandTransport transport;
    private BuiltInScannerMode mode = BuiltInScannerMode.StaticImage;
    private int currentId = 1;
    private bool isSending;
    private string statusText = "Choose a built-in ID and send it to a connected mask.";
    private string lastCommandText = "None";

    public BuiltInsViewModel(IMaskCommandTransport transport)
    {
        this.transport = transport;
        transport.TransportStateChanged += OnTransportStateChanged;

        SelectStaticImageCommand = new AsyncRelayCommand(SelectStaticImageAsync);
        SelectAnimationCommand = new AsyncRelayCommand(SelectAnimationAsync);
        PreviousCommand = new AsyncRelayCommand(PreviousAsync, CanStepPrevious);
        NextCommand = new AsyncRelayCommand(NextAsync, CanStepNext);
        SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync, CanSend);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand SelectStaticImageCommand { get; }

    public AsyncRelayCommand SelectAnimationCommand { get; }

    public AsyncRelayCommand PreviousCommand { get; }

    public AsyncRelayCommand NextCommand { get; }

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand BlackoutCommand { get; }

    public BuiltInScannerMode Mode
    {
        get => mode;
        private set
        {
            if (SetField(ref mode, value))
            {
                CurrentId = Math.Min(CurrentId, MaxId);
                OnPropertyChanged(nameof(ModeText));
                OnPropertyChanged(nameof(IsStaticImageSelected));
                OnPropertyChanged(nameof(IsAnimationSelected));
                OnPropertyChanged(nameof(MaxId));
                OnPropertyChanged(nameof(RangeNote));
                OnPropertyChanged(nameof(SendButtonText));
                RaiseCommandStates();
            }
        }
    }

    public bool IsStaticImageSelected => Mode == BuiltInScannerMode.StaticImage;

    public bool IsAnimationSelected => Mode == BuiltInScannerMode.Animation;

    public string ModeText => Mode == BuiltInScannerMode.StaticImage ? "Static Image / IMAG" : "Animation / ANIM";

    public int CurrentId
    {
        get => currentId;
        set
        {
            var clamped = Math.Clamp(value, 0, MaxId);
            if (SetField(ref currentId, clamped))
            {
                OnPropertyChanged(nameof(CurrentIdValue));
                OnPropertyChanged(nameof(CurrentHexId));
                OnPropertyChanged(nameof(SendButtonText));
                RaiseCommandStates();
            }
        }
    }

    public double CurrentIdValue
    {
        get => CurrentId;
        set => CurrentId = (int)Math.Round(value);
    }

    public int MaxId => Mode == BuiltInScannerMode.StaticImage ? 0x69 : 0x45;

    public string CurrentHexId => $"0x{CurrentId:X2}";

    public string RangeNote => Mode == BuiltInScannerMode.StaticImage
        ? "IMAG useful range is expected up to about 0x69. Needs real-mask test."
        : "ANIM useful range is expected up to about 0x45. Needs real-mask test.";

    public string ValidationLabel => "Needs real-mask test";

    public string SuggestedSequence => "Test IMAG 0, 1, 2, 3, 4, 5; then ANIM 0, 1, 2, 3, 4, 5. Record useful IDs manually.";

    public string TransportReadinessText => transport.TransportState == MaskCommandTransportState.Ready
        ? $"{transport.TransportDisplayName} command transport ready."
        : $"Command transport unavailable: {transport.TransportStatusText}";

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string LastCommandText
    {
        get => lastCommandText;
        private set => SetField(ref lastCommandText, value);
    }

    public string SendButtonText => $"Send {ModeText} {CurrentId} ({CurrentHexId})";

    private Task SelectStaticImageAsync(CancellationToken cancellationToken)
    {
        Mode = BuiltInScannerMode.StaticImage;
        StatusText = "Static image scanner selected.";
        return Task.CompletedTask;
    }

    private Task SelectAnimationAsync(CancellationToken cancellationToken)
    {
        Mode = BuiltInScannerMode.Animation;
        StatusText = "Animation scanner selected.";
        return Task.CompletedTask;
    }

    private Task PreviousAsync(CancellationToken cancellationToken)
    {
        CurrentId--;
        return SendAsync(cancellationToken);
    }

    private Task NextAsync(CancellationToken cancellationToken)
    {
        CurrentId++;
        return SendAsync(cancellationToken);
    }

    private Task BlackoutAsync(CancellationToken cancellationToken) =>
        SendCommandAsync(MaskCommandBuilder.Brightness(1), cancellationToken, "BLACKOUT");

    private Task SendAsync(CancellationToken cancellationToken)
    {
        var command = Mode == BuiltInScannerMode.StaticImage
            ? MaskCommandBuilder.Image(CurrentId, $"Image {CurrentId}")
            : MaskCommandBuilder.Animation(CurrentId, $"Animation {CurrentId}");
        return SendCommandAsync(command, cancellationToken, command.DisplayName);
    }

    private async Task SendCommandAsync(MaskCommand command, CancellationToken cancellationToken, string label)
    {
        if (!CanSend())
        {
            StatusText = transport.TransportStatusText;
            return;
        }

        try
        {
            IsSending = true;
            LastCommandText = $"{command.Kind}: {label} ({CurrentHexId})";
            StatusText = $"Sending {label}. Needs real-mask test.";
            var result = await transport.SendAsync(command, cancellationToken).ConfigureAwait(false);
            StatusText = result.Succeeded
                ? $"{result.Message} Needs real-mask test."
                : result.Message;
        }
        finally
        {
            IsSending = false;
        }
    }

    private bool CanSend() => !IsSending && transport.TransportState == MaskCommandTransportState.Ready;

    private bool CanStepPrevious() => CanSend() && CurrentId > 0;

    private bool CanStepNext() => CanSend() && CurrentId < MaxId;

    private void OnTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TransportReadinessText));
        RaiseCommandStates();
    }

    private void RaiseCommandStates()
    {
        PreviousCommand.RaiseCanExecuteChanged();
        NextCommand.RaiseCanExecuteChanged();
        SendCommand.RaiseCanExecuteChanged();
        BlackoutCommand.RaiseCanExecuteChanged();
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
