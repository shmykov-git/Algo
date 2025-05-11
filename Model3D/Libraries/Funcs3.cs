using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System;
using System.Diagnostics;
using Vector2 = Model.Vector2;

namespace Model3D.Libraries
{
    public delegate Vector3 Func3(double t);
    public delegate double Func_3(Vector3 v);

    public static class Funcs3
    {
        private static Func2 heartFn = Funcs2.Heart();

        public static Func3 SphereSpiral(double a = 1, double alfa = 0) => t =>
        {
            return new Vector3(Math.Cos(alfa + a * t) * Math.Sin(t / 2), Math.Sin(alfa + a * t) * Math.Sin(t / 2), Math.Cos(t / 2));
        };


        public static Func3 RootPolinomY(double mult, double[] coefs) => t => new Vector3(0, mult * Polinom.RootFn(t, coefs), t);
        public static Func3 ParabolaY = t => new Vector3(0, t * t, t);
        public static Func3 Spiral(double a = 1) => t => new Vector3(Math.Sin(t), Math.Cos(t), t * a / (2 * Math.PI));
        public static Func3 SpiralHeart = t => heartFn(t).ToV3()*5 + new Vector3(0,0, t / (2 * Math.PI));
        public static Func3 Spiral4 = t => new Vector3(Math.Sin(4 * t), Math.Cos(t), Math.Sin(t));
        public static Func3 Flower(int n) => t => new Vector3(Math.Sin(t), Math.Cos(t), 0) * Math.Sin(n * t / 2).Abs();
        public static Func3 Cloud(double a, double b) => t => new Vector3((a + b) * Math.Cos(t) - a * t * Math.Cos(a + b) / a, (a + b) * Math.Sin(t) - a * t * Math.Sin(a + b) * a, 0);
    }
}
