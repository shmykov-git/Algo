using System;
using System.Collections.Generic;

namespace Model.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<int> Index<T>(this IEnumerable<T> list)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                yield return i++;
        }

        public static IEnumerable<T> Evens<T>(this IEnumerable<T> list)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                if (i++ % 2 == 0)
                    yield return enumerator.Current;
        }

        public static IEnumerable<T> Triples<T>(this IEnumerable<T> list)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                if (i++ % 3 == 0)
                    yield return enumerator.Current;
        }

        public static IEnumerable<(T, T)> CirclePairs<T>(this IEnumerable<T> list) => list.SelectCirclePair((a, b) => (a, b));

        public static IEnumerable<TRes> SelectCirclePair<T, TRes>(this IEnumerable<T> list, Func<T, T, TRes> func)
        {
            var i = 0;
            var prevT = default(T);
            var first = default(T);
            foreach (var t in list)
            {
                if (i++ == 0)
                    first = t;
                else
                    yield return func(prevT, t);
                prevT = t;
            }
            yield return func(prevT, first);
        }
    }
}
