namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetPreviewFrame
{
    public BuiltInAssetPreviewFrame(int width, int height, string[] rows)
    {
        Width = width;
        Height = height;
        Rows = rows;
    }

    public int Width { get; }

    public int Height { get; }

    public string[] Rows { get; }

    public string Text => string.Join(Environment.NewLine, Rows);
}
