using System;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Libraries;

namespace Model3D.Extensions
{
    public static class SurfaceFuncExtensions
    {
        public static Func<double, double, Vector3> Rotate(this Func<double, double, Vector3> fn, double zX, double zY, double zZ) => (u, v) => Rotate((u, v) => fn(u, v), new Vector3(zX, zY, zZ))(u, v);
        public static SurfaceFunc Rotate(this SurfaceFunc fn, double zX, double zY, double zZ) => Rotate(fn, new Vector3(zX, zY, zZ));

        public static SurfaceFunc Rotate(this SurfaceFunc fn, Vector3 zAx)
        {
            var q = Quaternion.FromRotation(Vector3.ZAxis, zAx.Normalize());

            return (u, v) => q * fn(u, v);
        }

        public static SurfaceFunc Move(this SurfaceFunc fn, double x, double y, double z) => Move(fn, new Vector3(x, y, z));
        public static SurfaceFunc Move(this SurfaceFunc fn, Vector3 move)
        {
            return (u, v) => fn(u, v) + move;
        }

        public static SurfaceFunc Join(this SurfaceFunc fnA, SurfaceFunc fnB)
        {
            return (u, v) => fnA(u, v) + fnB(u, v);
        }

        public static SurfaceFunc Mult(this SurfaceFunc fn, double mult)
        {
            return (u, v) => fn(u, v) * mult;
        }
    }
}
