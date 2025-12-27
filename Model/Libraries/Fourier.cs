using Model.Extensions;
using Model.Fourier;
using System;
using System.Linq;
using System.Numerics;

namespace Model.Libraries
{
    public static class Fourier
    {
        private static Vector2 ExpC((double r, double i) c, double k) => new Complex(c.r, c.i) * Complex.Pow(new Complex(Math.E, 0), new Complex(0, k * 2 * Math.PI));

        public static Vector2 Exp(double t, params Fr[] members) => members.Select(m => Exp(t, m)).Aggregate((a, b) => a + b);

        private static Vector2 Exp(double t, Fr m)
        {
            if (m.dis.Abs() < Values.Epsilon12)
                return ExpC(m.c, t * m.k);

            var sgn = m.k.Sgn();
            var k = t * m.k;
            var k1 = sgn * (Math.Truncate(k.Abs() / m.dis + 0.5) - 0.5) * m.dis;
            var k2 = k1 + sgn * m.dis;

            var a = ExpC(m.c, k1);
            var b = ExpC(m.c, k2);
            var d = ExpC(m.c, k);

            var r = a + ((d - a) * (b - a) / (b - a).Len2) * (b - a);

            return r;
        }

        public static (double, double) RotateN(double r, double n) => (r * Math.Cos(Math.PI / n), r * Math.Sin(Math.PI / n));

        public static (double, double) RotateN(double n) => (Math.Cos(Math.PI / n), Math.Sin(Math.PI / n));

        public static (double, double) One = (1, 0);
    }
}
