using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Text;

public sealed class TextGlyphRasterizerTests
{
    [Fact]
    public void Render_BuildsDeterministicColumnBytesForKnownCharacters()
    {
        var data = TextGlyphRasterizer.Render("A");

        Assert.Equal(Convert.FromHexString("07E009000900090007E00000"), data);
    }

    [Fact]
    public void Render_UsesQuestionMarkForUnsupportedCharacters()
    {
        var unsupported = TextGlyphRasterizer.Render("é");
        var fallback = TextGlyphRasterizer.Render("?");

        Assert.Equal(fallback, unsupported);
    }

    [Fact]
    public void Render_BoldChangesColumnBytesWithoutChangingColumnCount()
    {
        var regular = TextGlyphRasterizer.Render("B");
        var bold = TextGlyphRasterizer.Render("B", bold: true);

        Assert.Equal(regular.Length, bold.Length);
        Assert.NotEqual(regular, bold);
    }
}
