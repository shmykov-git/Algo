using System;

namespace Model.Extensions
{
    public static class DoubleExtensions
    {
        public static double Pow(this double x, double y) => Math.Pow(x, y);
        public static double Sqrt(this double x) => Math.Sqrt(x);
        public static double Abs(this double x) => Math.Abs(x);
        public static double Sign(this double x) => Math.Sign(x);
        public static double Pow2(this double x) => x * x;
        public static double Pow3(this double x) => x * x * x;
    }
}
