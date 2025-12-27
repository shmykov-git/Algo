using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model3D.Systems
{

    public static class IteratedFunctionSystem
    {
        // http://grafika.me/node/184
        public static Vector2[] BarnsleyFern(int iterationCount)
        {
            var ifs = new (double a, double b, double c, double d, double e, double f, double p)[]
            {
                (0, 0, 0, 0.16, 0, 0, 0.01),
                (0.85, 0.04, -0.04, 0.85, 0, 1.6, 0.85),
                (0.2, -0.26, 0.23, 0.22, 0, 1.6, 0.07),
                (-0.15, 0.28, 0.26, 0.24, 0, 0.44, 0.07)
            };

            return GetPoints2(ifs, (0, 0), iterationCount);
        }

        private static Vector2[] GetPoints2((double a, double b, double c, double d, double e, double f, double p)[] ifs, Vector2 p0, int iterationCount)
        {
            var random = new Random(2);

            Func<Vector2, Vector2>[] funcs = ifs.Select(GetAffin2Fn).ToArray();
            List<Vector2> result = new List<Vector2>();

            Func<Vector2, Vector2> GetRndFunc()
            {
                var r = random.NextDouble();
                var p = 0.0;

                for (var i = 0; i < ifs.Length; i++)
                {
                    p += ifs[i].p;
                    if (r < p)
                        return funcs[i];
                }

                throw new ArgumentException("Incorrect ifs");
            }

            result.Add(p0);
            var p = p0;

            for (var i = 0; i < iterationCount; i++)
            {
                var fn = GetRndFunc();
                p = fn(p);
                result.Add(p);
            }

            return result.ToArray();
        }

        private static Func<Vector2, Vector2> GetAffin2Fn((double a, double b, double c, double d, double e, double f, double p) w)
        {
            var m = new Matrix2
            {
                a00 = w.a,
                a01 = w.b,
                a10 = w.c,
                a11 = w.d
            };
            var l = new Vector2(w.e, w.f);

            return v => m * v + l;
        }

    }
}
