using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model.Libraries;

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
        public static int SgnZ(this double x, double epsilon = Values.Epsilon9) => x > epsilon ? 1 : -1;
        public static double Pow2(this double x) => x * x;
        public static double Pow3(this double x) => x * x * x;
        public static double Pow4(this double x) => x * x * x * x;
        public static double Pow6(this double x) => x.Pow3().Pow2();
        public static double Pow12(this double x) => x.Pow6().Pow2();

        public static double Dispersion(this IEnumerable<double> values, double? avgValue = null) => Math.Sqrt(values.DispersionPow2(avgValue));

        public static double DispersionPow2(this IEnumerable<double> values, double? avgValue = null)
        {
            var avg = avgValue ?? values.Average();
            var s2 = values.Select(a => (a - avg).Pow2()).Average();

            return s2;
        }

        public static (double s, double avg) DispersionWithAvg(this IEnumerable<double> values)
        {
            var (s2, avg) = values.DispersionPow2WithAvg();

            return (Math.Sqrt(s2), avg);
        }

        public static (double s2, double avg) DispersionPow2WithAvg(this IEnumerable<double> values)
        {
            var avg = values.Average();
            var s2 = values.Select(a => (a - avg).Pow2()).Average();

            return (s2, avg);
        }

        [DebuggerStepThrough]
        public static void BreakNaN(this double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                Debugger.Break();
        }
    }
}
