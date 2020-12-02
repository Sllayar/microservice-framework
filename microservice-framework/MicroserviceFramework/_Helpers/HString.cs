using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HString
    {
        public static bool IsNullOrEmpty(this string str, bool trim = false) => string.IsNullOrEmpty((trim && str.NotNull()) ? str.Trim() : str);

        public static bool NotEmpty(this string str, bool trim = false) => str.IsNullOrEmpty(trim).Not();


        /* Regex */

        public static string Regex(this string input, string pattern, RegexOptions options = RegexOptions.Singleline) => System.Text.RegularExpressions.Regex.Match(input, pattern, options).Value;
        public static string Regex(this string input, string pattern, string checkContainsBefore, RegexOptions options = RegexOptions.Singleline) => input.Contains(checkContainsBefore) ? System.Text.RegularExpressions.Regex.Match(input, pattern, options).Value : "";

        public static List<string> RegexAll(this string input, string pattern, RegexOptions options = RegexOptions.Singleline) => input.IsNullOrEmpty() ? new List<string>() : System.Text.RegularExpressions.Regex.Matches(input, pattern, options).Cast<object>().Select(match => match.ToString()).ToList();

        public static List<string> RegexAll(this string input, string pattern, string checkContainsBefore, RegexOptions options = RegexOptions.Singleline)
        {
            return input.IsNullOrEmpty() || input.NotContains(checkContainsBefore) ? new List<string>() : System.Text.RegularExpressions.Regex.Matches(input, pattern, options).Cast<object>().Select(match => match.ToString()).ToList();
        }

        public static string RegexReplace(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.Singleline) => System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement, options);

        public static string RegexRemove(this string input, string pattern, RegexOptions options = RegexOptions.Singleline) => input.RegexReplace(pattern, "", options);
        public static string RegexRemove(this string input, string pattern, string checkContainsBefore, RegexOptions options = RegexOptions.Singleline) => input.Contains(checkContainsBefore) ? input.RegexReplace(pattern, "", options) : input;

        public static bool Match(this string input, string pattern, RegexOptions options = RegexOptions.Singleline) => System.Text.RegularExpressions.Regex.IsMatch(input, pattern, options);

        public static bool NotMatch(this string input, string pattern, RegexOptions options = RegexOptions.Singleline) => Match(input, pattern, options).Not();

        public static string RegexEscape(this string str) => System.Text.RegularExpressions.Regex.Escape(str);


        // удалить вхождение
        public static string Remove(this string str, string oldChar, bool ignoreCase = false)
        {
            if(ignoreCase.Not()) return str.Replace(oldChar, "");

            // если игнорируем регистр
            if(str.Length == 0 || oldChar.Length == 0) return str;
            var result = new StringBuilder();
            var startingPos = 0;
            int nextMatch;
            while((nextMatch = str.IndexOf(oldChar, startingPos, StringComparison.OrdinalIgnoreCase)) > -1)
            {
                result.Append(str, startingPos, nextMatch - startingPos);
                startingPos = nextMatch + oldChar.Length;
            }

            result.Append(str, startingPos, str.Length - startingPos);

            return result.ToString();
        }

        // удалить массив вхождений
        public static string Remove(this string str, params string[] oldChars) => oldChars.Aggregate(str, (current, oldChar) => current.Remove(oldChar));

        // заменить множество строк
        public static string Replace(this string str, IEnumerable<string> oldStrings, string newChar) => oldStrings.Aggregate(str, (current, oldChar) => current.Replace(oldChar, newChar));

        public static string Replace(this string str, params (string, string)[] pairs)
        {
            foreach(var (oldStr, newStr) in pairs) str = str.Replace(oldStr, newStr);
            return str;
        }

        public static string Replace(this string str, params (char, char)[] pairs)
        {
            foreach(var (oldStr, newStr) in pairs) str = str.Replace(oldStr, newStr);
            return str;
        }


        // сравниваем строки, приводя их к нижнему регистру
        public static bool CompareIgnoreCase(this string str1, string str2) => string.Equals(str1, str2, StringComparison.CurrentCultureIgnoreCase);
        
        public static bool CompareIgnoreCaseAny(this string str1, params string[] items) => items.Any(str1.CompareIgnoreCase);


        // равен одному из
        public static bool EqualsAny(this string str, params string[] items) => items.Any(str.Equals);

        // не содержит
        public static bool NotContains(this string str, string value) => str.Contains(value).Not();

        // содержит один из
        public static bool ContainsAny(this string str, params string[] items) => items.Any(str.Contains);
        
        // количество вхождений подстроки
        public static int SubstrCount(this string str, string needle) => (str.Length - str.Replace(needle, "").Length) / needle.Length;

        // заканчивается на
        public static bool EndsWithAny(this string str, params string[] items) => items.Any(str.EndsWith);


        // первая буква большая
        public static string FUcase(this string str) => str.IsNullOrEmpty(true) ? str : str[0].ToString().ToUpper() + str.Substring(1);

        // первая буква большая
        public static string FUcaseEachWord(this string str) => str.IsNullOrEmpty(true) ? str : str.Split(' ').Aggregate("", (current, word) => current + word.FUcase() + " ").Trim();


        // первая буква маленькая
        public static string FLcase(this string word) => word[0].ToString().ToLower() + word.Substring(1);

        // заменяет только первое вхождение
        public static string FReplace(this string text, string oldStr, string newStr)
        {
            var f1 = text.IndexOf(oldStr, StringComparison.Ordinal);
            text = text.Remove(f1, oldStr.Length);
            return text.Insert(f1, newStr);
        }


        // транслит
        // транслит
        public static string Translit(this string str, string separator = " ")
        {
            if(str.IsNullOrEmpty()) return str;

            str = str

                .ToLower()

                .Replace("-", " ")

                .RegexReplace("  +", " ")

                .Remove(new [] { ",", ".", ";", "…", "—", "_", "«", "»", "!", "?", "№", "*", "`", "=", "%", "+", "/", @"\", "|", ":", "(", ")", "'", "[", "]", "<", ">", "&", "·", "#", "©", "^", "@", "$", "\"" })

                .Trim()

                .Replace("а", "a")
                .Replace("б", "b")
                .Replace("в", "v")
                .Replace("г", "g")
                .Replace("д", "d")
                .Replace("е", "e")
                .Replace("ё", "e")
                .Replace("ж", "zh")
                .Replace("з", "z")
                .Replace("и", "i")
                .Replace("й", "j")
                .Replace("к", "k")
                .Replace("л", "l")
                .Replace("м", "m")
                .Replace("н", "n")
                .Replace("о", "o")
                .Replace("п", "p")
                .Replace("р", "r")
                .Replace("с", "s")
                .Replace("т", "t")
                .Replace("у", "u")
                .Replace("ф", "f")
                .Replace("х", "h")
                .Replace("ц", "c")
                .Replace("ч", "ch")
                .Replace("ш", "sh")
                .Replace("щ", "sch")
                .Replace("ъ", "")
                .Replace("ы", "y")
                .Replace("ь", "")
                .Replace("э", "e")
                .Replace("ю", "yu")
                .Replace("я", "ya")
                .Replace("ї", "j")
                .Replace("є", "e")
                .Replace("і", "i")

                .Replace(" ", separator);

            return str;
        }


        public static string URLEncode(this string str, Encoding encoding = null) => encoding.IsNull() ? HttpUtility.UrlEncode(str) : HttpUtility.UrlEncode(str, encoding);

        public static string URLDecode(this string str, Encoding encoding = null) => encoding.IsNull() ? HttpUtility.UrlDecode(str) : HttpUtility.UrlDecode(str, encoding);

        public static string URLEncodeANSI(this string str) => HttpUtility.UrlEncode(str, Encoding.GetEncoding(1251));

        public static string URLDecodeANSI(this string str) => HttpUtility.UrlDecode(str, Encoding.GetEncoding(1251));

        public static string PunyEncode(this string str) => new IdnMapping().GetAscii(str);

        public static string PunyDecode(this string str) => str.StartsWith("xn--") ? new IdnMapping().GetUnicode(str) : str;


        public static string MD5(this string str)
        {
            var md5Hasher = System.Security.Cryptography.MD5.Create(); // создаем объект этого класса, он создается не через new, а вызовом метода Create
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(str)); // преобразуем входную строку в массив байт и вычисляем хэш
            var sBuilder = new StringBuilder(); // cоздаем новый Stringbuilder 
            foreach(var t in data) sBuilder.Append(t.ToString("x2")); // указывает, что нужно преобразовать элемент в шестнадцатиричную строку длиной в два символа
            return sBuilder.ToString();
        }


        // архивируем строку
        public static byte[] CompressString(this string str)
        {
            var byteArray = new byte[0];
            if(str.NotEmpty())
            {
                byteArray = Encoding.UTF8.GetBytes(str);
                using var stream = new MemoryStream();
                using(var gZipStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    gZipStream.Write(byteArray, 0, byteArray.Length);

                    gZipStream.Close();
                    gZipStream.Dispose();
                }

                byteArray = stream.ToArray();

                stream.Close();
                stream.Dispose();
            }

            return byteArray;
        }

        // разархивируем строку
        public static string DecompressString(this byte[] bytes)
        {
            var resultString = string.Empty;
            if(bytes.NotNull() && bytes.Length > 0)
            {
                using var memoryStream = new MemoryStream(bytes);
                using(var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using(var streamReader = new StreamReader(gZipStream))
                    {
                        resultString = streamReader.ReadToEnd();

                        streamReader.Close();
                        streamReader.Dispose();
                    }
                    gZipStream.Close();
                    gZipStream.Dispose();
                }
                memoryStream.Close();
                memoryStream.Dispose();
            }

            return resultString;
        }


        // BASE64
        public static string Base64Encode(this string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

        public static string Base64Decode(this string str) => Encoding.UTF8.GetString(Convert.FromBase64String(str ?? ""));

        // from JWT spec
        public static string Base64UrlNormalize(this string input) => input.Replace(('-', '+'), ('_', '/')) + ((input.Length % 4) switch { 0 => "", 1 => "===", 2 => "==", 3 => "=", _ => throw new Exception("Illegal base64url string") });
        public static string Base64UrlDecode(this string input) => input.Base64UrlNormalize().Base64Decode();
    }
}