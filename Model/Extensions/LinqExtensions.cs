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
using System.Runtime.CompilerServices;
using Model.Trees;
using System.Collections.Concurrent;

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

        public static void ForEach<T>(this IEnumerable<IEnumerable<T>> list, Action<T, int, int> action)
        {
            list.ForEach((vs, i) => vs.ForEach((v, j) => action(v, i, j)));
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

        /// <summary>
        /// Каждый третий, shift = 0, 1, 2
        /// </summary>
        public static IEnumerable<T> Triples<T>(this IEnumerable<T> list, int shift = 0)
        {
            var i = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                if (i++ % 3 == shift)
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

        public static T[] CircleArrayShift<T>(this IEnumerable<T> list, int shift) => list.ToArray().CircleShift(shift);

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

        public static IEnumerable<TRes> SelectTriple<T, TRes>(this IEnumerable<T> list, Func<T, T, T, TRes> func)
        {
            var i = 0;
            var prevPrevT = default(T);
            var prevT = default(T);
            foreach (var t in list)
            {
                if (i >= 2)
                    yield return func(prevPrevT, prevT, t);

                prevPrevT = prevT;
                prevT = t;
                i++;
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

            foreach (var t in list.Concat(list.Take(groupSize - 1)))
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

        public static IEnumerable<TRes> SelectGroup<T, TRes>(this IEnumerable<T> list, int groupSize, Func<T[], TRes> func)
        {
            var group = new Queue<T>(groupSize);

            foreach (var t in list)
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

        public static IEnumerable<(T a, T b, T c)> SelectByTriple<T>(this IEnumerable<T> list) => list.SelectByTriple((a, b, c) => (a, b, c));
        public static IEnumerable<TRes> SelectByTriple<T, TRes>(this IEnumerable<T> list, Func<T, T, T, TRes> func)
        {
            var i = 0;
            var prevPrevT = default(T);
            var prevT = default(T);
            foreach (var t in list)
            {
                if (++i % 3 == 0)
                    yield return func(prevPrevT, prevT, t);

                prevPrevT = prevT;
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

        public static IEnumerable<TItem> SelectMany<TItem>(this IEnumerable<IEnumerable<TItem>> items) => items.SelectMany(v => v);

        public static (TItem1 a, TItem2 b)[] ToBothArray<TItem1, TItem2>(this (TItem1[] aItems, TItem2[] bItems) items) => items.SelectBoth().ToArray();

        public static IEnumerable<(TItem1 a, TItem2 b)> SelectBoth<TItem1, TItem2>(this (TItem1[] aItems, TItem2[] bItems) items) => items.SelectBoth((a, b) => (a, b));
        public static IEnumerable<TRes> SelectBoth<TItem1, TItem2, TRes>(this (TItem1[] aItems, TItem2[] bItems) items, Func<TItem1, TItem2, TRes> func) =>
            items.aItems.Index().Select(i => func(items.aItems[i], items.bItems[i]));

        public static IEnumerable<(TItem a, TItem b)> SelectCross<TItem>(this (TItem[] aItems, TItem[] bItems) items) =>
            items.SelectCross((a, b) => (a, b));

        public static IEnumerable<TRes> SelectCross<TItem, TRes>(this (TItem[] aItems, TItem[] bItems) items, Func<TItem, TItem, TRes> func)
        {
            return items.aItems.SelectMany(a => items.bItems.Select(b => func(a, b)));
        }

        public static TItem[] ManyToArray<TItem>(this IEnumerable<IEnumerable<TItem>> manyItems) =>
            manyItems.SelectMany(v => v).ToArray();
        public static TItem[] ToSingleArray<TItem>(this IEnumerable<IEnumerable<TItem>> manyItems) =>
            manyItems.SelectMany(v => v).ToArray();
        public static List<TItem> ToSingleList<TItem>(this IEnumerable<IEnumerable<TItem>> manyItems) =>
            manyItems.SelectMany(v => v).ToList();
        public static List<TItem> ToVerySingleList<TItem>(this IEnumerable<IEnumerable<IEnumerable<TItem>>> manyItems) =>
            manyItems.SelectMany(v => v).SelectMany(v => v).ToList();

        public static IEnumerable<TItem> ReverseList<TItem>(this List<TItem> items) => ((IEnumerable<TItem>)items).Reverse();

        public static async Task<TOut[]> SelectInParallelAsync<TIn, TOut>(this TIn[] items, Func<TIn, TOut> processFn)
        {
            if (items.Length == 0)
                return Array.Empty<TOut>();

            var e = new TaskCompletionSource();
            var allCompleted = false;
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

                    if (completed.All(v => v))
                    {
                        if (allCompleted)
                            return;

                        Task.Run(() =>
                        {
                            if (allCompleted)
                                return;
                            
                            lock (e)
                            {
                                if (allCompleted)
                                    return;

                                allCompleted = true;
                                e.SetResult();
                            }
                        });
                    }
                });
            });

            await e.Task;

            return results;
        }

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

        public static TItem[] CircleShift<TItem>(this TItem[] items, int shift)
        {
            var newItems = new TItem[items.Length];
            var n = items.Length;

            for (var i = 0; i < items.Length; i++)
            {
                newItems[i] = items[(i - shift + n + n) % n];
            }

            return newItems;
        }

        //public static TItem[] CircleMatrixShift<TItem>(this TItem[] items, int m, int shift)
        //{
        //    var newItems = new TItem[items.Length];
        //    var n = items.Length;

        //    for (var i = 0; i < items.Length; i++)
        //    {
        //        newItems[i] = items[(i - shift + n + n) % n];
        //    }

        //    return newItems;
        //}

        public static TItem MinOrDefault<TItem>(this IEnumerable<TItem> values, TItem defaultValue = default) where TItem : IComparable<TItem>
        {
            var enumerator = values.GetEnumerator();
            TItem min;

            if (enumerator.MoveNext())
                min = enumerator.Current;
            else
                return defaultValue;

            while (enumerator.MoveNext())
                if (min.CompareTo(enumerator.Current) > 0)
                    min = enumerator.Current;

            return min;
        }

        public static TItem MaxOrDefault<TItem>(this IEnumerable<TItem> values, TItem defaultValue = default) where TItem : IComparable<TItem>
        {
            var enumerator = values.GetEnumerator();
            TItem max;

            if (enumerator.MoveNext())
                max = enumerator.Current;
            else
                return defaultValue;

            while (enumerator.MoveNext())
                if (max.CompareTo(enumerator.Current) < 0)
                    max = enumerator.Current;

            return max;
        }

        public static TItem TopLast<TItem>(this IEnumerable<TItem> values, int top)
        {
            var enumerator = values.GetEnumerator();
            var count = 0;

            while (count <= top && enumerator.MoveNext())
                count++;

            return enumerator.Current;
        }

        public static IEnumerable<TItem> While<TItem>(this IEnumerable<TItem> values, Func<TItem, bool> continueCondition)
        {
            foreach(var item in values)
            {
                if (continueCondition(item))
                    yield return item;
                else
                    break;
            }
        }

        public static IEnumerable<T> ToSync<T>(this IAsyncEnumerable<T> values)
        {
            var e = values.GetAsyncEnumerator();

            var hasCurrent = false;
            var stop = false;

            Task.Run(async () =>
            {
                while (await e.MoveNextAsync())
                {
                    hasCurrent = true;
                    
                    while(hasCurrent)
                        await Task.Delay(1);
                }

                stop = true;
            });

            while (!stop)
            {
                while (!hasCurrent)
                    Thread.Sleep(1);
                
                yield return e.Current;

                hasCurrent = false;
            }
        }
    }
}
