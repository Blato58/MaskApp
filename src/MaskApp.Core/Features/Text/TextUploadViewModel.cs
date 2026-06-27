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
        statusText = transport.StatusText;
        SendCommand = new AsyncRelayCommand(SendAsync, () => !string.IsNullOrWhiteSpace(Text) && transport.IsReady);
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

        if (!transport.IsReady)
        {
            StatusText = transport.StatusText;
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

        var result = await transport.UploadAsync(package, cancellationToken).ConfigureAwait(false);
        StatusText = result.Message;
        FrameCount = result.FramesSent;
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

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
