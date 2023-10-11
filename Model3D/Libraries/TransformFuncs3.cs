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

        private static TransformFunc3 PullOnSphere(Func2 xyFn, Func2 zFn)
        {
            return v =>
            {
                var q = AngleFn(v.z, (v.x * v.x + v.y * v.y).Sqrt());
                var fi = AngleFn(v.x, v.y);

                var vZ = zFn(q);
                var vXY = xyFn(fi);

                return new Vector3(vXY.x * vZ.x, vXY.y * vZ.x, vZ.y);
            };
        }

        private static TransformFunc3 WrapSphere(Func2 xyFn, Func2 zFn)
        {
            return v =>
            {
                var vZ = zFn(v.y);
                var vXY = xyFn(v.x);

                return new Vector3(vXY.x * vZ.x, vXY.y * vZ.x, vZ.y);
            };
        }

        private static TransformFunc3 WrapSphereZ(Func2 xyFn, Func2 zFn)
        {
            return v =>
            {
                var vZ = zFn(v.y);
                var vXY = xyFn(v.x);

                return new Vector3(vXY.x * vZ.x, vXY.y * vZ.x, vZ.y) * (1 + v.z);
            };
        }

        private static TransformFunc3 WrapCylinderZObsolet(Func2 xyFn, Func2 zFn)
        {
            return v =>
            {
                var vZ = zFn(v.y);
                var vXY = xyFn(v.x);

                return new Vector3((1 + v.z) * vXY.x * vZ.x, (1 + v.z) * vXY.y * vZ.x, vZ.y);
            };
        }

        private static TransformFunc3 WrapCylinderZ(double r, Func2 xyFn, Func2 zFn)
        {
            return v =>
            {
                var vZ = zFn(v.y);
                var vXY = xyFn(v.x);

                return new Vector3((r + v.z) * vXY.x * vZ.x, vZ.y, (r + v.z) * vXY.y * vZ.x);
            };
        }
        private static TransformFunc3 WrapCylinderZ(Func1 rf, Func2 xyFn, Func2 zFn)
        {
            return v =>
            {
                var vZ = zFn(v.y);
                var vXY = xyFn(v.x);
                var r = rf(vXY.Len);

                return new Vector3((r + v.z) * vXY.x * vZ.x, vZ.y, (r + v.z) * vXY.y * vZ.x);
            };
        }

        public static TransformFunc3 Heart => PullOnSphere(Funcs2.Circle(), Funcs2.Heart());
        public static TransformFunc3 Sphere => PullOnSphere(Funcs2.Circle(), Funcs2.Circle());
        public static TransformFunc3 HeartWrap => WrapSphere(Funcs2.Circle(), Funcs2.Heart());
        public static TransformFunc3 HeartWrapZ => WrapSphereZ(Funcs2.Circle(), Funcs2.Heart());
        public static TransformFunc3 SphereWrap => WrapSphere(Funcs2.Circle(), Funcs2.Circle());
        public static TransformFunc3 SphereWrapZ => WrapSphereZ(Funcs2.Circle(), Funcs2.Circle());
        public static TransformFunc3 CylinderWrap => WrapSphere(Funcs2.Circle(), Funcs2.VerticalLine());
        public static TransformFunc3 CylinderWrapZ => WrapCylinderZObsolet(Funcs2.Circle(), Funcs2.VerticalLine());
        public static TransformFunc3 CylinderWrapZR(double r) => WrapCylinderZ(r, Funcs2.Circle(), Funcs2.VerticalLine());
        public static TransformFunc3 CylinderWrapZR(Func1 rF) => WrapCylinderZ(rF, Funcs2.Circle(), Funcs2.VerticalLine());
        public static TransformFunc3 Flower(double a, double b, int n) => WrapSphereZ(Funcs2.Flower(n, b), Funcs2.Torus(a));
        public static TransformFunc3 Torus(double a) => WrapSphereZ(Funcs2.Circle(), Funcs2.Torus(a));

        public static TransformFunc3 RotateX(double turn = 1, double fluency = 1) => v =>
        {
            var fi = turn * Math.Atan2(v.y.Abs(), fluency * v.x);
            var q = Quaternion.FromAngleAxis(fi, Vector3.XAxis);

            return q * v;
        };
    }
}
