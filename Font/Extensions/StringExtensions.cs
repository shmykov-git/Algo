namespace Font.Extensions;

public static class StringExtensions
{
    public static (string, string) SplitByTwo(this string value, params char[] cs)
    {
        var parts = value.Split(cs);
        return (parts[0], parts[1]);
    }
}
