using System.Globalization;
using System.Text;

namespace SumiClas.Shared.Domain;

/// <summary>
/// Canonical form used to compare and search names: trimmed, single-spaced, lowercase and without diacritics.
/// The tilde of the letter n is removed too (the Spanish words for "mountain" and "to snow" collapse to the same text
/// as their unaccented spelling), so a search typed without accents still matches.
/// The same function must be applied to the stored text and to the text typed by the user.
/// </summary>
public static class TextNormalizer
{
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string decomposed = text.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder(decomposed.Length);
        bool previousWasSpace = false;
        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                if (!previousWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                }
                previousWasSpace = true;
                continue;
            }

            builder.Append(char.ToLowerInvariant(character));
            previousWasSpace = false;
        }

        return builder.ToString().TrimEnd();
    }
}
