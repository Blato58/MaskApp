using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Text;

public sealed class TextUploadViewModel : INotifyPropertyChanged
{
    private const int PreviewColumns = 48;
    private const int PreviewRows = 16;
    private const string PreviewOffColor = "#111827";

    private readonly ITextUploadTransport transport;
    private string text = "HELLO";
    private TextColorOption selectedColor;
    private TextAnimationModeOption selectedAnimationMode;
    private int speed = 50;
    private string statusText;
    private string lastPayloadHex = "None";
    private string lastCommandText = "None";
    private int columnCount;
    private int frameCount;
    private bool isSending;
    private bool useCompatibilityWriteOnly;
    private bool supportsAcknowledgements;
    private TextUploadTransportState transportState;

    public TextUploadViewModel(ITextUploadTransport transport)
    {
        this.transport = transport;
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
            new TextAnimationModeOption("Static", 0),
            new TextAnimationModeOption("Scroll left", 1),
            new TextAnimationModeOption("Scroll right", 2),
            new TextAnimationModeOption("Breathe", 3)
        ];

        selectedColor = TextColorOptions[0];
        selectedAnimationMode = AnimationModes[0];
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

    public IReadOnlyList<TextAnimationModeOption> AnimationModes { get; }

    public ObservableCollection<TextPreviewCell> PreviewCells { get; } = [];

    public AsyncRelayCommand SendCommand { get; }

    public string Text
    {
        get => text;
        set
        {
            if (SetField(ref text, value))
            {
                RefreshPreview();
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public TextColorOption SelectedColor
    {
        get => selectedColor;
        set
        {
            if (value is not null && SetField(ref selectedColor, value))
            {
                RefreshPreview();
            }
        }
    }

    public TextAnimationModeOption SelectedAnimationMode
    {
        get => selectedAnimationMode;
        set
        {
            if (value is not null)
            {
                SetField(ref selectedAnimationMode, value);
            }
        }
    }

    public int Speed
    {
        get => speed;
        set => SetField(ref speed, Math.Clamp(value, 1, 100));
    }

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

        var package = TextUploadProtocol.CreatePackage(
            Text,
            SelectedColor.ToLedColor(),
            SelectedAnimationMode.Mode,
            Speed);

        LastCommandText = $"{package.StartCommand.DisplayName}; {package.ModeCommand.DisplayName}; {package.SpeedCommand.DisplayName}";
        LastPayloadHex = Convert.ToHexString(package.Payload);
        ColumnCount = package.ColumnCount;
        FrameCount = package.Frames.Count;

        var options = UseCompatibilityWriteOnly
            ? TextUploadOptions.WriteOnlyCompatibility
            : TextUploadOptions.RequireAcknowledgements;

        try
        {
            IsSending = true;
            StatusText = UseCompatibilityWriteOnly
                ? $"Sending {package.Frames.Count} frame(s) without ACK confirmation..."
                : $"Sending {package.Frames.Count} frame(s) with ACK confirmation...";

            var result = await transport.UploadAsync(package, options, cancellationToken).ConfigureAwait(false);
            StatusText = result.Message;
            FrameCount = result.FramesSent;
        }
        finally
        {
            IsSending = false;
        }
    }

    private void RefreshPreview()
    {
        var ledData = TextGlyphRasterizer.Render(Text);
        ColumnCount = ledData.Length / 2;
        FrameCount = TextUploadProtocol.SplitFrames(
            TextUploadProtocol.BuildPayload(ledData, Enumerable.Repeat(SelectedColor.ToLedColor(), ColumnCount)),
            TextUploadProtocol.DefaultFramePayloadLength).Count;

        PreviewCells.Clear();
        for (var row = 0; row < PreviewRows; row++)
        {
            for (var column = 0; column < PreviewColumns; column++)
            {
                var isLit = IsLit(ledData, column, row);
                PreviewCells.Add(new TextPreviewCell(isLit, isLit ? SelectedColor.Hex : PreviewOffColor));
            }
        }
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
