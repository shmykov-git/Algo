namespace Model.Extensions
{
    public static class StringExtensions
    {
        public static bool HasText(this string value) => !string.IsNullOrEmpty(value);
    }
}