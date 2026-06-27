namespace MaskApp.Core.Features.Text;

public sealed class TextUploadFrame
{
    private readonly byte[] data;

    public TextUploadFrame(int index, byte[] data)
    {
        Index = index;
        this.data = [.. data];
    }

    public int Index { get; }

    public ReadOnlyMemory<byte> Data => data;
}
