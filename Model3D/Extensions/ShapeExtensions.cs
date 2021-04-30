using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Libraries;
using System.Collections.Generic;
using System.Linq;

namespace Model3D.Extensions
{

    public static class ShapeExtensions
    {
        public static Shape Transform(this Shape shape, Multiplication transformation)
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

        public static Shape ToSpots(this Shape shape)
        {
            var spot = Polygons.Elipse(1, 1, 10).Mult(0.02).MakeShape();

            return new Shape
            {
                Points3 = shape.Points3.SelectMany(p => spot.Points3.Select(s => p + s)).ToArray(),
                Convexes = shape.PointIndices.SelectMany(i => spot.Convexes.Select(convex => convex.Select(j => spot.PointsCount * i + j).ToArray())).ToArray()
            };
        }

        public static Shape ToMetaShape(this Shape shape)
        {
            return shape.ToSpots().Join(shape.ToLines());
        }

        public static Shape Join(this Shape shape, Shape another)
        {
            return new Shape
            {
                Points = shape.Points.Concat(another.Points).ToArray(),
                Convexes = shape.Convexes.Concat(another.Convexes.Select(convex => convex.Select(i => i + shape.PointsCount).ToArray())).ToArray()
            };
        }

        public static Shape ToLines(this Shape shape)
        {
            var width = 0.003;
            var points = shape.Points2;

            Vector4[] GetLine((int,int) edge)
            {
                var a = points[edge.Item1];
                var b = points[edge.Item2];
                var line = new Line2(a, b);
                var shift = line.Normal * width * 0.5;

                return new Model.Vector2[]
                {
                    a + shift,
                    b + shift,
                    b - shift,
                    a - shift
                }.Select(v=>v.ToV4()).ToArray();
            }

            var lines = shape.OrderedEdges.Select(GetLine).ToArray();

            return new Shape
            {
                Points = lines.SelectMany(edge => edge).ToArray(),
                Convexes = lines.Index().Select(i => new[] { 4 * i, 4 * i + 1, 4 * i + 2, 4 * i + 3 }).ToArray()
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
