using System;

namespace Model.Extensions
{
    public static class DoubleExtensions
    {
        public static double Round(this double x, int e) => Math.Round(x * 10.0.Pow(e)) / 10.0.Pow(e);
        public static double Pow(this double x, double y) => Math.Pow(x, y);
        public static double Sqrt(this double x) => Math.Sqrt(x);
        public static double Abs(this double x) => Math.Abs(x);
        public static double Sign(this double x) => Math.Sign(x);
        public static int Sgn(this double x) => x < 0 ? -1 : 1;
        public static double Pow2(this double x) => x * x;
        public static double Pow3(this double x) => x * x * x;
        public static double Pow6(this double x) => x.Pow3().Pow2();
        public static double Pow12(this double x) => x.Pow6().Pow2();
    }
}
