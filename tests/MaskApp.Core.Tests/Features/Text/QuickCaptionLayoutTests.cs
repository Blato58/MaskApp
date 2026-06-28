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
}
