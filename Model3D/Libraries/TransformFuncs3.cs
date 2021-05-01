using Aspose.ThreeD.Utilities;
using System;
using Model.Extensions;
using Model.Libraries;

namespace Model3D.Libraries
{
    public delegate Vector3 TransformFunc3(Vector3 v);

    public static class TransformFuncs3
    {
        private static double AngleFn(double x, double y) => Math.Atan2(y, x);

        public static TransformFunc3 LikeSphere(Func2 xyFn, Func2 zFn)
        {
            return v =>
            {
                var q = AngleFn(v.z, (v.x * v.x + v.y * v.y).Sqrt());
                var fi = AngleFn(v.x, v.y);

                var vZ = zFn(q);
                var vXY = xyFn(fi);

                return new Vector3(vXY.X * vZ.X, vXY.Y * vZ.X, vZ.Y);
            };

        }

        public static TransformFunc3 Heart() => LikeSphere(Funcs2.Circle(), Funcs2.Heart());
    }
}
