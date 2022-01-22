using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model.Libraries;
using System;
using Model;

namespace Model3D.Libraries
{
    public delegate Vector3 SurfaceFunc(double u, double v);

    public static class SurfaceFuncs
    {
        public static SurfaceFunc APowerB => (a, b) => new Vector3(a, b, (a.Pow(b) - b.Pow(a)) / 10);

        public static SurfaceFunc ParaboloidZ => (double u, double v) => new Vector3(u, v, u * u + v * v);
        public static SurfaceFunc HyperboloidZ => (double u, double v) => new Vector3(u, v, u * u - v * v);

        public static SurfaceFunc NormalDistribution(double mu, double sigma, Model.Vector2? shift = null)
        {
            Model.Vector2 zero = (0, 0);
            var fi =  Funcs.ParametricNormDistribution(mu, sigma);

            return (double u, double v) => new Vector3(u, v, 100*fi((new Model.Vector2(u, v) + shift?? zero).Len));
        }

        public static SurfaceFunc Sphere => (double u, double v) => new Vector3(Math.Cos(u) * Math.Sin(v), Math.Sin(u) * Math.Sin(v), Math.Cos(v));
        public static SurfaceFunc Torus(double a) => (double u, double v) => new Vector3(Math.Cos(u) * (a + Math.Sin(v)), Math.Sin(u) * (a + Math.Sin(v)), Math.Cos(v));
        public static SurfaceFunc Cylinder => (double u, double v) => new Vector3(Math.Cos(u), Math.Sin(u), v);

        public static SurfaceFunc Shamrock => (double u, double v) =>
            new Vector3(
                Math.Cos(u) * Math.Cos(v) + 3 * Math.Cos(u) * (1.5 + Math.Sin(1.5 * u / 2)),
                Math.Sin(u) * Math.Cos(v) + 3 * Math.Sin(u) * (1.5 + Math.Sin(1.5 * u / 2)),
                Math.Sin(v) + 2 * Math.Cos(1.5 * u)
            );

        public static SurfaceFunc Shell => (double u, double v) =>
            new Vector3(
                u * Math.Cos(u) * Math.Cos(v) * (Math.Cos(v) + 1),
                u * Math.Sin(u) * Math.Cos(v) * (Math.Cos(v) + 1),
                u * Math.Sin(v)
            );

        public static SurfaceFunc SeeShell => (double u, double v) =>
            new Vector3(
                u * Math.Cos(u) * Math.Cos(v) * (Math.Cos(v) + 1),
                u * Math.Sin(u) * Math.Cos(v) * (Math.Cos(v) + 1),
                u * Math.Sin(v) - ((u + 3) * Math.PI / 8).Pow2() - 20
            );

        public static SurfaceFunc DiniSurface => (double u, double v) =>
            new Vector3(
                Math.Cos(u) * Math.Sin(v),
                Math.Sin(u) * Math.Sin(v),
                Math.Cos(v) + Math.Log(Math.Tan(v / 2)) + 0.2 * u - 4
            );

        public static SurfaceFunc MobiusStrip => (double u, double v) =>
            new Vector3(
                (1 + v / 2 * Math.Cos(u / 2)) * Math.Cos(u),
                (1 + v / 2 * Math.Cos(u / 2)) * Math.Sin(u),
                v / 2 * Math.Sin(u / 2)
            );
    }
}
