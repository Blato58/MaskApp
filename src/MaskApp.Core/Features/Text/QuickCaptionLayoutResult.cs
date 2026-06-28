namespace MaskApp.Core.Features.Text;

public sealed record QuickCaptionLayoutResult(
    string SourceText,
    string DisplayText,
    bool Succeeded,
    bool WasShortened,
    string? Warning,
    int ColumnCount,
    byte[] LedData)
{
    public static QuickCaptionLayoutResult Success(
        string sourceText,
        string displayText,
        bool wasShortened,
        string? warning,
        int columnCount,
        byte[] ledData) =>
        new(sourceText, displayText, true, wasShortened, warning, columnCount, [.. ledData]);

    public static QuickCaptionLayoutResult Failed(string sourceText, string warning) =>
        new(sourceText, string.Empty, false, false, warning, 0, []);
}
