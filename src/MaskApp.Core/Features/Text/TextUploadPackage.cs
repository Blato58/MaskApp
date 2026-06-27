using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Text;

public sealed class TextUploadPackage
{
    internal TextUploadPackage(
        string text,
        int columnCount,
        byte[] ledData,
        byte[] payload,
        IReadOnlyList<TextUploadFrame> frames,
        MaskCommand startCommand,
        MaskCommand finishCommand,
        MaskCommand modeCommand,
        MaskCommand speedCommand)
    {
        Text = text;
        ColumnCount = columnCount;
        LedData = [.. ledData];
        Payload = [.. payload];
        Frames = frames;
        StartCommand = startCommand;
        FinishCommand = finishCommand;
        ModeCommand = modeCommand;
        SpeedCommand = speedCommand;
    }

    public string Text { get; }

    public int ColumnCount { get; }

    public byte[] LedData { get; }

    public byte[] Payload { get; }

    public IReadOnlyList<TextUploadFrame> Frames { get; }

    public MaskCommand StartCommand { get; }

    public MaskCommand FinishCommand { get; }

    public MaskCommand ModeCommand { get; }

    public MaskCommand SpeedCommand { get; }
}
