using System.Globalization;
using System.Text;
namespace WebQuanLyNhaHang.Models
{
    public class StringUtils
    {
            public static string ConvertToLowerAndRemoveDiacritics(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return input;

                // Chuyển đổi thành chữ thường
                string lowerCase = input.ToLower();

                // Bỏ dấu
                var normalizedString = lowerCase.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (char c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    // Chỉ giữ lại ký tự không có dấu
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            }
    }
}
