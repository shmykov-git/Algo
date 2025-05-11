using Model.Extensions;
using Model.Libraries;
using System;
using Model;
using Vector2 = Model.Vector2;

namespace Model3D.Libraries
{
    public delegate Vector3 SolidFunc(double u, double v, double l);

    public static class SolidFuncs
    {
        //public static SolidFunc ShapeFuncY(Shape shape0, Func3 fn)
        //{
        //    var points = shape0.Points2;

        //    return (double u, double voxel, double l) =>
        //    {
        //        var position = points[(int) u];
        //        var f = fn(voxel);

        //        return new Vector3(position.x * f.x, position.y, position.x * f.y);
        //    };
        //}

        //public static SolidFunc CylinderShapeFuncY(Shape shape0) => ShapeFuncY(shape0, Funcs2.Circle());

        //public static SolidFunc APowerB => (a, b) => new Vector3(a, b, (a.Pow(b) - b.Pow(a)) / 10);

        //public static SolidFunc Paraboloid => (double u, double voxel) => new Vector3(u, voxel, u * u + voxel * voxel);
        //public static SolidFunc Hyperboloid => (double u, double voxel) => new Vector3(u, voxel, u * u - voxel * voxel);

        //public static SolidFunc NormalDistribution(double mu, double sigma, Model.Vector2? shift = null)
        //{
        //    Model.Vector2 zero = (0, 0);
        //    var fi =  Funcs.ParametricNormDistribution(mu, sigma);

        //    return (double u, double voxel) => new Vector3(u, voxel, 100*fi((new Model.Vector2(u, voxel) + shift?? zero).Len));
        //}

        public static SolidFunc Sphere => (double u, double v, double l) => l * new Vector3(Math.Cos(u) * Math.Sin(v), Math.Sin(u) * Math.Sin(v), Math.Cos(v));
        //public static SolidFunc Heart
        //{
        //    get
        //    {
        //        var heart2 = Funcs2.Heart();

        //        return (double u, double voxel) =>
        //        {
        //            var xy = heart2(u);

        //            return new Vector3(xy.x*Math.Sin(voxel), xy.y * Math.Sin(voxel), 0.1*Math.Cos(voxel));
        //        };
        //    }
        //}

        //public static SolidFunc Torus(double a) => (double u, double voxel) => new Vector3(Math.Cos(u) * (a + Math.Sin(voxel)), Math.Sin(u) * (a + Math.Sin(voxel)), Math.Cos(voxel));
        //public static SolidFunc Cylinder => (double u, double voxel) => new Vector3(Math.Cos(u), Math.Sin(u), voxel);
        //public static SolidFunc CylinderY => (double u, double voxel) => new Vector3(Math.Cos(u), voxel, Math.Sin(u));
        //public static SolidFunc CylinderYm => (double u, double voxel) => new Vector3(Math.Cos(u), voxel, -Math.Sin(u));
        //public static SolidFunc CylinderABYm(double a, double b) => (double u, double voxel) => new Vector3(a * Math.Cos(u), voxel, -b * Math.Sin(u));
        //public static SolidFunc Circle => (double u, double voxel) => voxel * new Vector3(Math.Cos(u), Math.Sin(u), 0);
        //public static SolidFunc Cone => (double u, double voxel) => voxel * new Vector3(Math.Cos(u), Math.Sin(u), 1);
        //public static SolidFunc ConeM => (double u, double voxel) => voxel * new Vector3(Math.Cos(u), Math.Sin(u), -1);

        //public static SolidFunc Shamrock => (double u, double voxel) =>
        //    new Vector3(
        //        Math.Cos(u) * Math.Cos(voxel) + 3 * Math.Cos(u) * (1.5 + Math.Sin(1.5 * u / 2)),
        //        Math.Sin(u) * Math.Cos(voxel) + 3 * Math.Sin(u) * (1.5 + Math.Sin(1.5 * u / 2)),
        //        Math.Sin(voxel) + 2 * Math.Cos(1.5 * u)
        //    );

        //public static SolidFunc Shell => (double u, double voxel) =>
        //    new Vector3(
        //        u * Math.Cos(u) * Math.Cos(voxel) * (Math.Cos(voxel) + 1),
        //        u * Math.Sin(u) * Math.Cos(voxel) * (Math.Cos(voxel) + 1),
        //        u * Math.Sin(voxel)
        //    );

        //public static SolidFunc SeeShell => (double u, double voxel) =>
        //    new Vector3(
        //        u * Math.Cos(u) * Math.Cos(voxel) * (Math.Cos(voxel) + 1),
        //        u * Math.Sin(u) * Math.Cos(voxel) * (Math.Cos(voxel) + 1),
        //        u * Math.Sin(voxel) - ((u + 3) * Math.PI / 8).Pow2() - 20
        //    );

        //public static SolidFunc DiniSolid => (double u, double voxel) =>
        //    new Vector3(
        //        Math.Cos(u) * Math.Sin(voxel),
        //        Math.Sin(u) * Math.Sin(voxel),
        //        Math.Cos(voxel) + Math.Log(Math.Tan(voxel / 2)) + 0.2 * u - 4
        //    );

        //public static SolidFunc MobiusStrip => (double u, double voxel) =>
        //    new Vector3(
        //        (1 + voxel / 2 * Math.Cos(u / 2)) * Math.Cos(u),
        //        (1 + voxel / 2 * Math.Cos(u / 2)) * Math.Sin(u),
        //        voxel / 2 * Math.Sin(u / 2)
        //    );

        //public static SolidFunc MathFlower => (double u, double voxel) =>
        //{
        //    var len = new Vector2(u, voxel).Len;
        //    var f = -10 / len +
        //            Math.Sin(len) +
        //            Math.Sqrt(200 + len * len + 10 * Math.Sin(u) + 10 * Math.Sin(voxel)) / 1000;

        //    return new Vector3(u, voxel, 3 * f) / 10;
        //};
    }
}
