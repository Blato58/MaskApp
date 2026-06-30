namespace MaskApp.Core.Features.Text;

public static class TextGlyphRasterizer
{
    private const int GlyphHeight = 7;
    private const int CompactGlyphHeight = 5;
    private const int TopPadding = 4;

    private static readonly IReadOnlyDictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
    {
        [' '] = ["00000", "00000", "00000", "00000", "00000", "00000", "00000"],
        ['?'] = ["01110", "10001", "00001", "00010", "00100", "00000", "00100"],
        ['!'] = ["00100", "00100", "00100", "00100", "00100", "00000", "00100"],
        ['.'] = ["00000", "00000", "00000", "00000", "00000", "00110", "00110"],
        [','] = ["00000", "00000", "00000", "00000", "00110", "00100", "01000"],
        ['-'] = ["00000", "00000", "00000", "11111", "00000", "00000", "00000"],
        [':'] = ["00000", "00110", "00110", "00000", "00110", "00110", "00000"],
        ['/'] = ["00001", "00010", "00100", "01000", "10000", "00000", "00000"],
        ['+'] = ["00000", "00100", "00100", "11111", "00100", "00100", "00000"],
        ['<'] = ["00010", "00100", "01000", "10000", "01000", "00100", "00010"],
        ['>'] = ["01000", "00100", "00010", "00001", "00010", "00100", "01000"],
        ['0'] = ["01110", "10001", "10011", "10101", "11001", "10001", "01110"],
        ['1'] = ["00100", "01100", "00100", "00100", "00100", "00100", "01110"],
        ['2'] = ["01110", "10001", "00001", "00010", "00100", "01000", "11111"],
        ['3'] = ["11110", "00001", "00001", "01110", "00001", "00001", "11110"],
        ['4'] = ["00010", "00110", "01010", "10010", "11111", "00010", "00010"],
        ['5'] = ["11111", "10000", "10000", "11110", "00001", "00001", "11110"],
        ['6'] = ["00110", "01000", "10000", "11110", "10001", "10001", "01110"],
        ['7'] = ["11111", "00001", "00010", "00100", "01000", "01000", "01000"],
        ['8'] = ["01110", "10001", "10001", "01110", "10001", "10001", "01110"],
        ['9'] = ["01110", "10001", "10001", "01111", "00001", "00010", "11100"],
        ['A'] = ["01110", "10001", "10001", "11111", "10001", "10001", "10001"],
        ['B'] = ["11110", "10001", "10001", "11110", "10001", "10001", "11110"],
        ['C'] = ["01110", "10001", "10000", "10000", "10000", "10001", "01110"],
        ['D'] = ["11110", "10001", "10001", "10001", "10001", "10001", "11110"],
        ['E'] = ["11111", "10000", "10000", "11110", "10000", "10000", "11111"],
        ['F'] = ["11111", "10000", "10000", "11110", "10000", "10000", "10000"],
        ['G'] = ["01110", "10001", "10000", "10111", "10001", "10001", "01110"],
        ['H'] = ["10001", "10001", "10001", "11111", "10001", "10001", "10001"],
        ['I'] = ["01110", "00100", "00100", "00100", "00100", "00100", "01110"],
        ['J'] = ["00111", "00010", "00010", "00010", "10010", "10010", "01100"],
        ['K'] = ["10001", "10010", "10100", "11000", "10100", "10010", "10001"],
        ['L'] = ["10000", "10000", "10000", "10000", "10000", "10000", "11111"],
        ['M'] = ["10001", "11011", "10101", "10101", "10001", "10001", "10001"],
        ['N'] = ["10001", "11001", "10101", "10011", "10001", "10001", "10001"],
        ['O'] = ["01110", "10001", "10001", "10001", "10001", "10001", "01110"],
        ['P'] = ["11110", "10001", "10001", "11110", "10000", "10000", "10000"],
        ['Q'] = ["01110", "10001", "10001", "10001", "10101", "10010", "01101"],
        ['R'] = ["11110", "10001", "10001", "11110", "10100", "10010", "10001"],
        ['S'] = ["01111", "10000", "10000", "01110", "00001", "00001", "11110"],
        ['T'] = ["11111", "00100", "00100", "00100", "00100", "00100", "00100"],
        ['U'] = ["10001", "10001", "10001", "10001", "10001", "10001", "01110"],
        ['V'] = ["10001", "10001", "10001", "10001", "10001", "01010", "00100"],
        ['W'] = ["10001", "10001", "10001", "10101", "10101", "10101", "01010"],
        ['X'] = ["10001", "10001", "01010", "00100", "01010", "10001", "10001"],
        ['Y'] = ["10001", "10001", "01010", "00100", "00100", "00100", "00100"],
        ['Z'] = ["11111", "00001", "00010", "00100", "01000", "10000", "11111"]
    };

    public static byte[] Render(string text, bool bold = false)
    {
        return Render(text, TopPadding, bold);
    }

    public static byte[] Render(string text, int topPadding, bool bold = false)
    {
        return RenderCore(text, topPadding, GlyphHeight, bold);
    }

    public static byte[] RenderCompact(string text, int topPadding, bool bold = false)
    {
        return RenderCore(text, topPadding, CompactGlyphHeight, bold);
    }

    private static byte[] RenderCore(string text, int topPadding, int glyphHeight, bool bold)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        if (topPadding < 0 || topPadding + glyphHeight > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(topPadding), "Glyphs must fit inside the 16-row mask text area.");
        }

        var columns = new List<ushort>();
        foreach (var character in text)
        {
            AppendGlyph(columns, character, topPadding, glyphHeight, bold);
            columns.Add(0);
        }

        var data = new byte[columns.Count * 2];
        for (var i = 0; i < columns.Count; i++)
        {
            data[i * 2] = (byte)(columns[i] >> 8);
            data[(i * 2) + 1] = (byte)(columns[i] & 0xFF);
        }

        return data;
    }

    private static void AppendGlyph(ICollection<ushort> columns, char character, int topPadding, int glyphHeight, bool bold)
    {
        var normalized = char.ToUpperInvariant(character);
        if (!Glyphs.TryGetValue(normalized, out var glyph))
        {
            glyph = Glyphs['?'];
        }

        var glyphColumns = new ushort[glyph[0].Length];
        for (var column = 0; column < glyph[0].Length; column++)
        {
            ushort bits = 0;
            for (var row = 0; row < glyphHeight; row++)
            {
                if (IsLit(glyph, row, column, glyphHeight))
                {
                    bits |= (ushort)(1 << (15 - (topPadding + row)));
                }
            }

            glyphColumns[column] = bits;
        }

        if (bold)
        {
            var originalColumns = glyphColumns.ToArray();
            for (var column = 1; column < glyphColumns.Length; column++)
            {
                glyphColumns[column] |= originalColumns[column - 1];
            }
        }

        foreach (var bits in glyphColumns)
        {
            columns.Add(bits);
        }
    }

    private static bool IsLit(string[] glyph, int outputRow, int column, int glyphHeight)
    {
        if (glyphHeight == GlyphHeight)
        {
            return glyph[outputRow][column] == '1';
        }

        int[] sourceRows = outputRow switch
        {
            0 => [0],
            1 => [1, 2],
            2 => [3],
            3 => [4, 5],
            _ => [6]
        };
        return sourceRows.Any(row => glyph[row][column] == '1');
    }
}
