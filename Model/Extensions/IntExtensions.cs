using Model3D.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Extensions
{
    public static class IntExtensions
    {
        public static int Abs(this int a) => Math.Abs(a);

        public static bool Even(this int a) => a.Abs() % 2 == 0;
        public static bool Odd(this int a) => a.Abs() % 2 == 1;

        public static int[][] Shift(this int[][] lists, int shift)
        {
            return lists.Transform(i => i + shift);
        }

        public static int[][] Transform(this IEnumerable<int[]> lists, Func<int, int> transformFn)
        {
            return lists.Select(list => list.Select(i => transformFn(i)).ToArray()).ToArray();
        }

        public static Dictionary<int, int> BackIndices(this IEnumerable<int> indices)
        {
            var list = indices.ToArray();

            return indices.Index().ToDictionary(i => list[i], i => i);
        }
    }
}
