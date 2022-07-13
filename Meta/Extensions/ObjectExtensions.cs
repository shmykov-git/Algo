using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
