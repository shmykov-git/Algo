using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Libraries;
using Model3D.Tools;
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

        public static Shape Transform(this Shape shape, TransformFunc3 fn) => new Shape
        {
            Points3 = shape.Points3.Select(p => fn(p)).ToArray(),
            Convexes = shape.Convexes
        };

        public static Shape ApplyZ(this Shape shape, Func3Z func, Func3Z func2 = null) => new Shape
        {
            Points = shape.Points.Select(p => new Vector4(p.x, p.y, p.z + func(p.x, p.y) + (func2?.Invoke(p.x, p.y) ?? 0), p.w)).ToArray(),
            Convexes = shape.Convexes
        };

        public static Shape AddVolumeX(this Shape shape, double xVolume) => AddVolume(shape, xVolume, 0, 0);
        public static Shape AddVolumeY(this Shape shape, double yVolume) => AddVolume(shape, 0, yVolume, 0);
        public static Shape AddVolumeZ(this Shape shape, double zVolume) => AddVolume(shape, 0, 0, zVolume);

        public static Shape AddVolume(this Shape shape, double x, double y, double z)
        {
            var halfVolume = new Vector4(x, y, z, 0) * 0.5;

            return new Shape
            {
                Points = shape.Points.Select(p => p - halfVolume)
                    .Concat(shape.Points.Select(p => p + halfVolume)).ToArray(),

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

        public static Shape ToMetaShape(this Shape shape, double multPoint = 1, double multLines = 1)
        {
            return shape.ToSpots(multPoint).Join(shape.ToLines(multLines));
        }

        public static Shape ToMetaShape3(this Shape shape, double multPoint = 1, double multLines = 1)
        {
            return shape.ToSpots3(multPoint).Join(shape.ToLines3(multLines));
        }

        public static Shape ToSpots(this Shape shape, double mult = 1)
        {
            var spot = Polygons.Elipse(1, 1, 10).Mult(0.02 * mult).ToShape2().ToShape3();

            return new Shape
            {
                Points3 = shape.Points3.SelectMany(p => spot.Points3.Select(s => p + s)).ToArray(),
                Convexes = shape.PointIndices.SelectMany(i => spot.Convexes.Select(convex => convex.Select(j => spot.PointsCount * i + j).ToArray())).ToArray()
            };
        }

        public static Shape ToSpots3(this Shape shape, double mult = 1)
        {
            var sphere = Surfaces.Sphere(9, 5, true).Mult(0.02 * mult);

            return new Shape
            {
                Points3 = shape.Points3.SelectMany(p => sphere.Points3.Select(s => p + s)).ToArray(),
                Convexes = shape.PointIndices.SelectMany(i => sphere.Convexes.Select(convex => convex.Select(j => sphere.PointsCount * i + j).ToArray())).ToArray()
            };
        }

        public static Shape ToLines(this Shape shape, double mult = 1)
        {
            var width = 0.003 * mult;
            var points = shape.Points2;
            var points3 = shape.Points3;

            Vector4[] GetLine((int i, int j) e)
            {
                var a = points[e.i];
                var b = points[e.j];
                var line = new Line2(a, b);
                var shift = line.Normal * width * 0.5;

                return new Vector3[]
                {
                    (a + shift).ToV3(points3[e.i].z),
                    (b + shift).ToV3(points3[e.j].z),
                    (b - shift).ToV3(points3[e.j].z),
                    (a - shift).ToV3(points3[e.i].z)
                }.Select(v=>v.ToV4()).ToArray();
            }

            var lines = shape.OrderedEdges.Select(GetLine).ToArray();

            return new Shape
            {
                Points = lines.SelectMany(edge => edge).ToArray(),
                Convexes = lines.Index().Select(i => new[] { 4 * i, 4 * i + 1, 4 * i + 2, 4 * i + 3 }).ToArray()
            };
        }

        public static Shape ToLines3(this Shape shape, double mult = 1)
        {
            var line3 = Surfaces.Cylinder(5, 2);
            var n = line3.PointsCount;
            
            var width = 0.003 * mult;
            var points = shape.Points3;

            Shape GetLine((int i, int j) e)
            {
                var a = points[e.i];
                var b = points[e.j];
                var ab = b - a;
                var q = Quaternion.FromRotation(Vector3.ZAxis, ab.Normalize());

                var line = line3.Scale(width, width, ab.Length).Transform(p => q * p).Move(a);
                
                return line;
            }

            var lines = shape.OrderedEdges.Select(GetLine).ToArray();

            return new Shape
            {
                Points = lines.SelectMany(line => line.Points).ToArray(),
                Convexes = lines.Index().SelectMany(i => lines[i].Convexes.Transform(c => c + i * n)).ToArray()
            };
        }

        public static Shape Join(this Shape shape, Shape another)
        {
            return new Shape
            {
                Points = shape.Points.Concat(another.Points).ToArray(),
                Convexes = shape.Convexes.Concat(another.Convexes.Select(convex => convex.Select(i => i + shape.PointsCount).ToArray())).ToArray()
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

        public static Shape Scale(this Shape shape, double x, double y, double z)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(x * p.x, y * p.y, z * p.z, p.w)).ToArray(),
                Convexes = shape.Convexes
            };
        }

        public static Shape Move(this Shape shape, double x, double y, double z)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(x + p.x, y + p.y, z + p.z, p.w)).ToArray(),
                Convexes = shape.Convexes
            };
        }

        public static Shape Move(this Shape shape, Vector3 v)
        {
            return new Shape
            {
                Points3 = shape.Points3.Select(p => p + v).ToArray(),
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

        public static Shape Centered(this Shape shape)
        {
            return new Shape
            {
                Points3 = shape.Points3.Centered(),
                Convexes = shape.Convexes
            };
        }

        public static Shape SplitConvexes(this Shape shape)
        {
            return Extender.SplitConvexes(shape);
        }

        public static Shape ToTube(this Shape shape, Func3 func)
        {
            return Tuber.MakeTube(func, shape);
        }

        public static Shape Normalize(this Shape shape)
        {
            var shapePoints = shape.Points3.Select(p => p.ToV3D()).ToArray();
            var points = shapePoints.Distinct().ToList();
            var convexes = shape.Convexes.Transform(i => points.IndexOf(shapePoints[i]));
            convexes = convexes.Select(convex => convex.OrderSafeDistinct().ToArray()).Where(convex => convex.Length >= 3).ToArray();

            return new Shape
            {
                Points3 = points.Select(p=>p.ToV3()).ToArray(),
                Convexes = convexes
            };
        }
    }
}
