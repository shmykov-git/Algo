using System.Text.Json;

namespace Meta.Extensions
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T o)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(o));
        }
        public static T Copy<T>(this object o)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(o));
        }
    }
}
