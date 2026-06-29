using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Text;

public sealed class TextUploadViewModel : INotifyPropertyChanged
{
    public const int MaxTextLength = 64;

    private const int PreviewColumns = 48;
    private const int PreviewRows = 16;
    private const string PreviewOffColor = "#111827";
    private const int MaxDiagnosticHexLength = 1024;
    private static readonly TimeSpan PreviewDebounceDelay = TimeSpan.FromMilliseconds(180);

    private readonly ITextUploadTransport transport;
    private readonly SynchronizationContext? synchronizationContext;
    private string text = "HELLO";
    private TextColorOption selectedColor;
    private TextLayoutModeOption selectedLayoutMode;
    private TextAnimationModeOption selectedAnimationMode;
    private int speed = 50;
    private string statusText;
    private string lastPayloadHex = "None";
    private string lastCommandText = "None";
    private IReadOnlyList<TextPreviewCell> previewCells = [];
    private int columnCount;
    private int frameCount;
    private bool isSending;
    private bool useCompatibilityWriteOnly;
    private bool supportsAcknowledgements;
    private TextUploadTransportState transportState;
    private CancellationTokenSource? previewRefreshCancellation;

    public TextUploadViewModel(ITextUploadTransport transport)
    {
        this.transport = transport;
        synchronizationContext = SynchronizationContext.Current;
        TextColorOptions =
        [
            new TextColorOption("Cyan", 0x52, 0xE3, 0xFF, "#52E3FF"),
            new TextColorOption("White", 0xFF, 0xFF, 0xFF, "#FFFFFF"),
            new TextColorOption("Pink", 0xF4, 0x72, 0xB6, "#F472B6"),
            new TextColorOption("Amber", 0xFA, 0xCC, 0x15, "#FACC15"),
            new TextColorOption("Green", 0x22, 0xC5, 0x5E, "#22C55E")
        ];
        AnimationModes =
        [
            new TextAnimationModeOption("Off", 1),
            new TextAnimationModeOption("Blink", 2),
            new TextAnimationModeOption("Scroll right-to-left", 3),
            new TextAnimationModeOption("Scroll left-to-right", 4)
        ];
        LayoutModes =
        [
            new TextLayoutModeOption("Scroll / variable width", TextLayoutMode.VariableWidth),
            new TextLayoutModeOption("Centered 44-column", TextLayoutMode.FixedWidthCentered)
        ];

        selectedColor = TextColorOptions[0];
        selectedLayoutMode = LayoutModes[1];
        selectedAnimationMode = AnimationModes[1];
        supportsAcknowledgements = transport.SupportsAcknowledgements;
        transportState = transport.State;
        useCompatibilityWriteOnly = ShouldDefaultToCompatibilityMode(transport.State, transport.SupportsAcknowledgements);
        statusText = transport.StatusText;
        SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        transport.StateChanged += OnTransportStateChanged;
        RefreshPreview();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<TextColorOption> TextColorOptions { get; }

    public IReadOnlyList<TextLayoutModeOption> LayoutModes { get; }

    public IReadOnlyList<TextAnimationModeOption> AnimationModes { get; }

    public IReadOnlyList<TextPreviewCell> PreviewCells
    {
        get => previewCells;
        private set => SetField(ref previewCells, value);
    }

    public AsyncRelayCommand SendCommand { get; }

    public string Text
    {
        get => text;
        set
        {
            var incomingValue = value ?? string.Empty;
            var clampedValue = incomingValue.Length > MaxTextLength
                ? incomingValue[..MaxTextLength]
                : incomingValue;

            if (SetField(ref text, clampedValue))
            {
                SchedulePreviewRefresh();
                OnPropertyChanged(nameof(CharacterCountText));
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CharacterCountText => $"{Text.Length}/{MaxTextLength}";

    public TextColorOption SelectedColor
    {
        get => selectedColor;
        set
        {
            if (value is not null && SetField(ref selectedColor, value))
            {
                SchedulePreviewRefresh();
                OnPropertyChanged(nameof(ProfileSummary));
            }
        }
    }

    public TextLayoutModeOption SelectedLayoutMode
    {
        get => selectedLayoutMode;
        set
        {
            if (value is not null && SetField(ref selectedLayoutMode, value))
            {
                SchedulePreviewRefresh();
                OnPropertyChanged(nameof(ProfileSummary));
            }
        }
    }

    public TextAnimationModeOption SelectedAnimationMode
    {
        get => selectedAnimationMode;
        set
        {
            if (value is not null && SetField(ref selectedAnimationMode, value))
            {
                OnPropertyChanged(nameof(ProfileSummary));
            }
        }
    }

    public int Speed
    {
        get => speed;
        set
        {
            if (SetField(ref speed, Math.Clamp(value, 1, 100)))
            {
                OnPropertyChanged(nameof(ProfileSummary));
            }
        }
    }

    public string ProfileSummary => BuildComposerProfile().Name == TextSendProfile.ComposerCentered.Name
        ? $"Centered 44 columns, {SelectedAnimationMode.Name}, Speed {Speed}"
        : $"Variable width, {SelectedAnimationMode.Name}, Speed {Speed}";

    public string ActiveTransportText =>
        transport.IsSimulated
            ? $"{transport.TransportDisplayName} (simulated)"
            : $"{transport.TransportDisplayName} (real)";

    public bool SupportsAcknowledgements
    {
        get => supportsAcknowledgements;
        private set
        {
            if (SetField(ref supportsAcknowledgements, value))
            {
                OnPropertyChanged(nameof(AcknowledgementModeText));
            }
        }
    }

    public TextUploadTransportState TransportState
    {
        get => transportState;
        private set
        {
            if (SetField(ref transportState, value))
            {
                OnPropertyChanged(nameof(CanUseCompatibilityWriteOnly));
                OnPropertyChanged(nameof(AcknowledgementModeText));
            }
        }
    }

    public bool CanUseCompatibilityWriteOnly =>
        TransportState is TextUploadTransportState.Ready
            or TextUploadTransportState.CompatibilityReady
            or TextUploadTransportState.Simulated;

    public bool UseCompatibilityWriteOnly
    {
        get => useCompatibilityWriteOnly;
        set
        {
            if (SetField(ref useCompatibilityWriteOnly, value))
            {
                OnPropertyChanged(nameof(AcknowledgementModeText));
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string AcknowledgementModeText
    {
        get
        {
            if (UseCompatibilityWriteOnly)
            {
                return "Write-only compatibility: sends without ACK confirmation.";
            }

            return SupportsAcknowledgements
                ? "ACK required: each text step waits for mask confirmation."
                : "ACK unavailable: enable write-only compatibility to send.";
        }
    }

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string LastPayloadHex
    {
        get => lastPayloadHex;
        private set => SetField(ref lastPayloadHex, value);
    }

    public string LastCommandText
    {
        get => lastCommandText;
        private set => SetField(ref lastCommandText, value);
    }

    public int ColumnCount
    {
        get => columnCount;
        private set => SetField(ref columnCount, value);
    }

    public int FrameCount
    {
        get => frameCount;
        private set => SetField(ref frameCount, value);
    }

    public void SelectColor(TextColorOption color)
    {
        SelectedColor = color;
    }

    private async Task SendAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            StatusText = "Enter text before sending.";
            return;
        }

        if (!CanSend())
        {
            StatusText = BuildCannotSendStatus();
            return;
        }

        var plan = TextSendPackageFactory.Create(
            Text,
            BuildComposerProfile(),
            transport.SupportsAcknowledgements);
        var package = plan.Package;

        LastCommandText = $"{plan.Summary}; {package.StartCommand.DisplayName}; {package.SpeedCommand.DisplayName}; {package.ModeCommand.DisplayName}";
        LastPayloadHex = TruncateDiagnosticHex(Convert.ToHexString(package.Payload));
        ColumnCount = package.ColumnCount;
        FrameCount = package.Frames.Count;

        try
        {
            IsSending = true;
            StatusText = $"Sending {plan.Summary}...";

            var result = await transport.UploadAsync(package, plan.Options, cancellationToken);
            StatusText = result.Succeeded ? plan.Summary : result.Message;
            FrameCount = result.FramesSent;
        }
        catch (OperationCanceledException)
        {
            StatusText = "Text send cancelled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed: {GetShortErrorMessage(ex)}";
        }
        finally
        {
            IsSending = false;
        }
    }

    private void RefreshPreview()
    {
        try
        {
            var plan = TextSendPackageFactory.Create(
                Text,
                BuildComposerProfile(),
                transport.SupportsAcknowledgements);
            var ledData = plan.Package.LedData;
            ColumnCount = ledData.Length / 2;
            FrameCount = CalculateFrameCount(ColumnCount);

            var cells = new TextPreviewCell[PreviewColumns * PreviewRows];
            var index = 0;
            for (var row = 0; row < PreviewRows; row++)
            {
                for (var column = 0; column < PreviewColumns; column++)
                {
                    var isLit = IsLit(ledData, column, row);
                    cells[index++] = new TextPreviewCell(isLit, isLit ? SelectedColor.Hex : PreviewOffColor);
                }
            }

            PreviewCells = cells;
        }
        catch (Exception ex)
        {
            ColumnCount = 0;
            FrameCount = 0;
            PreviewCells = BuildBlankPreview();
            StatusText = $"Preview unavailable: {GetShortErrorMessage(ex)}";
        }
    }

    private void SchedulePreviewRefresh()
    {
        previewRefreshCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        previewRefreshCancellation = cancellation;
        _ = RefreshPreviewAfterDelayAsync(cancellation);
    }

    private async Task RefreshPreviewAfterDelayAsync(CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(PreviewDebounceDelay, cancellation.Token).ConfigureAwait(false);
            if (cancellation.IsCancellationRequested)
            {
                return;
            }

            RunOnCapturedContext(RefreshPreview);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(previewRefreshCancellation, cancellation))
            {
                previewRefreshCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    private void RunOnCapturedContext(Action action)
    {
        if (synchronizationContext is null)
        {
            action();
            return;
        }

        synchronizationContext.Post(_ => action(), null);
    }

    private static bool IsLit(byte[] ledData, int column, int row)
    {
        var byteOffset = column * 2;
        if (byteOffset + 1 >= ledData.Length)
        {
            return false;
        }

        var columnBits = (ledData[byteOffset] << 8) | ledData[byteOffset + 1];
        return (columnBits & (1 << (15 - row))) != 0;
    }

    private void OnTransportStateChanged(object? sender, TextUploadTransportStateChangedEventArgs e)
    {
        TransportState = e.State;
        SupportsAcknowledgements = e.SupportsAcknowledgements;
        StatusText = e.Message;

        if (ShouldDefaultToCompatibilityMode(e.State, e.SupportsAcknowledgements))
        {
            UseCompatibilityWriteOnly = true;
        }

        OnPropertyChanged(nameof(ActiveTransportText));
        SendCommand.RaiseCanExecuteChanged();
    }

    private bool CanSend()
    {
        if (IsSending || string.IsNullOrWhiteSpace(Text) || !transport.IsReady)
        {
            return false;
        }

        if (UseCompatibilityWriteOnly)
        {
            return CanUseCompatibilityWriteOnly;
        }

        return SupportsAcknowledgements
            && TransportState is TextUploadTransportState.Ready or TextUploadTransportState.Simulated;
    }

    private string BuildCannotSendStatus()
    {
        if (transport.IsReady && !SupportsAcknowledgements && !UseCompatibilityWriteOnly)
        {
            return "ACK notifications are unavailable. Enable write-only compatibility to send.";
        }

        return transport.StatusText;
    }

    private static bool ShouldDefaultToCompatibilityMode(
        TextUploadTransportState state,
        bool supportsAcknowledgements) =>
        !supportsAcknowledgements && state == TextUploadTransportState.CompatibilityReady;

    private TextSendProfile BuildComposerProfile()
    {
        var baseProfile = SelectedLayoutMode.LayoutMode == TextLayoutMode.FixedWidthCentered
            ? TextSendProfile.ComposerCentered
            : TextSendProfile.ComposerScroll;

        return baseProfile with
        {
            DisplayMode = ToTextDisplayMode(SelectedAnimationMode.Mode),
            Reliability = UseCompatibilityWriteOnly
                ? TextSendReliability.WriteOnlyCompatibility
                : TextSendReliability.ReliableAcknowledgement,
            Speed = Speed,
            TextColor = SelectedColor.ToLedColor()
        };
    }

    private static TextDisplayMode ToTextDisplayMode(int mode) =>
        mode switch
        {
            1 => TextDisplayMode.Off,
            2 => TextDisplayMode.Blink,
            4 => TextDisplayMode.ScrollLeftToRight,
            _ => TextDisplayMode.ScrollRightToLeft
        };

    private static int CalculateFrameCount(int columnCount)
    {
        if (columnCount <= 0)
        {
            return 0;
        }

        var payloadLength = columnCount * 5;
        return (int)Math.Ceiling(payloadLength / (double)TextUploadProtocol.DefaultFramePayloadLength);
    }

    private static TextPreviewCell[] BuildBlankPreview()
    {
        var cells = new TextPreviewCell[PreviewColumns * PreviewRows];
        for (var i = 0; i < cells.Length; i++)
        {
            cells[i] = new TextPreviewCell(false, PreviewOffColor);
        }

        return cells;
    }

    private static string TruncateDiagnosticHex(string value) =>
        value.Length <= MaxDiagnosticHexLength
            ? value
            : string.Concat(value.AsSpan(0, MaxDiagnosticHexLength), "...");

    private static string GetShortErrorMessage(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message)
            ? ex.GetType().Name
            : ex.Message;
        return message.Length <= 96 ? message : string.Concat(message.AsSpan(0, 96), "...");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
}
