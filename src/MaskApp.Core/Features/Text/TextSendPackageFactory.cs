using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Text;

public static class TextSendPackageFactory
{
    public static TextSendPlan Create(
        string text,
        TextSendProfile profile,
        bool acknowledgementsAvailable = true)
    {
        var warnings = new List<string>();
        var (displayText, ledData, layout) = BuildLayout(text, profile, warnings);
        var colors = profile.LayoutMode == TextLayoutMode.FixedWidthCentered
            ? QuickCaptionLayout.CreateColumnColors(ledData, profile.TextColor)
            : Enumerable.Repeat(profile.TextColor, ledData.Length / 2).ToArray();
        var styleCommands = BuildStyleCommands();
        var package = TextUploadProtocol.CreatePackageFromLedData(
            displayText,
            ledData,
            colors,
            profile.ProtocolMode,
            profile.Speed,
            profile.UseLargeMtu,
            styleCommands);
        var options = profile.CreateOptions(acknowledgementsAvailable);
        var summary = BuildSummary(profile, package, options, layout, warnings);

        return new TextSendPlan(profile, package, options, layout, warnings, summary);
    }

    private static (string DisplayText, byte[] LedData, TextSendLayoutMetadata Layout) BuildLayout(
        string text,
        TextSendProfile profile,
        ICollection<string> warnings)
    {
        if (profile.LayoutMode == TextLayoutMode.FixedWidthCentered)
        {
            var columnCount = profile.FixedWidthColumns ?? QuickCaptionLayout.VisibleColumns;
            var layout = QuickCaptionLayout.Create(text, columnCount);
            if (!layout.Succeeded)
            {
                throw new ArgumentException(layout.Warning ?? "Text could not be fitted.", nameof(text));
            }

            if (!string.IsNullOrWhiteSpace(layout.Warning))
            {
                warnings.Add(layout.Warning);
            }

            var rawColumnCount = TextGlyphRasterizer.Render(layout.DisplayText).Length / 2;
            var twoLine = rawColumnCount > columnCount && layout.DisplayText.Contains(' ');
            return (
                layout.DisplayText,
                layout.LedData,
                new TextSendLayoutMetadata(
                    layout.ColumnCount,
                    FixedWidth: true,
                    Centered: true,
                    VariableWidth: false,
                    TwoLine: twoLine,
                    Shortened: layout.WasShortened,
                    Description: $"{layout.ColumnCount} centered"));
        }

        var sanitized = NormalizeText(text);
        if (sanitized.Length == 0)
        {
            throw new ArgumentException("Text is empty.", nameof(text));
        }

        var ledData = TextGlyphRasterizer.Render(sanitized);
        var columns = ledData.Length / 2;
        return (
            sanitized,
            ledData,
            new TextSendLayoutMetadata(
                columns,
                FixedWidth: false,
                Centered: false,
                VariableWidth: true,
                TwoLine: false,
                Shortened: false,
                Description: $"{columns} columns"));
    }

    private static IReadOnlyList<MaskCommand> BuildStyleCommands()
    {
        return
        [
            MaskCommandBuilder.TextBackgroundColor(
                enabled: true,
                red: 0x00,
                green: 0x00,
                blue: 0x00)
        ];
    }

    private static string BuildSummary(
        TextSendProfile profile,
        TextUploadPackage package,
        TextUploadOptions options,
        TextSendLayoutMetadata layout,
        IReadOnlyCollection<string> warnings)
    {
        var transport = options.AckRequired ? "ACK" : "write-only";
        var repeat = options.RepeatModeAndSpeed ? " · repeat mode" : string.Empty;
        var warningText = warnings.Count == 0 ? string.Empty : $" · {string.Join("; ", warnings)}";
        return $"{profile.Name} · {layout.Description} · black background · MODE {profile.ProtocolMode} · SPEED {profile.Speed} · {transport}{repeat}{warningText}";
    }

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var chars = new List<char>(text.Length);
        var previousWasSpace = false;
        foreach (var rawCharacter in text.Trim())
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
}
