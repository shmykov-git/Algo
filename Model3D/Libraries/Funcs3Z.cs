using Model.Extensions;
using System;

namespace Model3D.Libraries
{
    public delegate double Func3Z(double x, double y);

    public static class Funcs3Z
    {
        public static Func3Z Zero = (x, y) => 0;
        public static Func3Z Hyperboloid = (x, y) => x * x - y * y;
        public static Func3Z Paraboloid = (x, y) => x * x + y * y;
        public static Func3Z Waves = (x, y) => Math.Sin((x * x + y * y).Sqrt() *40)/50;
        public static Func3Z Sphere = (x, y) => Math.Sqrt(Math.Abs(1 - x * x - y * y));
        public static Func3Z SphereR(double r) => (x, y) => Math.Sqrt(Math.Abs(r * r - x * x - y * y));
    }
}
