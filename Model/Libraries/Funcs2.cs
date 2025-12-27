using Model.Bezier;
using Model.Extensions;
using System;

namespace Model.Libraries
{
    public delegate Vector2 Func2(double v);

    public static class Funcs2
    {
        public static Func2 HorizontalLine() => t => (t, 1);
        public static Func2 VerticalLine() => t => (1, t);
        public static Func2 CircleY() => t => (Math.Sin(t), Math.Cos(t));
        public static Func2 Circle() => t => (Math.Cos(t), Math.Sin(t));
        public static Func2 CircleR(double r) => t => (r * Math.Cos(t), r * Math.Sin(t));
        public static Func2 Torus(double centerRadius) => t => new Vector2(centerRadius + Math.Cos(t), Math.Sin(t)) / (centerRadius + 1);
        public static Func2 Parabola() => t => (t, t * t);

        public static Func2 Heart()
        {
            Func2 heartFn = t =>
                (
                    16 * Math.Sin(t).Pow3(),
                    13 * Math.Cos(t) - 5 * Math.Cos(2 * t) - 2 * Math.Cos(3 * t) - Math.Cos(4 * t)
                );

            return v => heartFn(v).Scale((1.0 / 64, 1.0 / 48)) + new Vector2(0, 0.1);
        }

        public static Func2 Flower(int n, double a) => t => (a * new Vector2(Math.Sin(t), Math.Cos(t)) * Math.Sin(n * t / 2).Abs() + new Vector2(Math.Sin(t), Math.Cos(t))) / (a + 1);

        public static Func2 Bz(Vector2[][] ps, bool closed = false) => ps.ToBz(closed);

        public static Func2 Slide(double slope, double height = 0.5, double? hillHeight = null, double hillCenter = 0.625)
        {
            var sl = 1 - slope;

            Vector2[][] bps = [
                hillHeight == null
                    ? [(0, height), (0, height * sl), (sl, 0)]
                    : [(0, height), (0, height * sl), (hillCenter, hillHeight.Value), (sl, 0)],
                [(1, 0)],
            ];

            return bps.ToBz();
        }
    }
}
