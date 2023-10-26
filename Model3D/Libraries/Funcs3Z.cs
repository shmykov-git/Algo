using Model.Extensions;
using System;

namespace Model3D.Libraries
{
    public delegate double Func3Z(double x, double y);

    public static class Funcs3Z
    {
        public static Func3Z Zero = (x, y) => 0;
        public static Func3Z Hyperboloid = (x, y) => x * x - y * y;
        public static Func3Z HyperboloidM = (x, y) => -x * x + y * y;
        public static Func3Z Atan = (x, y) => Math.Atan2(1, Math.Sqrt(x* x + y * y));
        public static Func3Z HyperboloidR(double kX, double kY) => (x, y) => (x/kX).Pow2() - (y/kY).Pow2();
        public static Func3Z Paraboloid = (x, y) => x * x + y * y;
        public static Func3Z ParaboloidM = (x, y) => - x * x - y * y;
        public static Func3Z Cylinder = (x, y) => Math.Sqrt(Math.Abs(1 - y * y));
        public static Func3Z CylinderM = (x, y) => -Math.Sqrt(Math.Abs(1 - y * y));
        public static Func3Z CylinderXM = (x, y) => -Math.Sqrt(Math.Abs(1 - x * x));
        public static Func3Z CylinderR(double r) => (x, y) => Math.Sqrt(Math.Abs(r * r - y * y));
        public static Func3Z CylinderRX(double r) => (x, y) => Math.Sqrt(Math.Abs(r * r - x * x));
        public static Func3Z CylinderXMR(double r) => (x, y) => -Math.Sqrt(Math.Abs(r * r - x * x));
        public static Func3Z Waves = (x, y) => Math.Sin((x * x + y * y).Sqrt() * 40) / 50;
        public static Func3Z Waves2(double t) => (x, y) => Math.Sin((t + (x * x + y * y).Sqrt()) * 40) / 50;
        public static Func3Z Sphere = (x, y) => Math.Sqrt(Math.Abs(1 - x * x - y * y));
        public static Func3Z SphereM = (x, y) => -Math.Sqrt(Math.Abs(1 - x * x - y * y));
        public static Func3Z SphereR(double r) => (x, y) => Math.Sqrt(Math.Abs(r * r - x * x - y * y));
        public static Func3Z SphereRC(double r) => (x, y) => Math.Sqrt(Math.Abs(r * r - x * x - y * y)) - r;
        public static Func3Z SphereMR(double r) => (x, y) => -Math.Sqrt(Math.Abs(r * r - x * x - y * y));
    }
}
