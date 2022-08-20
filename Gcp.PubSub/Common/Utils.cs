using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gcp.PubSub.Common
{
    internal static class Utils
    {
        public static bool IsEmpty(this string str) => string.IsNullOrWhiteSpace(str);

        public static bool IsNotEmpty(this string str) => !string.IsNullOrWhiteSpace(str);

        public static string ToTitle(this string str, bool onlyFirstWord = false)
        {
            if (str.IsEmpty())
            {
                return str;
            }

            if (onlyFirstWord)
            {
                return char.ToUpper(str[0], CultureInfo.InvariantCulture) + (str.Length == 1 ? string.Empty : str[1..]);
            }

            var array = str.Split(' ');

            return string.Join(' ', array.Select(x => x.ToTitle(true)));
        }

        public static string ToCamelCase(this string str)
        {
            if (str.IsEmpty())
            {
                return str;
            }

            var enGbCulture = new CultureInfo("en-GB");

            return new string(
                enGbCulture
                    .TextInfo
                    .ToTitleCase(string.Join(" ", Pattern.Matches(str)).ToLower(enGbCulture))
                    .Replace(@" ", "")
                    .Select((x, i) => i == 0 ? char.ToLower(x, enGbCulture) : x)
                    .ToArray()
            );
        }

        public static string Dump(this object obj, string comment = "", bool truncate = true) => new ObjectSerializer().ToJson(obj, comment, truncate);

        private static readonly Regex Pattern = new(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+", RegexOptions.Compiled);
    }
}
