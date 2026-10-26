using System.Globalization;
using System.Text;

namespace HotelChain.Domain.Common;

public static class TextNormalizer
{
    /// <summary>
    /// Lower-cases the text and strips diacritics so searches ignore case and accents
    /// ("bogota" matches "Bogotá").
    /// </summary>
    public static string ForSearch(string value)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(c));
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
