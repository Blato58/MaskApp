using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.App.Resources.Strings;
using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Profiles;
using Microsoft.Maui.ApplicationModel;

namespace MaskApp.App.Features.Audio;

public sealed record AudioFramingOption(string Label, AudioVisualizationFraming Value);

public sealed record AudioPackingModeOption(string Label, AudioVisualizationPackingMode Value);

public sealed record AudioVisualizerModeOption(string Label, AudioVisualizerMode Value);

public sealed class AudioLabsViewModel : INotifyPropertyChanged
{
    private readonly AudioVisualizationDiagnostic diagnostic;
    private readonly AudioVisualizerEngine engine;
    private readonly IAudioVisualizationTransport transport;
    private readonly IMaskEmergencyControl emergencyControl;
    private readonly MaskProfileSession profileSession;
    private AudioFramingOption selectedFramingOption;
    private AudioPackingModeOption selectedPackingModeOption;
    private AudioVisualizerModeOption selectedModeOption;
    private AudioVisualizationEvidence evidence = new();
    private double sensitivity = 1;
    private double threshold = 0.08;
    private double smoothing = 0.55;
    private bool isBusy;
    private bool isActive;
    private string statusText = AppText.Get("Ui441");

    public AudioLabsViewModel(
        AudioVisualizationDiagnostic diagnostic,
        AudioVisualizerEngine engine,
        IAudioVisualizationTransport transport,
        IMaskEmergencyControl emergencyControl,
        MaskProfileSession profileSession)
    {
        this.diagnostic = diagnostic;
        this.engine = engine;
        this.transport = transport;
        this.emergencyControl = emergencyControl;
        this.profileSession = profileSession;
        selectedFramingOption = FramingOptions[0];
        selectedPackingModeOption = PackingModeOptions[0];
        selectedModeOption = ModeOptions[0];
        RunDiagnosticCommand = new AsyncRelayCommand(RunDiagnosticAsync, CanRunDiagnostic, HandleCommandError);
        StartCommand = new AsyncRelayCommand(StartAsync, CanStart, HandleCommandError);
        StopCommand = new AsyncRelayCommand(StopAsync, () => true, HandleCommandError);
        CalibrateCommand = new AsyncRelayCommand(CalibrateAsync, () => !IsBusy && !IsRunning, HandleCommandError);
        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync, () => true, HandleCommandError);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<AudioFramingOption> FramingOptions { get; } =
    [
        new(AppText.Get("Ui442"), AudioVisualizationFraming.LegacyAndroidLength),
        new(AppText.Get("Ui443"), AudioVisualizationFraming.FirmwareLength)
    ];

    public IReadOnlyList<AudioPackingModeOption> PackingModeOptions { get; } =
    [
        new(AppText.Get("Ui444"), AudioVisualizationPackingMode.PaletteA),
        new(AppText.Get("Ui445"), AudioVisualizationPackingMode.PaletteB),
        new(AppText.Get("Ui446"), AudioVisualizationPackingMode.DuplicatedPairs),
        new(AppText.Get("Ui447"), AudioVisualizationPackingMode.SpacedPairs)
    ];

    public IReadOnlyList<AudioVisualizerModeOption> ModeOptions { get; } =
    [
        new(AppText.Get("Ui448"), AudioVisualizerMode.Spectrum),
        new(AppText.Get("Ui449"), AudioVisualizerMode.BassFace),
        new(AppText.Get("Ui450"), AudioVisualizerMode.VoiceMouth),
        new(AppText.Get("Ui451"), AudioVisualizerMode.DropDetector)
    ];

    public AsyncRelayCommand RunDiagnosticCommand { get; }

    public AsyncRelayCommand StartCommand { get; }

    public AsyncRelayCommand StopCommand { get; }

    public AsyncRelayCommand CalibrateCommand { get; }

    public AsyncRelayCommand BlackoutCommand { get; }

    public AudioFramingOption SelectedFramingOption
    {
        get => selectedFramingOption;
        set
        {
            if (value is not null)
            {
                SetField(ref selectedFramingOption, value);
            }
        }
    }

    public AudioPackingModeOption SelectedPackingModeOption
    {
        get => selectedPackingModeOption;
        set
        {
            if (value is not null)
            {
                SetField(ref selectedPackingModeOption, value);
            }
        }
    }

    public AudioVisualizerModeOption SelectedModeOption
    {
        get => selectedModeOption;
        set
        {
            if (value is not null && SetField(ref selectedModeOption, value))
            {
                ApplySettings();
                OnPropertyChanged(nameof(ModeSafetyText));
            }
        }
    }

    public double Sensitivity
    {
        get => sensitivity;
        set
        {
            if (SetField(ref sensitivity, value))
            {
                ApplySettings();
                OnPropertyChanged(nameof(SensitivityText));
            }
        }
    }

    public string SensitivityText => $"{Sensitivity:0.00}×";

    public double Threshold
    {
        get => threshold;
        set
        {
            if (SetField(ref threshold, value))
            {
                ApplySettings();
                OnPropertyChanged(nameof(ThresholdText));
            }
        }
    }

    public string ThresholdText => Threshold.ToString("0.00");

    public double Smoothing
    {
        get => smoothing;
        set
        {
            if (SetField(ref smoothing, value))
            {
                ApplySettings();
                OnPropertyChanged(nameof(SmoothingText));
            }
        }
    }

    public string SmoothingText => Smoothing.ToString("0.00");

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                RaiseCommandStates();
                OnPropertyChanged(nameof(CanStartLive));
            }
        }
    }

    public bool IsRunning => engine.State == AudioVisualizerEngineState.Running;

    public bool CanStartLive => evidence.EnablesLiveMicrophone && transport.IsReady && !IsBusy && !IsRunning;

    public bool CanConfirmPhysicalResult =>
        evidence.Status == AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation
        && !evidence.IsSimulated;

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string TransportText => transport.State switch
    {
        AudioVisualizationTransportState.Disconnected => AppText.Get("Ui481"),
        AudioVisualizationTransportState.Discovering => AppText.Get("Ui482"),
        AudioVisualizationTransportState.Ready => AppText.Get("Ui483"),
        AudioVisualizationTransportState.Unsupported => AppText.Get("Ui484"),
        AudioVisualizationTransportState.Failed => AppText.Get("Ui485"),
        AudioVisualizationTransportState.Simulated => AppText.Get("Ui486"),
        _ => AppText.Get("Ui481")
    };

    public string EvidenceText => evidence.Status switch
    {
        AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation => AppText.Get("Ui466"),
        AudioVisualizationEvidenceStatus.Passed => AppText.Get("Ui467"),
        AudioVisualizationEvidenceStatus.Failed => AppText.Get("Ui468"),
        AudioVisualizationEvidenceStatus.Unsupported => AppText.Get("Ui469"),
        _ => AppText.Get("Ui465")
    };

    public string EvidenceStatusText => evidence.Status switch
    {
        AudioVisualizationEvidenceStatus.Passed => AppText.Get("Ui452"),
        AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation => AppText.Get("Ui453"),
        AudioVisualizationEvidenceStatus.Failed => AppText.Get("Ui454"),
        AudioVisualizationEvidenceStatus.Unsupported => AppText.Get("Ui455"),
        _ => AppText.Get("Ui456")
    };

    public string RuntimeStatsText =>
        string.Format(
            AppText.Get("Ui457"),
            engine.FramesSent,
            engine.FramesSuppressed,
            engine.NoiseFloor);

    public string ModeSafetyText => SelectedModeOption.Value == AudioVisualizerMode.DropDetector
        ? AppText.Get("Ui458")
        : AppText.Get("Ui459");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var profile = await profileSession.GetActiveProfileAsync(cancellationToken);
        evidence = profile?.AudioVisualizationEvidence.Normalize() ?? new AudioVisualizationEvidence();
        if (evidence.Status != AudioVisualizationEvidenceStatus.Unknown)
        {
            selectedFramingOption = FramingOptions.First(option => option.Value == evidence.Framing);
            selectedPackingModeOption = PackingModeOptions.First(option => option.Value == evidence.PackingMode);
            OnPropertyChanged(nameof(SelectedFramingOption));
            OnPropertyChanged(nameof(SelectedPackingModeOption));
        }

        StatusText = IsRunning
            ? AppText.Get("Ui473")
            : profile is null
                ? AppText.Get("Ui460")
                : EvidenceText;
        RaiseAllStateProperties();
    }

    public void Activate()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;
        engine.StateChanged += HandleEngineStateChanged;
        transport.StateChanged += HandleTransportStateChanged;
        profileSession.ActiveProfileChanged += HandleActiveProfileChanged;
    }

    public void Deactivate()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        engine.StateChanged -= HandleEngineStateChanged;
        transport.StateChanged -= HandleTransportStateChanged;
        profileSession.ActiveProfileChanged -= HandleActiveProfileChanged;
    }

    public async Task ConfirmPhysicalResultAsync(
        bool passed,
        CancellationToken cancellationToken = default)
    {
        if (!CanConfirmPhysicalResult)
        {
            StatusText = AppText.Get("Ui461");
            return;
        }

        IsBusy = true;
        try
        {
            var confirmed = AudioVisualizationDiagnostic.ConfirmPhysicalResult(evidence, passed);
            var profile = await profileSession.RecordAudioVisualizationEvidenceAsync(confirmed, cancellationToken);
            evidence = profile?.AudioVisualizationEvidence.Normalize() ?? confirmed;
            StatusText = EvidenceText;
            RaiseAllStateProperties();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task StopMicrophoneForNavigationAsync(CancellationToken cancellationToken = default)
    {
        diagnostic.CancelActiveTest();
        return engine.StopAsync(cancellationToken);
    }

    private async Task RunDiagnosticAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            await engine.StopAsync(cancellationToken);
            var result = await diagnostic.RunAsync(new AudioVisualizationDiagnosticOptions
            {
                Framing = SelectedFramingOption.Value,
                PackingMode = SelectedPackingModeOption.Value
            }, cancellationToken);
            var profile = await profileSession.RecordAudioVisualizationEvidenceAsync(
                result.Evidence,
                cancellationToken);
            evidence = profile?.AudioVisualizationEvidence.Normalize() ?? result.Evidence;
            StatusText = profile is null
                ? AppText.Get("Ui462")
                : EvidenceText;
            RaiseAllStateProperties();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            ApplySettings();
            var result = await engine.StartAsync(cancellationToken);
            StatusText = result.Succeeded ? AppText.Get("Ui476") : AppText.Get("Ui480");
        }
        finally
        {
            IsBusy = false;
            RaiseAllStateProperties();
        }
    }

    private async Task StopAsync(CancellationToken cancellationToken)
    {
        diagnostic.CancelActiveTest();
        IsBusy = true;
        try
        {
            await engine.StopAsync(cancellationToken);
            var result = await emergencyControl.StopAsync(cancellationToken);
            StatusText = result.Succeeded ? AppText.Get("Ui477") : AppText.Get("Ui480");
        }
        finally
        {
            IsBusy = false;
            RaiseAllStateProperties();
        }
    }

    private async Task CalibrateAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            var result = await engine.CalibrateAsync(cancellationToken);
            StatusText = result.Succeeded ? AppText.Get("Ui478") : AppText.Get("Ui480");
        }
        finally
        {
            IsBusy = false;
            RaiseAllStateProperties();
        }
    }

    private async Task BlackoutAsync(CancellationToken cancellationToken)
    {
        diagnostic.CancelActiveTest();
        IsBusy = true;
        try
        {
            await engine.StopAsync(cancellationToken);
            var result = await emergencyControl.BlackoutAsync(cancellationToken);
            StatusText = result.Succeeded ? AppText.Get("Ui479") : AppText.Get("Ui480");
        }
        finally
        {
            IsBusy = false;
            RaiseAllStateProperties();
        }
    }

    private bool CanRunDiagnostic() =>
        !IsBusy
        && !IsRunning
        && transport.State is AudioVisualizationTransportState.Ready
            or AudioVisualizationTransportState.Unsupported;

    private bool CanStart() => CanStartLive;

    private void ApplySettings() => engine.UpdateSettings(new AudioVisualizerSettings
    {
        Mode = SelectedModeOption.Value,
        Sensitivity = Sensitivity,
        Threshold = Threshold,
        Smoothing = Smoothing
    });

    private void HandleEngineStateChanged(object? sender, AudioVisualizerEngineStateChangedEventArgs args) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusText = args.State switch
            {
                AudioVisualizerEngineState.Stopped => AppText.Get("Ui470"),
                AudioVisualizerEngineState.Calibrating => AppText.Get("Ui471"),
                AudioVisualizerEngineState.Starting => AppText.Get("Ui472"),
                AudioVisualizerEngineState.Running => AppText.Get("Ui473"),
                AudioVisualizerEngineState.Stopping => AppText.Get("Ui474"),
                AudioVisualizerEngineState.Blocked => AppText.Get("Ui475"),
                AudioVisualizerEngineState.Failed => AppText.Get("Ui463"),
                _ => AppText.Get("Ui463")
            };
            RaiseAllStateProperties();
        });

    private void HandleTransportStateChanged(
        object? sender,
        AudioVisualizationTransportStateChangedEventArgs args) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(TransportText));
            OnPropertyChanged(nameof(CanStartLive));
            RaiseCommandStates();
        });

    private void HandleActiveProfileChanged(object? sender, MaskProfileChangedEventArgs args) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            evidence = args.Profile.AudioVisualizationEvidence.Normalize();
            RaiseAllStateProperties();
        });

    private void HandleCommandError(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine($"Audio Labs command failed: {exception}");
        MainThread.BeginInvokeOnMainThread(() => StatusText = AppText.Get("Ui463"));
    }

    private void RaiseAllStateProperties()
    {
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(CanStartLive));
        OnPropertyChanged(nameof(CanConfirmPhysicalResult));
        OnPropertyChanged(nameof(EvidenceText));
        OnPropertyChanged(nameof(EvidenceStatusText));
        OnPropertyChanged(nameof(RuntimeStatsText));
        OnPropertyChanged(nameof(TransportText));
        RaiseCommandStates();
    }

    private void RaiseCommandStates()
    {
        RunDiagnosticCommand.RaiseCanExecuteChanged();
        StartCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
        CalibrateCommand.RaiseCanExecuteChanged();
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
