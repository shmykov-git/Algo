using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Extensions
{
    public static class ValueTupleExtensions
    {
        public static IEnumerable<(int i, int j)> Range(this (int m, int n) range) =>
            range.SelectRange((i, j) => (i, j));

        public static IEnumerable<T> SelectRange<T>(this (int m, int n) range, Func<int, int, T> selectFn)
        {
            return Enumerable.Range(0, range.m).SelectMany(i => Enumerable.Range(0, range.n).Select(j => selectFn(i, j)));
        }

        public static IEnumerable<T> SelectSnakeRange<T>(this (int m, int n) range, Func<int, int, T> selectFn)
        {
            return range.SelectRange((i, j) => i % 2 == 0 ? selectFn(i, j) : selectFn(i, range.n - j - 1));
        }

        public static IEnumerable<T> SelectRange<T>(this ((int start, int count) m, (int start, int count) n) range, Func<int, int, T> selectFn)
        {
            return Enumerable.Range(range.m.start, range.m.count).SelectMany(i => Enumerable.Range(range.n.start, range.n.count).Select(j => selectFn(i, j)));
        }

        public static IEnumerable<T> SelectMiddleRange<T>(this (int m, int n) range, Func<int, int, T> selectFn)
        {
            return Enumerable.Range(0, range.m).SelectMany(i =>
                Enumerable.Range(0, range.n).Select(j => selectFn(i - range.m / 2, j - range.n / 2)));
        }

        public static IEnumerable<T> SelectRange<T>(this (int m, int n, int l) range, Func<int, int, int, T> selectFn)
        {
            return Enumerable.Range(0, range.m).SelectMany(i => Enumerable.Range(0, range.n).SelectMany(j => Enumerable.Range(0, range.l).Select(k => selectFn(i, j, k))));
        }

        public static IEnumerable<int> Range(this int n) => n.SelectRange(i => i);

        public static IEnumerable<T> SelectRange<T>(this int range, Func<int, T> selectFn)
        {
            return Enumerable.Range(0, range).Select(i => selectFn(i));
        }

        public static void ForEach(this int range, Action<int> action)
        {
            Enumerable.Range(0, range).ForEach(action);
        }

        public static IEnumerable<T> SelectMiddleRange<T>(this int range, Func<int, T> selectFn)
        {
            return Enumerable.Range(0, range).Select(i => selectFn(i - range/2));
        }

        public static (int i, int j) OrderedEdge(this (int i, int j) e)
        {
            return e.i < e.j ? e : (e.j, e.i);
        }

        public static (int i, int j) Add(this (int i, int j) a, (int i, int j) b)
        {
            return (a.i + b.i, a.j + b.j);
        }

        public static (int i, int j) Sub(this (int i, int j) a, (int i, int j) b)
        {
            return (a.i - b.i, a.j - b.j);
        }

        public static int Mult(this (int i, int j) a, (int i, int j) b)
        {
            return a.i * b.i + a.j * b.j;
        }

        public static (int i, int j) Normal(this (int i, int j) a)
        {
            return (a.j, -a.i);
        }

        public static (int i, int j) Direct(this (int i, int j) a, (int i, int j) b)
        {
            if (a.i.Abs() > 1 || a.j.Abs() > 1 || b.i.Abs() > 1 || b.j.Abs() > 1)
                throw new ArgumentException($"Args should have -1, 0, 1 values. Actual {a} {b}");

            if (a == (0, 0))
                return b;

            var i = a.Mult(b);
            var j = a.Normal().Mult(b);

            return (i.Abs() == 2 ? i / 2 : i, j.Abs() == 2 ? j / 2 : j);
        }

        public static (int i, int j) ReversedEdge(this (int i, int j) e)
        {
            return (e.j, e.i);
        }

        public static (int i, int j) Reverse(this (int i, int j) e)
        {
            return (e.j, e.i);
        }

        public static int Another(this (int i, int j) e, int k)
        {
            return e.i == k ? e.j : e.i;
        }

        public static string ToStr(this ((double r, double i) c, double k)[] args)
        {
            return string.Join(", ", args.Select(a => $"(({a.c.r}, {a.c.i}), {a.k})"));
        }
    }
}
