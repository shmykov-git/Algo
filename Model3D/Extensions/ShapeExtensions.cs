using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Libraries;
using System.Linq;

namespace Model3D.Extensions
{
    public static class ShapeExtensions
    {
        public static Shape Transform(this Shape shape, Transformation transformation)
        {
            return new Shape
            {
                Points = transformation.Transformations.SelectMany(f => shape.Points.Select(p => f(p))).ToArray(),
                Convexes = transformation.Transformations.Index()
                    .SelectMany(i => shape.Convexes.Select(convex => convex.Select(j => shape.Points.Length * i + j).ToArray())).ToArray()
            };
        }

        public static Shape ApplyZ(this Shape shape, Func3 func)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(p.x, p.y, p.z + func(p.x, p.y), p.w)).ToArray(),
                Convexes = shape.Convexes
            };
        }

        public static Shape AddZVolume(this Shape shape, double zVolume)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(p.x, p.y, p.z - zVolume / 2, p.w))
                    .Concat(shape.Points.Select(p => new Vector4(p.x, p.y, p.z + zVolume / 2, p.w))).ToArray(),

                Convexes = shape.Convexes.SelectMany(convex => new int[][]
                {
                    convex,
                    convex.Select(i=>i+shape.Points.Length).ToArray()

                }.Concat(convex.SelectCirclePair((i, j) => new int[] { i, i + shape.Points.Length, j + shape.Points.Length, j }).ToArray())).ToArray()
            };
        }

        public static Shape ToSphere(this Shape shape)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => p.ToV3().ToLen(0.5).ToV4()).ToArray(),
                Convexes = shape.Convexes
            };
        }

        public static Shape Mult(this Shape shape, double k)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(k * p.x, k * p.y, k * p.z, p.w)).ToArray(),
                Convexes = shape.Convexes
            };
        }

        public static Shape Rotate(this Shape shape, Quaternion q)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => q * p).ToArray(),
                Convexes = shape.Convexes
            };
        }
    }
}
