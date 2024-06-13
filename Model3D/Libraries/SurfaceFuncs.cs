using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model.Libraries;
using System;
using Model;
using Vector2 = Model.Vector2;

namespace Model3D.Libraries
{
    public delegate Vector3 SurfaceFunc(double u, double v);

    public static class SurfaceFuncs
    {
        private static SurfaceFunc LineComposeY(Func2 fXY, Func2 fZY, Vector3 shift) => (double u, double v) =>
        {
            var fxy = fXY(u);
            var fzy = fZY(v);

            return new Vector3(fxy.x, fxy.y + fzy.y, fzy.x) + shift;
        };

        public static SurfaceFunc ShapeFuncY(Shape shape, Func2 fn)
        {
            var points = shape.Points2;

            return (double u, double v) =>
            {
                var p = points[(int) u];
                var f = fn(v);

                return new Vector3(p.x * f.x, p.y, p.x * f.y);
            };
        }

        public static SurfaceFunc MagicWand(double w = 0.5, double a = 1.3, double b = 1.3, double c = 2)
        {
            var fn = Funcs.MagicWand(w, a, b, c);

            return (double u, double v) =>
            {
                var fV = fn(v);

                return new Vector3(Math.Cos(u) * fV, Math.Sin(u) * fV, v);
            };
        }

        public static SurfaceFunc CylinderShapeFuncY(Shape shape) => ShapeFuncY(shape, Funcs2.CircleY());

        public static SurfaceFunc APowerB => (a, b) => new Vector3(a, b, (a.Pow(b) - b.Pow(a)) / 10);

        public static SurfaceFunc Polynom4 => (double u, double v) => new Vector3(u, v, 0.25 * (u - 2) * (u + 2) * u * u + v * v + 1);

        public static SurfaceFunc Paraboloid => (double u, double v) => new Vector3(u, v, u * u + v * v);
        public static SurfaceFunc Hyperboloid => (double u, double v) => new Vector3(u, v, u * u - v * v);
        public static SurfaceFunc Wave(double t, double w) => (double u, double v) => new Vector3(u, v, Math.Sin((t + (u * u + v * v).Sqrt()) * w));
        public static SurfaceFunc WaveXY(double t, double w) => (double u, double v) => new Vector3(u, v, Math.Cos((t + 0.5*(u+v)) * w));

        public static SurfaceFunc WaveFi(double t, double w) => (double r, double fi) => new Vector3(r*Math.Sin(fi), r * Math.Cos(fi), Math.Sin((t + r) * w));

        public static SurfaceFunc NormalDistribution(double mu, double sigma, Model.Vector2? shift = null)
        {
            Model.Vector2 zero = (0, 0);
            var fi =  Funcs.ParametricNormDistribution(mu, sigma);

            return (double u, double v) => new Vector3(u, v, 100*fi((new Model.Vector2(u, v) + shift?? zero).Len));
        }

        public static SurfaceFunc Slide(double slope, double height, double width = 0.2, double? hillHeight = null) =>
            LineComposeY(Funcs2.Slide(slope, height, hillHeight), Funcs2.CircleR(width), new Vector3(0, width, 0));

        public static SurfaceFunc Sphere => (double u, double v) => new Vector3(Math.Cos(u) * Math.Sin(v), Math.Sin(u) * Math.Sin(v), Math.Cos(v));
        public static SurfaceFunc Heart
        {
            get
            {
                var heart2 = Funcs2.Heart();

                return (double u, double v) =>
                {
                    var xy = heart2(u);

                    return new Vector3(xy.x*Math.Sin(v), xy.y * Math.Sin(v), 0.1*Math.Cos(v));
                };
            }
        }

        public static SurfaceFunc Torus(double a) => (double u, double v) => new Vector3(Math.Cos(u) * (a + Math.Sin(v)), Math.Sin(u) * (a + Math.Sin(v)), Math.Cos(v));
        public static SurfaceFunc Cylinder => (double u, double v) => new Vector3(Math.Cos(u), Math.Sin(u), v);
        public static SurfaceFunc CylinderY => (double u, double v) => new Vector3(Math.Cos(u), v, Math.Sin(u));
        public static SurfaceFunc CylinderYm => (double u, double v) => new Vector3(Math.Cos(u), v, -Math.Sin(u));
        public static SurfaceFunc CylinderABYm(double a, double b) => (double u, double v) => new Vector3(a * Math.Cos(u), v, -b * Math.Sin(u));
        public static SurfaceFunc Circle => (double u, double v) => v * new Vector3(Math.Cos(u), Math.Sin(u), 0);
        public static SurfaceFunc Cone => (double u, double v) => v * new Vector3(Math.Cos(u), Math.Sin(u), 1);
        public static SurfaceFunc ConeM => (double u, double v) => v * new Vector3(Math.Cos(u), Math.Sin(u), -1);

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

        public static SurfaceFunc MathFlower => (double u, double v) =>
        {
            var len = new Vector2(u, v).Len;
            var f = -10 / len +
                    Math.Sin(len) +
                    Math.Sqrt(200 + len * len + 10 * Math.Sin(u) + 10 * Math.Sin(v)) / 1000;

            return new Vector3(u, v, 3 * f) / 10;
        };
    }
}
