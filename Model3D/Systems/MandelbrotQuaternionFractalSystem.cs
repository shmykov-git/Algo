using System;
using System.Numerics;

using Quaternion = Model4D.Quaternion;
namespace Model3D.Systems;

public static class MandelbrotQuaternionFractalSystem
{
    private static int MandelbrotDistance(Quaternion c, int maxIterations)
    {
        Func<Quaternion, Quaternion> GetFn(Quaternion cc) => z => z*z*z*z*z + cc;
        bool IsOutside(Quaternion z) => z.Len2 > 4;

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

    public static bool CheckBounds(Quaternion c0, int maxIterations)
    {
        return MandelbrotDistance(c0, maxIterations) == 0;
    }

    //private static (Vector2 a, Vector2 b) FindBounds(Quaternion c0, double precision, int maxIterations)
    //{
    //    var step = new Complex(precision, 0);
    //    var c = c0;
    //    while (MandelbrotDistance(c, maxIterations) == 0)
    //        c += step;

    //    return (c - step, c);
    //}

    //public static Vector2[] GetPoints((double re, double im) power, double precision, int maxIterations = 1000, double insideCoff = 0.99,
    //    int limit = 100000) => GetPoints(new Complex(power.re, power.im), precision, maxIterations, insideCoff, limit);

    //public static Vector2[] GetPoints(Complex power, double precision, int maxIterations = 1000, double insideCoff = 0.99, int limit = 100000)
    //{
    //    var precision2 = precision.Pow2();
    //    var v0 = FindBounds(power, new Complex(0, 0), precision, maxIterations);

    //    bool IsInside(Vector2 v) => MandelbrotDistance(power, v.ToZ(), maxIterations) == 0;

    //    (Vector2 a, Vector2 b) NextPoint((Vector2 a, Vector2 b) v)
    //    {
    //        var dir = (v.b - v.a).NormalM.ToLen(precision);
    //        var c = v.a + dir;
    //        var d = v.b + dir;
    //        var isCInside = IsInside(c);
    //        var isDInside = IsInside(d);

    //        if (isCInside)
    //        {
    //            if (isDInside)
    //                return (d, v.b);
    //            else
    //                return (c, d);
    //        }
    //        else
    //        {
    //            //if (isDInside)
    //            //    throw new ApplicationException("Loop");
    //            //else
    //            return (v.a, c);
    //        }
    //    }

    //    var res = new List<Vector2>();

    //    var v = NextPoint(v0);
    //    res.Add(v.b);

    //    do
    //    {
    //        v = NextPoint(v);
    //        var p = v.a + insideCoff * (v.b - v.a);

    //        res.Add(p);

    //        if (limit-- == 0)
    //            break;

    //    } while ((v.b - v0.b).Len2 > precision2);

    //    return res.ToArray();
    //}
}