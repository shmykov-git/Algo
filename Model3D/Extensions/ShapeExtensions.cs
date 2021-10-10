using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Libraries;
using Model3D.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
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
            Convexes = shape.Convexes,
            Materials = shape.Materials
        };

        public static Shape ApplyZ(this Shape shape, Func3Z func, Func3Z func2 = null) => new Shape
        {
            Points = shape.Points.Select(p => new Vector4(p.x, p.y, p.z + func(p.x, p.y) + (func2?.Invoke(p.x, p.y) ?? 0), p.w)).ToArray(),
            Convexes = shape.Convexes
        };

        public static Shape SetZ(this Shape shape, Func3Z func) => new Shape
        {
            Points = shape.Points.Select(p => new Vector4(p.x, p.y, func(p.x, p.y), p.w)).ToArray(),
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
                    convex.Reverse().Select(i=>i+shape.Points.Length).ToArray()

                }.Concat(convex.SelectCirclePair((i, j) => new int[] { i, i + shape.Points.Length, j + shape.Points.Length, j }).ToArray())).ToArray()
            };
        }

        public static Shape ToSphere(this Shape shape)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => p.ToV3().ToLen(0.5).ToV4()).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape ToMetaShape(this Shape shape, double multPoint = 1, double multLines = 1, Material pointMaterial = null, Material linesMaterial = null)
        {
            return shape.ToSpots(multPoint, pointMaterial).Join(shape.ToLines(multLines, linesMaterial));
        }

        public static Shape ToMetaShape3(this Shape shape, double multPoint = 1, double multLines = 1, Color? pointColor = null, Color? linesColor = null, Shape spotShape = null)
        {
            return shape.ToLines3(multLines, linesColor)
                .Join(shape.ToSpots3(multPoint, pointColor, spotShape));
        }

        public static Shape ToCubeMetaShape3(this Shape shape, double multPoint = 1, double multLines = 1, Color? pointColor = null, Color? linesColor = null)
        {
            return shape.ToLines3(multLines, linesColor)
                .Join(shape.ToCubeSpots3(multPoint, pointColor));
        }

        public static Shape ToTetrahedronMetaShape3(this Shape shape, double multPoint = 1, double multLines = 1, Color? pointColor = null, Color? linesColor = null)
        {
            return shape.ToLines3(multLines, linesColor)
                .Join(shape.ToTetrahedronSpots3(multPoint, pointColor));
        }

        public static Shape ToMetaShapeWithMaterial3(this Shape shape, double multPoint = 1, double multLines = 1, Material pointMaterial = null, Material linesMaterial = null)
        {
            return shape.ToSpots3WithMaterial(multPoint, null, pointMaterial).Join(shape.ToLines3WithMaterial(multLines, linesMaterial));
        }

        public static Shape ToSpots(this Shape shape, double mult = 1, Material material = null)
        {
            var spot = Polygons.Elipse(1, 1, 10).Mult(0.02 * mult).ToShape2().ToShape3();

            return new Shape
            {
                Points3 = shape.Points3.SelectMany(p => spot.Points3.Select(s => p + s)).ToArray(),
                Convexes = shape.PointIndices.SelectMany(i => spot.Convexes.Select(convex => convex.Select(j => spot.PointsCount * i + j).ToArray())).ToArray()
            }.ApplyMaterial(material);
        }

        public static Shape ToNumSpots3(this Shape shape, double mult = 1, Color? spotColor = null, Color? numColor = null)
        {
            spotColor ??= Color.Red;
            numColor ??= Color.Black;

            var shapes = new List<Shape>();

            foreach (var (i, p) in shape.Points3.IndexValue())
            {
                var iText = Vectorizer.GetText(i.ToString()).ToLines3(500).Centered().Mult(0.002* mult).Move(p).Move(mult*new Vector3(0.1, 0.1, 0)).ApplyColor(numColor.Value);
                shapes.Add(iText);
            }

            var spots = shape.ToSpots3(mult, spotColor.Value);

            return shapes.Aggregate((a, b) => a + b) + spots;
        }

        public static Shape ToNumFigureSpots3(this Shape shape, double mult = 1, Color? spotColor = null, Color? numColor = null, Color? fiveColor = null)
        {
            spotColor ??= Color.Red;
            numColor ??= Color.DarkGreen;
            fiveColor ??= Color.Black;
            
            var shapes = new List<Shape>();

            foreach (var (i, p) in shape.Points3.IndexValue())
            {
                var n = i % 5;
                if (n > 0)
                {
                    var circle = Polygons.Elipse(1, 1, n).ToShape2().ToShape3().Mult(0.1* mult).Centered().ToMetaShape3(0.5, 1).Move(p).Move(0, 0, 0.1).ApplyColor(numColor.Value);
                    shapes.Add(circle);
                }

                var k = i / 5;
                for (var j = 1; j <= k; j++)
                {
                    var five = Polygons.Elipse(1, 1, 5).ToShape2().ToShape3().AddVolumeZ(0.1).Mult(0.07 * mult).Move(p).Move(0, 0, 0.1 + j * 0.05).ApplyColor(fiveColor.Value);
                    shapes.Add(five);
                }
            }

            var spots = shape.ToSpots3(mult, spotColor.Value);

            return shapes.Aggregate((a, b) => a + b) + spots;
        }

        public static Shape ToSpots3(this Shape shape, double mult = 1, Color? color = null, Shape spotShape = null) => shape.ToSpots3WithMaterial(mult, spotShape, color.HasValue ? new Material { Color = color.Value } : null);

        public static Shape ToCubeSpots3(this Shape shape, double mult = 1, Color? color = null) =>
            shape.ToSpots3WithMaterial(mult, Shapes.Cube.Centered().Mult(0.02 * mult), color.HasValue ? new Material { Color = color.Value } : null);

        public static Shape ToTetrahedronSpots3(this Shape shape, double mult = 1, Color? color = null) =>
            shape.ToSpots3WithMaterial(mult, Shapes.Tetrahedron.Centered().Mult(0.04 * mult), color.HasValue ? new Material { Color = color.Value } : null);

        public static Shape ToSpots3WithMaterial(this Shape shape, double mult = 1, Shape pointShape = null, Material material = null)
        {
            pointShape ??= Shapes.Icosahedron;
            pointShape = pointShape.Mult(0.02 * mult);

            return new Shape
            {
                Points3 = shape.Points3.SelectMany(p => pointShape.Points3.Select(s => p + s)).ToArray(),
                Convexes = shape.PointIndices.SelectMany(i => pointShape.Convexes.Select(convex => convex.Select(j => pointShape.PointsCount * i + j).ToArray())).ToArray()
            }.ApplyMaterial(material);
        }

        public static Shape ToLines3(this Shape shape, double mult = 1, Color? color = null) => shape.ToLines3WithMaterial(mult, color.HasValue ? new Material { Color = color.Value } : null);

        public static Shape ToLines3WithMaterial(this Shape shape, double mult = 1, Material material = null)
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
            }.ApplyMaterial(material);
        }

        public static Shape ToLines(this Shape shape, double mult = 1, Material material = null)
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
                }.Select(v => v.ToV4()).ToArray();
            }

            var lines = shape.OrderedEdges.Select(GetLine).ToArray();

            return new Shape
            {
                Points = lines.SelectMany(edge => edge).ToArray(),
                Convexes = lines.Index().Select(i => new[] { 4 * i, 4 * i + 1, 4 * i + 2, 4 * i + 3 }).ToArray()
            }.ApplyMaterial(material);
        }

        public static Shape Join(this Shape shape, Shape another)
        {
            var newShape = new Shape
            {
                Points = shape.Points.Concat(another.Points).ToArray(),
                Convexes = shape.Convexes.Concat(another.Convexes.Select(convex => convex.Select(i => i + shape.PointsCount).ToArray())).ToArray()
            };

            if (shape.Materials != null || another.Materials != null)
            {
                newShape.Materials = shape.MaterialsOrDefault().Concat(another.MaterialsOrDefault()).ToArray();
            }

            return newShape;
        }

        public static Material[] MaterialsOrDefault(this Shape shape) => shape.Materials ?? shape.Convexes.Index().Select(i => (Material)null).ToArray();

        public static Shape Mult(this Shape shape, double k)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(k * p.x, k * p.y, k * p.z, p.w)).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Scale(this Shape shape, double x, double y, double z)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(x * p.x, y * p.y, z * p.z, p.w)).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Move(this Shape shape, double x, double y, double z)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(x + p.x, y + p.y, z + p.z, p.w)).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Move(this Shape shape, Vector3 v)
        {
            return new Shape
            {
                Points3 = shape.Points3.Select(p => p + v).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Rotate(this Shape shape, Quaternion q)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => q * p).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Centered(this Shape shape)
        {
            return new Shape
            {
                Points3 = shape.Points3.Centered(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape CenteredXZ(this Shape shape)
        {
            var shift = shape.Points3.Select(p => new Vector3(p.x, 0, p.z)).Center();

            return shape.Move(-shift);
        }

        public static Shape BottomedY(this Shape shape)
        {
            var minY = shape.Points.Min(p => p.y);

            return shape.Move(0, -minY, 0);
        }

        public static Shape SplitConvexes(this Shape shape)
        {
            return Extender.SplitConvexes(shape);
        }

        public static Shape SplitSphere(this Shape shape, double deformation = 1.5)
        {
            return Extender.SplitSphere(shape, deformation);
        }

        public static Shape JoinConvexesBy6(this Shape shape)
        {
            return Extender.JoinConvexesBy6(shape);
        }

        public static Shape ToMaze(this Shape shape, int seed = 0, MazeType type = MazeType.SimpleRandom, (int i, int j)[] exits = null, bool openExits = true)
        {
            return Mazerator.MakeMaze(shape, seed, type, exits, openExits);
        }

        public static (Shape maze, Shape path) ToMazeWithPath(this Shape shape, int seed = 0, MazeType type = MazeType.SimpleRandom, (int i, int j)[] exits = null, bool openExits = true)
        {
            return Mazerator.MakeMazeWithPath(shape, seed, type, exits, openExits);
        }

        public static Shape[] SplitByMaterial(this Shape shape)
        {
            return Extender.SplitConvexesByMaterial(shape);
        }

        public static Shape CurveZ(this Shape tube, Func3 fn)
        {
            var from = tube.Points.Min(v => v.z);
            var to = tube.Points.Max(v => v.z);
            var dt = 0.001;

            Vector3 Rotate(Vector3 p)
            {
                var t = p.z;
                var a = fn(t - dt);
                var b = fn(t);
                var dz = (b - a).Normalize();

                return Quaternion.FromRotation(Vector3.ZAxis, dz) * new Vector3(p.x, p.y, 0);
            }

            return new Shape
            {
                Points3 = tube.Points3.Select(p => new Vector3(p.x, p.y, p.z)).Select(p => Rotate(p) + fn(p.z)).ToArray(),
                Convexes = tube.Convexes
            };
        }

        public static Shape ApplyFn(this Shape shape, Func_3 fnX = null, Func_3 fnY = null, Func_3 fnZ = null)
        {
            fnX ??= v => v.x;
            fnY ??= v => v.y;
            fnZ ??= v => v.z;

            return new Shape
            {
                Points3 = shape.Points3.Select(p => new Vector3(fnX(p), fnY(p), fnZ(p))).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Normed(this Shape shape)
        {
            var r = shape.GetRadius();
            return shape.Mult(1 / r);
        }

        public static Shape Normalize(this Shape shape)
        {
            var shapePoints = shape.Points3.Select(p => p.ToV3D()).ToArray();
            var points = shapePoints.Distinct().ToList();
            var convexes = shape.Convexes.Transform(i => points.IndexOf(shapePoints[i]));
            convexes = convexes.Select(convex => convex.OrderSafeDistinct().ToArray()).Where(convex => convex.Length >= 3).ToArray();

            var allMaterials = shape.Materials?.Distinct().ToArray();
            // todo: support all materials
            var materials = ((allMaterials?.Length ?? 0) == 1) ? convexes.Index().Select(_ => allMaterials[0]).ToArray() : null;

            return new Shape
            {
                Points3 = points.Select(p=>p.ToV3()).ToArray(),
                Convexes = convexes,
                Materials = materials
            };
        }

        public static Shape ApplyMaterial(this Shape shape, Material material)
        {
            if (material == null)
                return shape;

            shape.Materials = shape.Convexes.Index().Select(i => material).ToArray();

            return shape;
        }

        public static Shape ApplyColor(this Shape shape, Color color)
        {
            shape.ApplyMaterial(Materials.GetByColor(color));

            return shape;
        }

        public static Shape ApplyColorGradientX(this Shape shape, params Color?[] colors) => shape.ApplyColorGradient(v => v.x, colors);
        public static Shape ApplyColorGradientX(this Shape shape, Func3Z gradientFn, params Color?[] colors) => shape.ApplyColorGradient(v => gradientFn(v.y,v.z), colors);

        public static Shape ApplyColorGradientY(this Shape shape, params Color?[] colors) => shape.ApplyColorGradient(v => v.y, colors);
        public static Shape ApplyColorGradientY(this Shape shape, Func3Z gradientFn, params Color?[] colors) => shape.ApplyColorGradient(v => gradientFn(v.x,v.z), colors);

        public static Shape ApplyColorGradientZ(this Shape shape, params Color?[] colors) => shape.ApplyColorGradient(v => v.z, colors);
        public static Shape ApplyColorGradientZ(this Shape shape, Func3Z gradientFn, params Color?[] colors) => shape.ApplyColorGradient(v => gradientFn(v.x,v.y), colors);

        private static Shape ApplyColorGradient(this Shape shape, Func<Vector4, double> valueFn, params Color?[] colors)
        {
            var centers = shape.Convexes.Select(convex => convex.Select(i => shape.Points[i]).Center()).ToArray();

            var min = centers.Min(p => valueFn(p));
            var max = centers.Max(p => valueFn(p));
            max += (max - min) * 0.00001;

            Vector4 GetColor(double z, Color current)
            {
                var n = colors.Length - 1;
                var k = (z - min) / (max - min); // [0, 1)
                var j = (int)(k * n);
                var fromColor = colors[j] ?? current;
                var toColor = colors[j + 1] ?? current;
                var kk = (k - j * 1.0 / n) * n;

                return new Vector4(fromColor) + (new Vector4(toColor) - new Vector4(fromColor)) * kk;
            }

            shape.Materials = centers.Index().Select(i => GetColor(valueFn(centers[i]), shape.Materials?[i].Color??default)).Select(color => Materials.GetByColor(color)).ToArray();

            return shape;
        }

    }
}
