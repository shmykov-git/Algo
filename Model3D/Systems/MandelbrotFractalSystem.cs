using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Model3D.Systems
{
    public static class MandelbrotFractalSystem
    {
        public static object Debuger { get; private set; }

        private static int MandelbrotDistance(Complex c, int maxIterations)
        {
            Func<Complex, Complex> GetFn(Complex c) => z => z * z + c;
            bool IsOutside(Complex z) => z.Real * z.Real + z.Imaginary * z.Imaginary > 4;

            var fn = GetFn(c);

            var z = c;
            for (var i = 1; i < maxIterations; i++)
            {
                if (IsOutside(z))
                    return i;

                z = fn(z);
            }

            return 0;
        }


        private static (Model.Vector2 a, Model.Vector2 b) FindBounds(Complex c0, double precision, int maxIterations)
        {
            var step = new Complex(precision, 0);
            var c = c0;
            while (MandelbrotDistance(c, maxIterations) == 0)
                c += step;

            return (c-step, c);
        }

        public static Model.Vector2[] GetPoints(double precision, int maxIterations = 100, int limit = 100000)
        {
            var v0 = FindBounds(new Complex(0, 0), precision, maxIterations);

            bool IsInside(Model.Vector2 v) => MandelbrotDistance(v.ToZ(), maxIterations) == 0;

            (Model.Vector2 a, Model.Vector2 b) NextPoint((Model.Vector2 a, Model.Vector2 b) v)
            {
                var dir = (v.a - v.b).Normal.ToLen(precision);
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

            var res = new List<Model.Vector2>();

            var v = NextPoint(v0);
            res.Add(v.b);

            do
            {
                v = NextPoint(v);
                res.Add(v.b);

                if (limit-- == 0)
                    break;

            } while ((v.b - v0.b).Len2 > precision * precision);

            return res.ToArray();
        }
    }
}
