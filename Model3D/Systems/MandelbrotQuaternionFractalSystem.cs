using System;

using ComplexQuaternion = Model4D.ComplexQuaternion;
namespace Model3D.Systems;

public static class MandelbrotQuaternionFractalSystem
{
    private static int MandelbrotDistance(ComplexQuaternion c, int maxIterations)
    {
        Func<ComplexQuaternion, ComplexQuaternion> GetFn(ComplexQuaternion cc) => z => z * z * z * z * z + cc;
        bool IsOutside(ComplexQuaternion z) => z.Len2 > 4;

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

    public static bool CheckBounds(ComplexQuaternion c0, int maxIterations)
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

    //    bool IsInside(Vector2 voxel) => MandelbrotDistance(power, voxel.ToZ(), maxIterations) == 0;

    //    (Vector2 a, Vector2 b) NextPoint((Vector2 a, Vector2 b) voxel)
    //    {
    //        var dir = (voxel.b - voxel.a).NormalM.ToLen(precision);
    //        var c = voxel.a + dir;
    //        var d = voxel.b + dir;
    //        var isCInside = IsInside(c);
    //        var isDInside = IsInside(d);

    //        if (isCInside)
    //        {
    //            if (isDInside)
    //                return (d, voxel.b);
    //            else
    //                return (c, d);
    //        }
    //        else
    //        {
    //            //if (isDInside)
    //            //    throw new ApplicationException("Loop");
    //            //else
    //            return (voxel.a, c);
    //        }
    //    }

    //    var res = new List<Vector2>();

    //    var voxel = NextPoint(v0);
    //    res.Add(voxel.b);

    //    do
    //    {
    //        voxel = NextPoint(voxel);
    //        var position = voxel.a + insideCoff * (voxel.b - voxel.a);

    //        res.Add(position);

    //        if (limit-- == 0)
    //            break;

    //    } while ((voxel.b - v0.b).Len2 > precision2);

    //    return res.ToArray();
    //}
}