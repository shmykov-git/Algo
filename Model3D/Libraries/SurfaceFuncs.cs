using Aspose.ThreeD.Utilities;
using System;

namespace Model3D.Libraries
{
    public delegate Vector3 SurfaceFunc(double u, double v); 

    public static class SurfaceFuncs
    {
        public static SurfaceFunc Sphere => (double u, double v) => new Vector3(Math.Cos(u) * Math.Sin(v), Math.Sin(u) * Math.Sin(v), Math.Cos(v));
        public static SurfaceFunc Cylinder => (double u, double v) => new Vector3(Math.Cos(u), Math.Sin(u), v);

        public static SurfaceFunc Shamrock => (double u, double v) =>
            new Vector3(
                Math.Cos(u) * Math.Cos(v) + 3 * Math.Cos(u) * (1.5 + Math.Sin(1.5 * u / 2)),
                Math.Sin(u) * Math.Cos(v) + 3 * Math.Sin(u) * (1.5 + Math.Sin(1.5 * u / 2)),
                Math.Sin(v) + 2 * Math.Cos(1.5 * u)
            );
    }
}
