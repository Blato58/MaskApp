namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetPreview
{
    public BuiltInAssetPreview(
        int width,
        int height,
        BuiltInAssetPreviewFrame[] frames,
        bool isDataBacked,
        string sourceLabel)
    {
        Width = width;
        Height = height;
        Frames = frames.Length == 0
            ? [new BuiltInAssetPreviewFrame(width, height, Enumerable.Repeat(new string('.', width), height).ToArray())]
            : frames;
        IsDataBacked = isDataBacked;
        SourceLabel = sourceLabel;
    }

    public int Width { get; }

    public int Height { get; }

    public BuiltInAssetPreviewFrame[] Frames { get; }

    public bool IsDataBacked { get; }

    public string SourceLabel { get; }

    public BuiltInAssetPreviewFrame FirstFrame => Frames[0];

    public string PreviewText => FirstFrame.Text;

    public string BadgeText => IsDataBacked ? "Android data" : "Generated preview";

    public string FrameCountText => Frames.Length == 1 ? "1 frame" : $"{Frames.Length} frames";
}
