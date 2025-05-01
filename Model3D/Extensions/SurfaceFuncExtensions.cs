using System;
using Model3D.AsposeModel;
using Model.Extensions;
using Model3D.Libraries;

namespace Model3D.Extensions
{
    public static class SurfaceFuncExtensions
    {
        public static Func<double, double, Vector3> ToFunc(this SurfaceFunc fn) => (u, v) => fn(u, v);

        public static Func<double, double, Vector3> Rotate(this Func<double, double, Vector3> fn, double zX, double zY, double zZ) => (u, v) => Rotate((u, v) => fn(u, v), new Vector3(zX, zY, zZ))(u, v);
        public static SurfaceFunc Rotate(this SurfaceFunc fn, double zX, double zY, double zZ) => Rotate(fn, new Vector3(zX, zY, zZ));

        public static SurfaceFunc Rotate(this SurfaceFunc fn, Vector3 zAx)
        {
            var q = Quaternion.FromRotation(Vector3.ZAxis, zAx.Normalize());

            return (u, v) => q * fn(u, v);
        }

        public static SurfaceFunc MoveZ(this SurfaceFunc fn, double z) => Move(fn, new Vector3(0, 0, z));
        public static SurfaceFunc Move(this SurfaceFunc fn, double x, double y, double z) => Move(fn, new Vector3(x, y, z));
        public static SurfaceFunc Move(this SurfaceFunc fn, Vector3 move)
        {
            return (u, v) => fn(u, v) + move;
        }

        public static Func<double, double, Vector3> Boxed(this SurfaceFunc fn, Vector3 scale, Vector3 center) => (u, v) => fn(u, v).Scale(scale) + center;

        public static SurfaceFunc Scale(this SurfaceFunc fn, double x, double y, double z) => Scale(fn, new Vector3(x, y, z));
        public static SurfaceFunc Scale(this SurfaceFunc fn, Vector3 scale)
        {
            return (u, v) => fn(u, v).Scale(scale);
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
