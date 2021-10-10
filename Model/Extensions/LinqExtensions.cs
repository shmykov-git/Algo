using Model.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<(int index, T value)> IndexValue<T>(this IEnumerable<T> list)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                yield return (i++, enumerator.Current);
        }

        public static IEnumerable<TRes> SelectWithIndex<T, TRes>(this IEnumerable<T> list, Func<T, int, TRes> func)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                yield return func(enumerator.Current, i++);
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
            
            if (i > 1)
                yield return func(prevT, first);
        }

        public static void ForEachCirclePair<T>(this IEnumerable<T> list, Action<T, T> action)
        {
            foreach (var _ in list.SelectCirclePair((a, b) =>
            {
                action(a, b);
                return true;
            }));
        }

        public static IEnumerable<TRes> SelectCircleTriple<T, TRes>(this IEnumerable<T> list, Func<T, T, T, TRes> func)
        {
            var i = 0;
            var prevPrevT = default(T);
            var prevT = default(T);
            var firstT = default(T);
            var secondT = default(T);
            foreach (var t in list)
            {
                if (i == 0)
                    firstT = t;
                else if (i == 1)
                    secondT = t;
                else
                    yield return func(prevPrevT, prevT, t);

                prevPrevT = prevT;
                prevT = t;
                i++;
            }

            yield return func(prevPrevT, prevT, firstT);
            yield return func(prevT, firstT, secondT);
        }

        public static IEnumerable<TRes> SelectCircleGroup<T, TRes>(this IEnumerable<T> list, int groupSize, Func<T[], TRes> func)
        {
            var group = new Queue<T>(groupSize);

            foreach (var t in list.Concat(list.Take(groupSize-1)))
            {
                if (group.Count < groupSize - 1)
                    group.Enqueue(t);
                else
                {
                    group.Enqueue(t);
                    yield return func(group.ToArray());
                    group.Dequeue();
                }
            }
        }

        public static IEnumerable<TRes> SelectPair<T, TRes>(this IEnumerable<T> list, Func<T, T, TRes> func)
        {
            var i = 0;
            var prevT = default(T);
            foreach (var t in list)
            {
                if (i++ == 0)
                { }
                else
                    yield return func(prevT, t);
                prevT = t;
            }
        }

        public static IEnumerable<T> OrderSafeDistinct<T>(this IEnumerable<T> list) where T : IEquatable<T>
        {
            var values = new HashSet<T>();
            foreach(var item in list)
            {
                if (values.Contains(item))
                    continue;

                values.Add(item);
                yield return item;
            }
        }

        public static (int[] bi, bool[] filter) DistinctBi<TItem>(this TItem[] items)
        {
            return Indexer.DistinctBi(items);
        }

        public static (int[] bi, List<TItem> items) RemoveBi<TItem>(this IEnumerable<TItem> items, IEnumerable<TItem> removeItems)
        {
            return Indexer.RemoveBi(items, removeItems);
        }
    }
}
