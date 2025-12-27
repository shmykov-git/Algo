using Newtonsoft.Json;

namespace meta.Extensions;

public static class JsonExtensions
{
    public static T? FromJson<T>(this string json)
    {
        if (json == null)
            return default;

        if (typeof(T) == typeof(string))
            return (T)(object)json;

        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (JsonSerializationException)
        {
            return default;
        }
        catch (JsonReaderException)
        {
            return default;
        }
    }

    public static string ToJson<T>(this T obj, Formatting formatting = Formatting.Indented)
    {
        return JsonConvert.SerializeObject(obj, formatting);
    }

    public static string ToJsonInLine<T>(this T obj)
    {
        return obj.ToJson(Formatting.None);
    }
}
