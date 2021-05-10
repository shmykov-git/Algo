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

        public static int[] ShiftConvex(this int[] c, int value)
        {
            var cc = c.ToList();
            var k = cc.IndexOf(value);

            return c.Index().Select(i => c[(i + k) % c.Length]).ToArray();
        }

        public static int[] JoinConvexes(this int[] a, int[] b)
        {
            var c = a.Intersect(b).ToArray();
            if (c.Length != 2)
                throw new JoinConvexesException();

            var aa = a.ShiftConvex(c[1]);
            var bb = b.ShiftConvex(c[0]);

            return aa.Concat(bb.Skip(1).Take(bb.Length - 2)).ToArray();
        }
    }
}
