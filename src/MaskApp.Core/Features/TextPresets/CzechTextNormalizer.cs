using System.Text;

namespace MaskApp.Core.Features.TextPresets;

public static class CzechTextNormalizer
{
    private static readonly Dictionary<char, char> CzechMap = new()
    {
        ['Á'] = 'A', ['Č'] = 'C', ['Ď'] = 'D', ['É'] = 'E', ['Ě'] = 'E',
        ['Í'] = 'I', ['Ň'] = 'N', ['Ó'] = 'O', ['Ř'] = 'R', ['Š'] = 'S',
        ['Ť'] = 'T', ['Ú'] = 'U', ['Ů'] = 'U', ['Ý'] = 'Y', ['Ž'] = 'Z',
        ['á'] = 'A', ['č'] = 'C', ['ď'] = 'D', ['é'] = 'E', ['ě'] = 'E',
        ['í'] = 'I', ['ň'] = 'N', ['ó'] = 'O', ['ř'] = 'R', ['š'] = 'S',
        ['ť'] = 'T', ['ú'] = 'U', ['ů'] = 'U', ['ý'] = 'Y', ['ž'] = 'Z'
    };

    public static CzechTextNormalizationResult Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new CzechTextNormalizationResult(string.Empty, string.Empty, false);
        }

        var builder = new StringBuilder(input.Length);
        var previousWasSpace = false;
        var previousWasHighSurrogate = false;
        foreach (var rawCharacter in input.Trim())
        {
            if (previousWasHighSurrogate && char.IsLowSurrogate(rawCharacter))
            {
                previousWasHighSurrogate = false;
                continue;
            }

            previousWasHighSurrogate = false;
            if (char.IsWhiteSpace(rawCharacter))
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            if (char.IsHighSurrogate(rawCharacter))
            {
                builder.Append('?');
                previousWasHighSurrogate = true;
                previousWasSpace = false;
                continue;
            }

            previousWasSpace = false;
            var character = CzechMap.TryGetValue(rawCharacter, out var transliterated)
                ? transliterated
                : char.ToUpperInvariant(rawCharacter);
            builder.Append(IsSupportedAscii(character) ? character : '?');
        }

        var maskText = builder.ToString();
        var displayText = CollapseWhitespace(input.Trim());
        return new CzechTextNormalizationResult(
            displayText,
            maskText,
            !string.Equals(displayText, maskText, StringComparison.Ordinal));
    }

    private static bool IsSupportedAscii(char character) => character is >= ' ' and <= '~';

    private static string CollapseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasSpace = false;
        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            builder.Append(character);
            previousWasSpace = false;
        }

        return builder.ToString();
    }
}

public sealed record CzechTextNormalizationResult(
    string InputText,
    string MaskText,
    bool Changed);
