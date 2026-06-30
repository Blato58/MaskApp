using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.TextPresets;

public sealed class CzechTextNormalizerTests
{
    [Theory]
    [InlineData("ČAU", "CAU")]
    [InlineData("TY VOLE", "TY VOLE")]
    [InlineData("PŘÍSAHA", "PRISAHA")]
    [InlineData("STAČILO", "STACILO")]
    [InlineData("KDE JE VODA?", "KDE JE VODA?")]
    [InlineData("příliš žluťoučký", "PRILIS ZLUTOUCKY")]
    public void Normalize_TransliteratesCzechToMaskSafeText(string input, string expected)
    {
        var result = CzechTextNormalizer.Normalize(input);

        Assert.Equal(expected, result.MaskText);
    }

    [Fact]
    public void Normalize_CollapsesWhitespaceAndReplacesUnsupportedCharacters()
    {
        var result = CzechTextNormalizer.Normalize("  čau   světe 😀 ");

        Assert.Equal("CAU SVETE ?", result.MaskText);
        Assert.True(result.Changed);
    }
}
