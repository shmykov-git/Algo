using Model.Extensions;
using System;

namespace Model3D.Tools.Model
{
    public static class ColorMasks
    {
        public static Func<double, double, Func<double, double, bool>> Ellipse(double a = 1, double b = 1) =>
            (m, n) => (u, v) => ((u - m / 2) / m / a).Pow2() + ((v - n / 2) / n / b).Pow2() < 0.25;

        public static Func<double, double, Func<double, double, bool>> Ellipse4(double a = 1, double b = 1) =>
            (m, n) => (u, v) => ((u - m / 2) / m / a).Pow2().Pow2() + ((v - n / 2) / n / b).Pow2().Pow2() < 0.25 * 0.25;
    }
}