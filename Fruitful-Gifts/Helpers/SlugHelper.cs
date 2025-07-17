using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace Fruitful_Gifts.Helpers
{
    public class SlugHelper
    {
        // Bỏ dấu tiếng Việt
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            text = RemoveDiacritics(text).ToLowerInvariant();         
            text = Regex.Replace(text, @"[^a-z0-9\s/-]", "");
            text = text.Replace("/", "-");
            text = Regex.Replace(text, @"\s+", "-").Trim('-');         
            text = Regex.Replace(text, @"-+", "-");                  

            return text;
        }
    }
}
