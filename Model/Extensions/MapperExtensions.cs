using System;
using MapsterMapper;

namespace Model.Extensions;

public static class MapperExtensions
{
    private static Mapper mapper = new Mapper();

    public static T With<T, TBase>(this T t, TBase with) where T : TBase
    {
        var res = mapper.Map<T>(t);
        mapper.Map(with, res);

        return res;
    }

    public static T With<T>(this T t, Action<T> modify)
    {
        var res = mapper.Map<T>(t);
        modify(res);

        return res;
    }
}
