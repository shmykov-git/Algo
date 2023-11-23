namespace Model.Extensions;

public static class NullableExtensions
{
    public static (bool hasValue, T value) SplitNullable<T>(this T? t) where T : struct => (t.HasValue, t ?? default);
}
