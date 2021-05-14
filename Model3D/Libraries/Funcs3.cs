using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System;

namespace Model3D.Libraries
{
    public delegate Vector3 Func3(double t);

    public static class Funcs3
    {
        private static Func2 heartFn = Funcs2.Heart();

        public static Func3 Spiral = t => new Vector3(Math.Sin(t), Math.Cos(t), t / (2 * Math.PI));
        public static Func3 SpiralHeart = t => heartFn(t).ToV3()*5 + new Vector3(0,0, t / (2 * Math.PI));
        public static Func3 Spiral4 = t => new Vector3(Math.Sin(4 * t), Math.Cos(t), Math.Sin(t));
        public static Func3 Flower(int n) => t => new Vector3(Math.Sin(t), Math.Cos(t), 0) * Math.Sin(n * t / 2).Abs();
        public static Func3 Cloud(double a, double b) => t => new Vector3((a + b) * Math.Cos(t) - a * t * Math.Cos(a + b) / a, (a + b) * Math.Sin(t) - a * t * Math.Sin(a + b) * a, 0);
    }
}
