namespace MaskApp.Core.Features.Text;

public static class QuickCaptionLayout
{
    public const int VisibleColumns = 44;
    public const int VisibleRows = 16;
    private const int TwoLineTopRow = 0;
    private const int TwoLineBottomRow = 9;
    private static readonly TextLedColor BlankColumnColor = new(0x00, 0x00, 0x00);

    public static QuickCaptionLayoutResult Create(string? caption, int visibleColumns = VisibleColumns)
    {
        if (visibleColumns <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(visibleColumns), "Visible columns must be positive.");
        }

        var normalized = NormalizeCaption(caption);
        if (normalized.Length == 0)
        {
            return QuickCaptionLayoutResult.Failed(caption ?? string.Empty, "Caption is empty.");
        }

        var displayText = FitCaption(normalized, visibleColumns, out var wasShortened);
        if (displayText.Length == 0)
        {
            return QuickCaptionLayoutResult.Failed(caption ?? string.Empty, "Caption could not be fitted.");
        }

        var ledData = BuildLedData(displayText, visibleColumns);
        var columnCount = ledData.Length / 2;
        if (columnCount > visibleColumns)
        {
            return QuickCaptionLayoutResult.Failed(caption ?? string.Empty, "Caption is too wide.");
        }

        var warning = wasShortened ? "Caption shortened to fit." : null;
        return QuickCaptionLayoutResult.Success(
            caption ?? string.Empty,
            displayText,
            wasShortened,
            warning,
            visibleColumns,
            ledData);
    }

    public static TextUploadPackage CreatePackage(
        string caption,
        TextLedColor color,
        int mode,
        int speed,
        bool useLargeMtu = false)
    {
        var layout = Create(caption);
        if (!layout.Succeeded)
        {
            throw new ArgumentException(layout.Warning ?? "Caption could not be fitted.", nameof(caption));
        }

        return TextUploadProtocol.CreatePackageFromLedData(
            layout.DisplayText,
            layout.LedData,
            CreateColumnColors(layout.LedData, color),
            mode,
            speed,
            useLargeMtu);
    }

    public static IReadOnlyList<TextLedColor> CreateColumnColors(byte[] ledData, TextLedColor litColumnColor)
    {
        if (ledData.Length % 2 != 0)
        {
            throw new ArgumentException("LED text data must contain two bytes per column.", nameof(ledData));
        }

        var colors = new TextLedColor[ledData.Length / 2];
        for (var column = 0; column < colors.Length; column++)
        {
            var offset = column * 2;
            colors[column] = (ledData[offset] | ledData[offset + 1]) == 0
                ? BlankColumnColor
                : litColumnColor;
        }

        return colors;
    }

    private static string NormalizeCaption(string? caption)
    {
        if (string.IsNullOrWhiteSpace(caption))
        {
            return string.Empty;
        }

        var chars = new List<char>(caption.Length);
        var previousWasSpace = false;
        foreach (var rawCharacter in caption.Trim())
        {
            var character = char.ToUpperInvariant(rawCharacter);
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasSpace)
                {
                    chars.Add(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            chars.Add(character is >= ' ' and <= '~' ? character : '?');
            previousWasSpace = false;
        }

        return new string(chars.ToArray());
    }

    private static string FitCaption(string caption, int visibleColumns, out bool wasShortened)
    {
        wasShortened = false;
        if (MeasureColumns(caption) <= visibleColumns)
        {
            return caption;
        }

        if (CanRenderAsTwoLines(caption, visibleColumns))
        {
            return caption;
        }

        wasShortened = true;
        var words = caption.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var fittedWords = new List<string>();
        foreach (var word in words)
        {
            var candidate = fittedWords.Count == 0
                ? word
                : $"{string.Join(' ', fittedWords)} {word}";
            if (MeasureColumns(candidate) > visibleColumns)
            {
                break;
            }

            fittedWords.Add(word);
        }

        if (fittedWords.Count > 0)
        {
            return string.Join(' ', fittedWords);
        }

        var chars = new List<char>();
        foreach (var character in caption)
        {
            var candidateChars = chars.ToArray();
            Array.Resize(ref candidateChars, candidateChars.Length + 1);
            candidateChars[^1] = character;
            var candidate = new string(candidateChars);
            if (MeasureColumns(candidate) > visibleColumns)
            {
                break;
            }

            chars.Add(character);
        }

        return new string(chars.ToArray()).Trim();
    }

    private static int MeasureColumns(string text) => TextGlyphRasterizer.Render(text).Length / 2;

    private static byte[] BuildLedData(string text, int visibleColumns)
    {
        if (MeasureColumns(text) <= visibleColumns)
        {
            return CenterLedData(TextGlyphRasterizer.Render(text), visibleColumns);
        }

        var lines = SplitTwoLineCaption(text, visibleColumns);
        if (lines is null)
        {
            return CenterLedData(TextGlyphRasterizer.Render(text), visibleColumns);
        }

        var topLine = CenterLedData(TextGlyphRasterizer.Render(lines.Value.Top, TwoLineTopRow), visibleColumns);
        var bottomLine = CenterLedData(TextGlyphRasterizer.Render(lines.Value.Bottom, TwoLineBottomRow), visibleColumns);
        for (var i = 0; i < topLine.Length; i++)
        {
            topLine[i] |= bottomLine[i];
        }

        return topLine;
    }

    private static bool CanRenderAsTwoLines(string caption, int visibleColumns) =>
        SplitTwoLineCaption(caption, visibleColumns) is not null;

    private static (string Top, string Bottom)? SplitTwoLineCaption(string caption, int visibleColumns)
    {
        var words = caption.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length != 2)
        {
            return null;
        }

        return MeasureColumns(words[0]) <= visibleColumns && MeasureColumns(words[1]) <= visibleColumns
            ? (words[0], words[1])
            : null;
    }

    private static byte[] CenterLedData(byte[] ledData, int visibleColumns)
    {
        var sourceColumns = ledData.Length / 2;
        var leftPadding = Math.Max(0, (visibleColumns - sourceColumns) / 2);
        var centered = new byte[visibleColumns * 2];
        ledData.CopyTo(centered.AsSpan(leftPadding * 2));
        return centered;
    }
}
