using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace _1.RabbitMQ.Producer
{
    public static class StringExtensions
    {
        public static string JoinString(this IEnumerable<string> arr, string character)
        {
            if (arr is null) return string.Empty;
            var enumerable = arr as string[] ?? arr.ToArray();
            return string.Join(character, enumerable);
        }

        public static Guid ToGuid(this string str)
        {
            return Guid.TryParse(str, out var guid) ? guid : Guid.Empty;
        }

        public static string ToSha256<T>(T value)
        {
            var valueJson = value.Serialize();
            var stringBuilder = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                var encoding = Encoding.UTF8;
                var result = hash.ComputeHash(encoding.GetBytes(valueJson));

                foreach (var byteCode in result)
                    stringBuilder.Append(byteCode.ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        public static string CreateMd5(this string input, string prefix = null)
        {
            if (string.IsNullOrEmpty(input)) return null;
            using var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var hashByte in hashBytes)
                sb.Append(hashByte.ToString("X2"));
            var prefixMd5 = !string.IsNullOrEmpty(prefix) ? $"{prefix}_" : string.Empty;
            return $"{prefixMd5}{sb}";
        }

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string FullTextSearch<T>(this T data)
        {
            var content = string.Empty;
            var props = data.GetType().GetProperties();
            content = props.Where(x => x.PropertyType == typeof(string))
                .Select(propertyInfo => propertyInfo.Name != "Search"
                    ? data.GetType().GetProperty(propertyInfo.Name)?.GetValue(data, null)
                    : null)
                .Where(value => value != null)
                .Aggregate(content,
                    (current, value) => current + (" " + value.ToString().RemoveDiacritics().FulltextSearchPretreatment()));

            return content.Trim();
        }

        public static string KeywordPretreatment(this string data)
        {
            return Regex.Split(data.RemoveDiacritics(), @"\s+").Select(x => $"-{x}-").JoinString(" and ");
        }

        public static string FulltextSearchPretreatment(this string data)
        {
            return data.Split(" ").Select(x => $"-{x}-").JoinString(" ");
        }

        public static string RemoveDiacritics(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            try
            {
                str = str.ToLower().Trim();
                str = Regex.Replace(str, @"[\r|\n]", " ");
                str = Regex.Replace(str, @"\s+", " ");
                str = Regex.Replace(str, "[áàảãạâấầẩẫậăắằẳẵặ]", "a");
                str = Regex.Replace(str, "[éèẻẽẹêếềểễệ]", "e");
                str = Regex.Replace(str, "[iíìỉĩị]", "i");
                str = Regex.Replace(str, "[óòỏõọơớờởỡợôốồổỗộ]", "o");
                str = Regex.Replace(str, "[úùủũụưứừửữự]", "u");
                str = Regex.Replace(str, "[yýỳỷỹỵ]", "y");
                str = Regex.Replace(str, "[đ]", "d");

                //Remove special character
                str = Regex.Replace(str, "[\"`~!@#$%^&*()\\-+=?/>.<,{}[]|]\\]", "");
                return str.Trim();
            }
            catch (Exception)
            {
                return str;
            }
        }

        public static bool FullTextContains(this string processedInput, string query)
        {
            var inputWords = Regex.Matches(processedInput, "-([^\\-]*)-")
                .Select(m => m.Groups[1].Value.ToLower())
                .ToList();
            var queryWords = Regex.Split(query.RemoveDiacritics(), @"\s+").Select(x => x.ToLower()).ToList();

            return !queryWords.Except(inputWords).Any(); ;
        }
    }
}
