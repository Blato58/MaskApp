using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Text;

public sealed class QuickCaptionLayoutTests
{
    [Theory]
    [InlineData("LOL")]
    [InlineData("DROP")]
    public void Create_ShortCaption_UsesFixedFortyFourColumns(string caption)
    {
        var layout = QuickCaptionLayout.Create(caption);

        Assert.True(layout.Succeeded);
        Assert.Equal(QuickCaptionLayout.VisibleColumns, layout.ColumnCount);
        Assert.Equal(QuickCaptionLayout.VisibleColumns * 2, layout.LedData.Length);
        Assert.False(HasLitPixelInRows(layout.LedData, startRow: 0, endRow: 3));
        Assert.True(HasLitPixelInRows(layout.LedData, startRow: 4, endRow: 10));
        Assert.False(HasLitPixelInRows(layout.LedData, startRow: 11, endRow: 15));
    }

    [Fact]
    public void Create_ShortCaption_AddsLeftAndRightPadding()
    {
        var rawLedData = TextGlyphRasterizer.Render("LOL");
        var layout = QuickCaptionLayout.Create("LOL");

        Assert.True(layout.Succeeded);
        Assert.NotEqual(0, rawLedData[0] | rawLedData[1]);
        Assert.Equal(0, layout.LedData[0] | layout.LedData[1]);
        Assert.Equal(0, layout.LedData[^2] | layout.LedData[^1]);
    }

    [Fact]
    public void Create_VibeCheck_RendersTwoCenteredLines()
    {
        var layout = QuickCaptionLayout.Create("VIBE CHECK");

        Assert.True(layout.Succeeded);
        Assert.False(layout.WasShortened);
        Assert.Equal("VIBE CHECK", layout.DisplayText);
        Assert.Equal(QuickCaptionLayout.VisibleColumns, layout.ColumnCount);
        Assert.True(HasLitPixelInRows(layout.LedData, startRow: 0, endRow: 6));
        Assert.True(HasLitPixelInRows(layout.LedData, startRow: 9, endRow: 15));
    }

    [Fact]
    public void CreateColumnColors_BlankPaddingColumnsAreBlack()
    {
        var layout = QuickCaptionLayout.Create("VIBE CHECK");
        var litColor = new TextLedColor(0xFF, 0xFF, 0xFF);

        var colors = QuickCaptionLayout.CreateColumnColors(layout.LedData, litColor);

        Assert.Equal(QuickCaptionLayout.VisibleColumns, colors.Count);
        Assert.Equal(new TextLedColor(0, 0, 0), colors[0]);
        Assert.Equal(new TextLedColor(0, 0, 0), colors[^1]);
        Assert.Contains(litColor, colors);
    }

    [Fact]
    public void Create_LongCaption_IsShortenedAndStillFortyFourColumns()
    {
        var layout = QuickCaptionLayout.Create("THIS CAPTION IS WAY TOO LONG FOR FAST RAVE TEXT");

        Assert.True(layout.Succeeded);
        Assert.True(layout.WasShortened);
        Assert.Equal(QuickCaptionLayout.VisibleColumns, layout.ColumnCount);
        Assert.True(TextGlyphRasterizer.Render(layout.DisplayText).Length / 2 <= QuickCaptionLayout.VisibleColumns);
    }

    [Fact]
    public void Create_WhitespaceCaption_FailsSafely()
    {
        var layout = QuickCaptionLayout.Create("   ");

        Assert.False(layout.Succeeded);
        Assert.Equal("Caption is empty.", layout.Warning);
        Assert.Empty(layout.LedData);
    }

    private static bool HasLitPixelInRows(byte[] ledData, int startRow, int endRow)
    {
        for (var column = 0; column < ledData.Length / 2; column++)
        {
            var offset = column * 2;
            var columnBits = (ledData[offset] << 8) | ledData[offset + 1];
            for (var row = startRow; row <= endRow; row++)
            {
                if ((columnBits & (1 << (15 - row))) != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
