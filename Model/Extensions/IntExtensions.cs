using System;
using System.Collections.Generic;
using System.Linq;
using Model.Hashes;

namespace Model.Extensions
{
    public static class IntExtensions
    {
        public static int Abs(this int a) => Math.Abs(a);
        public static int Sgn(this int x) => x < 0 ? -1 : 1;

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

        public static IEnumerable<int[]> ReverseConvexes(this IEnumerable<int[]> lists, bool needReverse = true)
        {
            return needReverse ? lists.Select(list => list.Reverse().ToArray()).ToArray() : lists;
        }

        public static int[][] CleanBi(this IEnumerable<int[]> lists)
        {
            return lists.Where(list=>!list.Contains(-1)).ToArray();
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

        public static int[] NormalizeConvex(this int[] c)
        {
            var k = 0;
            var min = c[0];

            for (var j = 0; j < c.Length; j++)
            {
                if (c[j] < min)
                {
                    min = c[j];
                    k = j;
                }
            }

            return c.Index().Select(i => c[(i + k) % c.Length]).ToArray();
        }

        public static Hashed<int[]> HashedConvex(this int[] c)
        {
            return new Hashed<int[]>(c.NormalizeConvex(), Hash.Get, (a, b) => a.Length == b.Length && a.Index().All(i => a[i] == b[i]));
        }

        public static int[] Line2(this int[] c, int k)
        {
            var i = 0;
            while (c[i] != k && i < c.Length)
                i++;

            if (i == c.Length)
                return new int[0];

            return new int[] { c[(i + 1) % c.Length], c[(i - 1 + c.Length) % c.Length] };
        }

        public static int[] Plane3(this int[] c, int k)
        {
            var i = 0;
            while (c[i] != k && i < c.Length)
                i++;

            if (i == c.Length)
                return new int[0];

            return new int[] { c[(i - 1 + c.Length) % c.Length], k, c[(i + 1) % c.Length] };
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
