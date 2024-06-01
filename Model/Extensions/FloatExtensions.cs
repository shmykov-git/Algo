using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model.Libraries;

namespace Model.Extensions
{
    public static class FloatExtensions
    {
        public static float Round(this float x, int e) => MathF.Round(x * 10.0f.Pow(e)) / 10.0f.Pow(e);
        public static float Pow(this float x, float y) => MathF.Pow(x, y);
        public static float Sqrt(this float x) => MathF.Sqrt(x);
        public static float Abs(this float x) => MathF.Abs(x);
        public static float Sign(this float x) => MathF.Sign(x);
        public static int Sgn(this float x) => x < 0 ? -1 : 1;
        public static int SgnZ(this float x, float epsilon = Values.Epsilon9f) => x > epsilon ? 1 : -1;
        public static float Pow2(this float x) => x * x;
        public static float Pow3(this float x) => x * x * x;
        public static float Pow4(this float x) => x * x * x * x;
        public static float Pow6(this float x) => x.Pow3().Pow2();
        public static float Pow12(this float x) => x.Pow6().Pow2();

        public static float Dispersion(this IEnumerable<float> values, float? avgValue = null) => MathF.Sqrt(values.DispersionPow2(avgValue));

        public static float DispersionPow2(this IEnumerable<float> values, float? avgValue = null)
        {
            var avg = avgValue ?? values.Average();
            var s2 = values.Select(a => (a - avg).Pow2()).Average();

            return s2;
        }

        public static (float s, float avg) DispersionWithAvg(this IEnumerable<float> values)
        {
            var (s2, avg) = values.DispersionPow2WithAvg();

            return (MathF.Sqrt(s2), avg);
        }

        public static (float s2, float avg) DispersionPow2WithAvg(this IEnumerable<float> values)
        {
            var avg = values.Average();
            var s2 = values.Select(a => (a - avg).Pow2()).Average();

            return (s2, avg);
        }

        [DebuggerStepThrough]
        public static void BreakNaN(this float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                Debugger.Break();
        }
    }
}
