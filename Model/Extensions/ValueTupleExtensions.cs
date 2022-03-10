using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Extensions
{
    public static class ValueTupleExtensions
    {
        public static IEnumerable<T> SelectRange<T>(this (int m, int n) range, Func<int, int, T> selectFn)
        {
            return Enumerable.Range(0, range.m).SelectMany(i => Enumerable.Range(0, range.n).Select(j => selectFn(i, j)));
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

        public static IEnumerable<T> SelectRange<T>(this int range, Func<int, T> selectFn)
        {
            return Enumerable.Range(0, range).Select(i => selectFn(i));
        }

        public static IEnumerable<T> SelectMiddleRange<T>(this int range, Func<int, T> selectFn)
        {
            return Enumerable.Range(0, range).Select(i => selectFn(i - range/2));
        }

        public static (int i, int j) OrderedEdge(this (int i, int j) e)
        {
            return e.i < e.j ? e : (e.j, e.i);
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
