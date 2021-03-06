using Model.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meta;
using Meta.Tools;
using ThreadPool = Meta.Tools.ThreadPool;

namespace Model.Extensions
{
    public enum ForEachAction
    {
        Keep,
        Remove
    }

    public static class LinqExtensions
    {
        private static ThreadPool threadPool = DI.Get<ThreadPool>();

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            var enumerator = list.GetEnumerator();

            while (enumerator.MoveNext())
                action(enumerator.Current);
        }

        public static void ForEachRemovable<T>(this IList<T> list, Func<T, ForEachAction> func)
        {
            var i = 0;
            do
            {
                var value = list[i];
                var state = func(value);

                switch (state)
                {
                    case ForEachAction.Keep:
                        i++;
                        break;

                    case ForEachAction.Remove:
                        list.RemoveAt(i);
                        break;

                    default:
                        throw new NotImplementedException(state.ToString());
                }
            } while (i < list.Count);
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T, int> action)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();

            while (enumerator.MoveNext())
                action(enumerator.Current, i++);
        }

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

        public static IEnumerable<(T v, int i)> WithIndex<T>(this IEnumerable<T> list) =>
            list.SelectWithIndex((v, i) => (v, i));

        public static IEnumerable<TRes> SelectWithIndex<T, TRes>(this IEnumerable<T> list, Func<T, int, TRes> func)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                yield return func(enumerator.Current, i++);
        }

       
        /// <summary>
        /// Четные
        /// </summary>
        public static IEnumerable<T> Evens<T>(this IEnumerable<T> list)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                if (i++ % 2 == 0)
                    yield return enumerator.Current;
        }

        /// <summary>
        /// Нечетные
        /// </summary>
        public static IEnumerable<T> Odds<T>(this IEnumerable<T> list)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                if (i++ % 2 == 1)
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

        public static IEnumerable<(T a, T b)> SelectCirclePair<T>(this IEnumerable<T> list) => list.SelectCirclePair((a, b) => (a, b));
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

        public static IEnumerable<(T a, T b, T c)> SelectCircleTriple<T>(this IEnumerable<T> list) =>
            list.SelectCircleTriple((a, b, c) => (a, b, c));
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

            if (i == 1)
            {
                yield return func(firstT, firstT, firstT);
            } 
            else
            {
                yield return func(prevPrevT, prevT, firstT);
                yield return func(prevT, firstT, secondT);
            }
        }

        public static IEnumerable<TRes> SelectCircleFours<T, TRes>(this IEnumerable<T> list, Func<T, T, T, T, TRes> func)
        {
            var i = 0;
            var prevPrevPrevT = default(T);
            var prevPrevT = default(T);
            var prevT = default(T);
            var firstT = default(T);
            var secondT = default(T);
            var thirdT = default(T);
            foreach (var t in list)
            {
                if (i == 0)
                    firstT = t;
                else if (i == 1)
                    secondT = t;
                else if (i == 2)
                    thirdT = t;
                else
                    yield return func(prevPrevPrevT, prevPrevT, prevT, t);

                prevPrevPrevT = prevPrevT;
                prevPrevT = prevT;
                prevT = t;
                i++;
            }

            yield return func(prevPrevPrevT, prevPrevT, prevT, firstT);
            yield return func(prevPrevT, prevT, firstT, secondT);
            yield return func(prevT, firstT, secondT, thirdT);
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

        public static IEnumerable<(T a, T b)> SelectPair<T>(this IEnumerable<T> list) => list.SelectPair((a, b) => (a, b));
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

        public static IEnumerable<TRes> SelectByPair<T, TRes>(this IEnumerable<T> list, Func<T, T, TRes> func)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            T prev = default;
            while (enumerator.MoveNext())
                if (i++ % 2 == 1)
                {
                    yield return func(prev, enumerator.Current);
                }
                else
                {
                    prev = enumerator.Current;
                }

            if (i % 2 == 1)
                yield return func(prev, default);
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

        public static IEnumerable<(TItem a, TItem b)> CrossSelect<TItem>(this (TItem[] aItems, TItem[] bItems) items) =>
            items.CrossSelect((a, b) => (a, b));
        public static IEnumerable<TRes> CrossSelect<TItem, TRes>(this (TItem[] aItems, TItem[] bItems) items, Func<TItem, TItem, TRes> func)
        {
            return items.aItems.SelectMany(a => items.bItems.Select(b => func(a, b)));
        }

        public static TItem[] ManyToArray<TItem>(this IEnumerable<IEnumerable<TItem>> manyItems) =>
            manyItems.SelectMany(v => v).ToArray();

        public static TOut[] SelectInParallel<TIn, TOut>(this TIn[] items, Func<TIn, TOut> processFn)
        {
            if (items.Length == 0)
                return Array.Empty<TOut>();

            var e = new AutoResetEvent(false);
            var results = new TOut[items.Length];

            var n = threadPool.ThreadsCount < items.Length ? threadPool.ThreadsCount : items.Length;
            var offset = 0;
            var completed = new bool[n];

            (n).ForEach(i =>
            {
                var m = items.Length / n + (i < items.Length % n ? 1 : 0);
                var k = offset;
                offset += m;

                threadPool.Run(() =>
                {
                    (m).ForEach(j => results[k + j] = processFn(items[k + j]));
                    completed[i] = true;

                    if (completed.All(v=>v))
                        e.Set();
                });
            });

            e.WaitOne();

            return results;
        }

        public static void ForEachInParallel<TIn>(this TIn[] items, Action<TIn> processFn)
        {
            if (items.Length == 0)
                return;

            var e = new AutoResetEvent(false);
            var counter = 0;
            var chunk = (items.Length - 1) / (2 * threadPool.ThreadsCount) + 1;

            items
                .Select((item, i) => (item, i))
                .GroupBy(v => v.i / chunk)
                .ForEach(gv =>
                {
                    var i = gv.Key * chunk;
                    Interlocked.Increment(ref counter);

                    threadPool.Run(() =>
                    {
                        gv.ForEach(v => processFn(v.item));
                        var v = Interlocked.Decrement(ref counter);

                        if (v == 0)
                            e.Set();
                    });
                });

            e.WaitOne();
        }
    }
}
