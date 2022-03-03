using System.Collections.Generic;

namespace Model.Extensions
{
    public static class StringExtensions
    {
        public static bool HasText(this string value) => !string.IsNullOrEmpty(value);

        public static string SJoin<T>(this IEnumerable<T> values, string separator = ", ") => string.Join(separator, values);
    }
}