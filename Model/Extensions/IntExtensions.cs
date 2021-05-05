using System;
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

        public static int[][] Transform(this int[][] lists, Func<int, int> transformFn)
        {
            return lists.Select(list => list.Select(i => transformFn(i)).ToArray()).ToArray();
        }
    }
}
