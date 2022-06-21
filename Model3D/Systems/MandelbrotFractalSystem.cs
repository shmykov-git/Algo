using System;
using System.Collections.Generic;
using System.Numerics;
using Model.Extensions;
using Vector2 = Model.Vector2;

namespace Model3D.Systems
{
    public static class MandelbrotFractalSystem
    {
        private static int MandelbrotDistance(Complex power, Complex c, int maxIterations)
        {
            Func<Complex, Complex> GetFn(Complex power, Complex c) => z => Complex.Pow(z, power)  + c;
            bool IsOutside(Complex z) => z.Real * z.Real + z.Imaginary * z.Imaginary > 4;

            var fn = GetFn(power, c);

            var z = c;
            for (var i = 1; i < maxIterations; i++)
            {
                if (IsOutside(z))
                    return i;

                z = fn(z);
            }

            return 0;
        }


        private static (Vector2 a, Vector2 b) FindBounds(Complex power, Complex c0, double precision, int maxIterations)
        {
            var step = new Complex(precision, 0);
            var c = c0;
            while (MandelbrotDistance(power, c, maxIterations) == 0)
                c += step;

            return (c-step, c);
        }

        public static Vector2[] GetPoints((double re, double im) power, double precision, int maxIterations = 1000, double insideCoff = 0.99,
            int limit = 100000) => GetPoints(new Complex(power.re, power.im), precision, maxIterations, insideCoff, limit);

        public static Vector2[] GetPoints(Complex power, double precision, int maxIterations = 1000, double insideCoff = 0.99, int limit = 100000)
        {
            var precision2 = precision.Pow2();
            var v0 = FindBounds(power, new Complex(0, 0), precision, maxIterations);

            bool IsInside(Vector2 v) => MandelbrotDistance(power, v.ToZ(), maxIterations) == 0;

            (Vector2 a, Vector2 b) NextPoint((Vector2 a, Vector2 b) v)
            {
                var dir = (v.b - v.a).Normal.ToLen(precision);
                var c = v.a + dir;
                var d = v.b + dir;
                var isCInside = IsInside(c);
                var isDInside = IsInside(d);

                if (isCInside)
                {
                    if (isDInside)
                        return (d, v.b);
                    else
                        return (c, d);
                }
                else
                {
                    //if (isDInside)
                    //    throw new ApplicationException("Loop");
                    //else
                        return (v.a, c);
                }
            }

            var res = new List<Vector2>();

            var v = NextPoint(v0);
            res.Add(v.b);

            do
            {
                v = NextPoint(v);
                var p = v.a + insideCoff * (v.b - v.a);
                
                res.Add(p);

                if (limit-- == 0)
                    break;

            } while ((v.b - v0.b).Len2 > precision2);

            return res.ToArray();
        }
    }
}
