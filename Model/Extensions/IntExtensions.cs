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

        //var from = 0;
        //var n = 21;
        //return (n-3).SelectSquarePoints((i, x, y) =>
        //{
        //    var ps = (i+3+from).SelectCirclePoints((i, x, y) => new Vector3(x, y, 0)).ToArray();
        //    var s = new Shape
        //    {
        //        Points3 = ps,
        //        Convexes = new[] { ps.Index().ToArray() }
        //    };

        //    //var s = new Shape { Points = ss.Points, Convexes = ss.Convexes.Take(1).ToArray() }.Normalize(false, false).Centered();
        //    var ts = s.TriangulateByFour();

        //    return (ts.ApplyColor(Color.Blue) + ts.ToMetaShape3(3, 3, Color.Red, Color.Green) + /*s.ToNumSpots3() + */vectorizer.GetTextLine(ts.Convexes.Length.ToString()).Centered().Mult(0.5).MoveY(1.6)).Centered().Move(4*x, 4*y, 0);
        //}).ToArray().ToSingleShape().ToMotion();

        public static IEnumerable<int[]> TriangulateByFour(this IEnumerable<int[]> lists) => lists.TriangulateByFourSplitted().SelectMany(v => v);
        public static IEnumerable<int[][]> TriangulateByFourSplitted(this IEnumerable<int[]> lists)
        {
            int[][] Split(int[] c)
            {
                if (c.Length > 6)
                    throw new NotImplementedException("Convex len > 6 is not implemented to triangulate");

                return c.Length <= 3
                    ? new[] { c }
                    : ((c.Length - 2) / 3 + 1)
                        .SelectRange(m => 3 * m)
                        .Select(m => (m, i: c[m % c.Length], j: c[(m + 1) % c.Length], k: c[(m + 2) % c.Length], l: c[(m + 3) % c.Length]))
                        .SelectMany(v => (c.Length - v.m - 1) switch
                        {
                            0 => new int[0][],
                            1 => new[] { new[] { v.i, v.j, v.k } },
                            _ => new[] { new[] { v.i, v.j, v.k }, new[] { v.k, v.l, v.i } }
                        }).ToArray();
            }

            return lists.Select(Split);
        }

        public static IEnumerable<int[]> ReverseConvexes(this IEnumerable<int[]> lists, bool needReverse = true)
        {
            return needReverse ? lists.Select(list => list.Reverse().ToArray()).ToArray() : lists;
        }

        public static int[][] CleanBi(this IEnumerable<int[]> convexes, bool cleanConvexes = true)
        {
            return cleanConvexes
                ? convexes.Where(convex => !convex.Contains(-1)).ToArray()
                : convexes.Select(list => list.Where(v => v != -1).ToArray()).ToArray();
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
