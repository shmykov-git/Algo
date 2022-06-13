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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics;
using Meta;
using Model.Graphs;
using View3D.Libraries;

namespace Model3D.Extensions
{
    public static class ShapeExtensions
    {
        private static Vectorizer vectorizer = DI.Get<Vectorizer>();

        public static Shape Transform(this Shape shape, Multiplication transformation)
        {
            return new Shape
            {
                Points = transformation.Transformations.SelectMany(f => shape.Points.Select(p => f(p))).ToArray(),
                Convexes = transformation.Transformations.Index()
                    .SelectMany(i => shape.Convexes.Select(convex => convex.Select(j => shape.Points.Length * i + j).ToArray())).ToArray()
            };
        }

        public static Shape Transform(this Shape shape, Func<Shape, Shape> trFn) => trFn(shape);

        public static Shape Transform(this Shape shape, TransformFunc3 fn) => new Shape
        {
            Points3 = shape.Points3.Select(p => fn(p)).ToArray(),
            Convexes = shape.Convexes,
            Materials = shape.Materials
        };

        public static Shape ApplyZ(this Shape shape, Func3Z func) => new Shape
        {
            Points = shape.Points.Select(p => new Vector4(p.x, p.y, p.z + func(p.x, p.y), p.w)).ToArray(),
            Convexes = shape.Convexes
        };

        public static Shape Where(this Shape shape, Func<Vector3, bool> whereFunc)
        {
            var pBi = shape.Points3.WhereBi(whereFunc);

            return new Shape
            {
                Points3 = pBi.items.ToArray(),
                Convexes = shape.Convexes.Transform(i => pBi.bi[i]).CleanBi()
            };
        }

        public static Shape WhereR(this Shape shape, double x, double y, double r)
        {
            return WhereR(shape, (x, y, r));
        }

        public static Shape WhereNotR(this Shape shape, double x, double y, double r)
        {
            return WhereNotR(shape, (x, y, r));
        }

        public static Shape WhereR(this Shape shape,
            params (double x, double y, double r)[] areas)
        {
            var conds = areas.Select(a => (Func<Vector3, bool>)(v => (v - new Vector3(a.x, a.y, 0)).Length > a.r)).ToArray();

            return shape.Where(v => conds.Any(fn => !fn(v)));
        }

        public static Shape WhereNotR(this Shape shape,
            params (double x, double y, double r)[] areas)
        {
            var conds = areas.Select(a => (Func<Vector3, bool>)(v => (v - new Vector3(a.x, a.y, 0)).Length > a.r)).ToArray();

            return shape.Where(v => conds.All(fn => fn(v)));
        }

        public static (Shape main, Shape cut) SplitR(this Shape shape,
            params (double x, double y, double r)[] areas)
        {
            var conds = areas.Select(a => (Func<Vector3, bool>) (v => (v - new Vector3(a.x, a.y, 0)).Length > a.r)).ToArray();
            var main = shape.Where(v => conds.All(fn => fn(v)));
            var cut = shape.Where(v => conds.Any(fn => !fn(v)));

            return (main, cut);
        }

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

                }.Concat(convex.SelectCirclePair((i, j) => new int[] { i, i + shape.Points.Length, j + shape.Points.Length, j }).ToArray())).ToArray(),
            };
        }

        public static Shape AddPerimeterVolume(this Shape shape, double z)
        {
            var halfVolume = new Vector4(0, 0, z, 0) * 0.5;
            var perimeters = shape.GetPerimeters();

            return new Shape
            {
                Points = shape.Points.Select(p => p - halfVolume)
                    .Concat(shape.Points.Select(p => p + halfVolume)).ToArray(),

                Convexes = new IEnumerable<int[]>[]
                {
                    shape.Convexes,
                    shape.Convexes.Select(convex=>convex.Reverse().Select(i=>i+shape.Points.Length).ToArray()),
                    perimeters.SelectMany(p=>p.SelectCirclePair((i,j)=>new int[] { i, i + shape.Points.Length, j + shape.Points.Length, j }))
                }.SelectMany(v=>v).ToArray()
            };
        }


        public static Shape AddSphereVolume(this Shape shape, double thicknessMult = 1.1)
        {
            return new Shape
            {
                Points3 = shape.Points3.Concat(shape.Points3.Select(p => thicknessMult*p)).ToArray(),

                Convexes = shape.Convexes.SelectMany(convex => new int[][]
                {
                    convex.Reverse().ToArray(),
                    convex.Select(i=>i+shape.Points.Length).ToArray()
                }).ToArray() //.Concat(convex.SelectCirclePair((i, j) => new int[] { i, i + shape.Points.Length, j + shape.Points.Length, j }).ToArray())).ToArray(),
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

        public static Shape ToBlowedShape(this IEnumerable<Shape> shapeList, double mult = 2, double angle = 0) => shapeList.Select(s =>
        {
            var c = s.MassCenter;
            var move = (mult - 1) * c;

            return s.RotateMassCenter(angle).Move(move);
        }).ToSingleShape();

        public static Shape ToRandomBlowedShape(this IEnumerable<Shape> shapeList, double mult = 2, int seed = 0)
        {
            var rnd = new Random(seed);

            return shapeList.Select(s =>
            {
                var c = s.MassCenter;
                var move = (mult - 1) * c;

                return s.RotateMassCenter(2 * Math.PI * rnd.NextDouble()).Move(move);
            }).ToSingleShape();
        }

        //public static Shape ToSingleShape1(this IEnumerable<Shape> shapeList)
        //{
        //    var shapes = shapeList.ToArray();

        //    while (shapes.Length > 1)
        //    {
        //        shapes = shapes.SelectByPair((a, b) => a + (b ?? Shape.Empty)).ToArray();
        //    }

        //    return shapes[0];
        //}

        public static Shape ToSingleShape(this IEnumerable<Shape> shapeList)
        {
            var shapes = shapeList.ToArray();

            if (shapes.Length == 0)
                return Shape.Empty;

            while (shapes.Length > 1)
            {
                var tasks = shapes.SelectByPair((a, b) => Task.Run(()=>a + (b ?? Shape.Empty))).ToArray();
                //Debug.WriteLine(tasks.Length);
                Task.WhenAll(tasks).Wait();
                shapes = tasks.Select(t => t.Result).ToArray();
            }

            return shapes[0];
        }

        public static Shape2 ToShape2(this Shape shape)
        {
            return new Shape2
            {
                Points = shape.Points.Select(p => p.ToV2()).ToArray(),
                Convexes = shape.Convexes,
            };
        }

        public static Shape ToMetaShape(this Shape shape, double multPoint = 1, double multLines = 1, Material pointMaterial = null, Material linesMaterial = null)
        {
            return shape.ToSpots(multPoint, pointMaterial).Join(shape.ToPlaneLines(multLines, linesMaterial));
        }

        public static Shape ToMetaShape3(this Shape shape, double multPoint = 1, double multLines = 1, Color? pointColor = null, Color? linesColor = null, Shape spotShape = null)
        {
            return shape.ToLines(multLines, linesColor)
                .Join(shape.ToSpots3(multPoint, pointColor, spotShape));
        }

        public static Shape ToCubeMetaShape3(this Shape shape, double multPoint = 1, double multLines = 1, Color? pointColor = null, Color? linesColor = null)
        {
            return shape.ToLines(multLines, linesColor)
                .Join(shape.ToCubeSpots3(multPoint, pointColor));
        }

        public static Shape ToTetrahedronMetaShape3(this Shape shape, double multPoint = 1, double multLines = 1, Color? pointColor = null, Color? linesColor = null)
        {
            return shape.ToLines(multLines, linesColor)
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

            var texts = new List<Shape>();

            foreach (var (i, p) in shape.Points3.IndexValue())
            {
                var iText = vectorizer.GetText(i.ToString()).Perfecto(0.1*mult).Move(p).Move(mult*new Vector3(0.1, 0.1, 0)).ToLines(mult, numColor);
                texts.Add(iText);
            }

            var spots = shape.ToSpots3(mult, spotColor.Value);

            return texts.ToSingleShape() + spots;
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
                    var circle = Polygons.Elipse(1, 1, n).ToShape2().ToShape3().Mult(0.1* mult).MassCentered().ToMetaShape3(0.5, 1).Move(p).Move(0, 0, 0.1).ApplyColor(numColor.Value);
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
            shape.ToSpots3WithMaterial(mult, Shapes.Cube.MassCentered(), color.HasValue ? new Material { Color = color.Value } : null);

        public static Shape ToTetrahedronSpots3(this Shape shape, double mult = 1, Color? color = null) =>
            shape.ToSpots3WithMaterial(mult, Shapes.Tetrahedron.MassCentered(), color.HasValue ? new Material { Color = color.Value } : null);

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

        public static Shape ToLines(this Shape shape, double mult = 1, Color? color = null) => shape.ToLines3WithMaterial(mult, color.HasValue ? Materials.GetByColor(color.Value) : null);
        public static Shape ToShapedLines(this Shape shape, Shape lineShape, double mult = 1, Color? color = null) => shape.ToLines3WithMaterial(mult, color.HasValue ? Materials.GetByColor(color.Value) : null, true, lineShape);

        public static Shape ToLines3WithMaterial(this Shape shape, double mult = 1, Material material = null, bool stretch = true, Shape lineShape = null)
        {
            lineShape = lineShape == null ? Surfaces.Cylinder(5, 2) : lineShape.Align(0.5, 0.5, 0);

            var n = lineShape.PointsCount;

            var width = 0.003 * mult;
            var points = shape.Points3;

            Shape GetLine((int i, int j) e)
            {
                var a = points[e.i];
                var b = points[e.j];
                var ab = b - a;
                var q = Quaternion.FromRotation(Vector3.ZAxis, ab.Normalize());

                var line = stretch 
                    ? lineShape.Scale(width, width, 0.8*width + ab.Length).Move(0, 0, -0.4 * width).Transform(p => q * p).Move(a)
                    : lineShape.Scale(width, width, ab.Length).Transform(p => q * p).Move(a);

                return line;
            }

            var lines = shape.OrderedEdges.Select(GetLine).ToArray();

            return new Shape
            {
                Points = lines.SelectMany(line => line.Points).ToArray(),
                Convexes = lines.Index().SelectMany(i => lines[i].Convexes.Transform(c => c + i * n)).ToArray()
            }.ApplyMaterial(material);
        }

        public static Shape ToPlaneLines(this Shape shape, double mult = 1, Material material = null)
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

        public static Shape ScaleX(this Shape shape, double mult) => Scale(shape, mult, 1, 1);
        public static Shape ScaleY(this Shape shape, double mult) => Scale(shape, 1, mult, 1);
        public static Shape ScaleZ(this Shape shape, double mult) => Scale(shape, 1, 1, mult);

        public static Shape Scale(this Shape shape, double x, double y, double z)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(x * p.x, y * p.y, z * p.z, p.w)).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape MoveX(this Shape shape, double value) => Move(shape, value, 0, 0);
        public static Shape MoveY(this Shape shape, double value) => Move(shape, 0, value, 0);
        public static Shape MoveZ(this Shape shape, double value) => Move(shape, 0, 0, value);

        public static Shape Move(this Shape shape, double x, double y, double z)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(x + p.x, y + p.y, z + p.z, p.w)).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape SplitLines(this Shape shape, int count)
        {
            var newPoints = shape
                .Convexes
                .SelectMany(convex => convex.SelectCirclePair((i, j) => (i, j)))
                .Select(e => new Line3(shape.Points[e.i].ToV3(), shape.Points[e.j].ToV3()))
                .SelectMany(l => (count).SelectRange(i => l.a + (i + 1d) / (count + 1) * l.ab))
                .ToArray();

            var n = shape.Points.Length;

            var k = -1;
            var newConvexes = shape.Convexes.Select(convex => convex.SelectCirclePair((i, j) =>
                {
                    k++;
                    return new[] { i }.Concat((count).SelectRange(l => n + k * count + l)).ToArray();
                })
                .SelectMany(c => c).ToArray()).ToArray();

            return new Shape
            {
                Points3 = shape.Points3.Concat(newPoints).ToArray(),
                Convexes = newConvexes,
                Materials = shape.Materials
            };
        }

        public static Shape SplitLongLine(this Shape shape, bool spliteTriangles = false, double? minSize = null)
        {
            var convexesInfos = shape
                .Convexes
                .Select(convex => convex.SelectCirclePair((i, j) => new
                {
                    i, j,
                    Line = new Line3(shape.Points[i].ToV3(), shape.Points[j].ToV3())
                }).ToArray())
                .SelectWithIndex((infos, index) => infos.Select(info=> new
                {
                    info.i, info.j, 
                    info.Line, 
                    Ind = shape.Points.Length + index,
                    LongLine = infos.OrderByDescending(c=>c.Line.Len).First().Line
                }).ToArray()).ToArray();


            var points = shape.Points.Concat(convexesInfos.Select(ps => ps.First(pi => pi.Line == pi.LongLine).LongLine.Center.ToV4()))
                .ToArray();

            if (spliteTriangles && shape.Convexes.All(c => c.Length == 3))
            {
                var convexes = convexesInfos.SelectMany(cs =>
                {
                    var l = cs.ToList();
                    var info = cs.First(ci => ci.Line == ci.LongLine);
                    var k = l.IndexOf(info);

                    if (minSize.HasValue && info.LongLine.Len > minSize.Value)
                    {
                        return new[]
                        {
                            new[] { cs[(k + 1) % 3].i, cs[(k + 2) % 3].i, info.Ind },
                            new[] { cs[(k + 2) % 3].i, cs[k].i, info.Ind },
                        };
                    }
                    else
                    {
                        return new[]
                        {
                            new[] { cs[0].i, cs[1].i, cs[2].i }
                        };
                    }
                }).ToArray();

                return new Shape
                {
                    Points = points,
                    Convexes = convexes
                };
            }
            else
            {
                var convexes = convexesInfos.Select(cs => cs.SelectMany(ci =>
                {
                    if (ci.LongLine == ci.Line)
                    {
                        return new[] { ci.i, ci.Ind };
                    }
                    else
                    {
                        return new[] { ci.i };
                    }
                }).ToArray()).ToArray();

                return new Shape
                {
                    Points = points,
                    Convexes = convexes,
                    Materials = shape.Materials
                };
            }
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

        public static Shape ToOx(this Shape shape) => shape.Rotate(Rotates.Z_X);
        public static Shape ToOxM(this Shape shape) => shape.Rotate(Rotates.Z_mX);
        public static Shape ToOy(this Shape shape) => shape.Rotate(Rotates.Z_Y);
        public static Shape OxToOy(this Shape shape) => shape.Rotate(Rotates.X_Y);

        public static Shape Rotate(this Shape shape, Quaternion q)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => q * p).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape FlipX(this Shape shape) => Rotate(shape, -Vector3.ZAxis);
        public static Shape FlipY(this Shape shape) => Rotate(shape, Quaternion.FromAngleAxis(Math.PI, Vector3.XAxis));
        public static Shape Rotate(this Shape shape, double x, double y, double z, Vector3? lookUp = null) => Rotate(shape, new Vector3(x, y, z), lookUp);

        public static Shape Rotate(this Shape shape, double alfa) => Rotate(shape, Quaternion.FromAngleAxis(alfa, Vector3.ZAxis));
        public static Shape Rotate(this Shape shape, Vector3 center, double alfa) => shape.Move(-center).Rotate(Quaternion.FromAngleAxis(alfa, Vector3.ZAxis)).Move(center);
        public static Shape RotateMassCenter(this Shape shape, double alfa) => shape.Rotate(shape.MassCenter, alfa);
        public static Shape RotateOx(this Shape shape, double alfa) => Rotate(shape, Quaternion.FromAngleAxis(alfa, Vector3.XAxis));
        public static Shape RotateOx(this Shape shape, double x, double y, double z) => Rotate(shape, Quaternion.FromRotation(Vector3.XAxis, new Vector3(x, y, z).Normalize()));

        public static Shape Rotate(this Shape shape, Vector3 zAxis, Vector3? yAxis = null)
        {
            var zN = zAxis.Normalize();
            var q1 = Quaternion.FromRotation(Vector3.ZAxis, zN);

            Func<Vector4, Vector4> fn = v => q1 * v;

            if (yAxis.HasValue)
            {
                var yN = yAxis.Value.Normalize();
                var yPAxis = yN.MultV(-zN).MultV(zN).Normalize();
                var q2 = Quaternion.FromRotation(q1 * Vector3.YAxis, yPAxis);
                fn = v => q2 * (q1 * v);
            }

            return new Shape
            {
                Points = shape.Points.Select(fn).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Centered(this Shape shape) => shape.Align(0.5, 0.5, 0.5);

        public static Shape MassCentered(this Shape shape)
        {
            return new Shape
            {
                Points3 = shape.Points3.Centered(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape TurnOut(this Shape shape)
        {
            return new Shape
            {
                Points = shape.Points,
                Convexes = shape.Convexes.Select(c=>c.Reverse().ToArray()).ToArray(),
                Materials = shape.Materials
            };
        }

        public static Shape MassCenteredXZ(this Shape shape) => MassCentered(shape, p => new Vector3(p.x, 0, p.z));

        public static Shape MassCentered(this Shape shape, Func<Vector3, Vector3> getValue)
        {
            var shift = shape.Points3.Select(getValue).Center();

            return shape.Move(-shift);
        }

        public static Shape Align(this Shape shape, double x, double y, double z)
        {
            var b = shape.GetBorders();

            var shift = new Vector3(
                b.min.x + x * (b.max.x - b.min.x),
                b.min.y + y * (b.max.y - b.min.y),
                b.min.z + z * (b.max.z - b.min.z)
            );

            return shape.Move(-shift);
        }

        public static Shape AlignX(this Shape shape, double x)
        {
            var borderX = shape.BorderX;

            var shift = new Vector3(
                borderX.a + x * (borderX.b - borderX.a),
                0,
                0
            );

            return shape.Move(-shift);
        }

        public static Shape AlignY(this Shape shape, double y)
        {
            var borderY = shape.BorderY;

            var shift = new Vector3(
                0,
                borderY.a + y * (borderY.b - borderY.a),
                0
            );

            return shape.Move(-shift);
        }

        public static Shape AlignZ(this Shape shape, double z)
        {
            var borderZ = shape.BorderZ;

            var shift = new Vector3(
                0,
                0,
                borderZ.a + z * (borderZ.b - borderZ.a)
            );

            return shape.Move(-shift);
        }

        public static Shape Perfecto(this Shape shape, double mult = 1) => shape.Centered().Adjust(mult);

        public static Shape Adjust(this Shape shape, double size = 1)
        {
            var s = shape.Size;

            return shape.Mult(size / new[]{ s.x, s.y, s.z}.Max());
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

        public static Graph ToGraph(this Shape shape)
        {
            return new Graph(shape.OrderedEdges);
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

        public static Shape NormedHalf(this Shape shape)
        {
            var r = shape.GetRadius();
            return shape.Mult(0.5 / r);
        }

        public static Shape Normalize(this Shape shape)
        {
            var shapePoints = shape.Points3.Select(p => p.ToV3D()).ToArray();
            var points = shapePoints.OrderSafeDistinct().ToList();
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

        public static Shape ApplyDefaultColor(this Shape shape, Color color)
        {
            if (shape.Materials == null)
                shape.ApplyColor(color);
            else
            {
                foreach (var i in shape.Materials.Index())
                {
                    shape.Materials[i] ??= Materials.GetByColor(color);
                }
            }

            return shape;
        }

        public static Shape ApplyColorGradientX(this Shape shape, params Color?[] colors) => shape.ApplyColorGradient(v => v.x, colors);
        public static Shape ApplyColorGradientX(this Shape shape, Func3Z gradientFn, params Color?[] colors) => shape.ApplyColorGradient(v => gradientFn(v.y,v.z), colors);

        public static Shape ApplyColorGradientY(this Shape shape, params Color?[] colors) => shape.ApplyColorGradient(v => v.y, colors);
        public static Shape ApplyColorGradientY(this Shape shape, Func3Z gradientFn, params Color?[] colors) => shape.ApplyColorGradient(v => gradientFn(v.x,v.z), colors);

        public static Shape ApplyColorGradientZ(this Shape shape, params Color?[] colors) => shape.ApplyColorGradient(v => v.z, colors);
        public static Shape ApplyColorGradientZ(this Shape shape, Func3Z gradientFn, params Color?[] colors) => shape.ApplyColorGradient(v => gradientFn(v.x,v.y), colors);

        public static Shape ApplyColorGradient(this Shape shape, Vector3 a, params Color?[] colors) =>
            ApplyColorGradient(shape, b => a.MultS(b.ToV3()), colors);

        public static Shape ApplyColorSphereGradient(this Shape shape, params Color?[] colors) =>
            ApplyColorGradient(shape, b => b.ToV3().Length, colors);

        public static Shape ApplyColorSphereGradient(this Shape shape, Vector3 center, params Color?[] colors) =>
            ApplyColorGradient(shape, b => (b.ToV3()- center).Length, colors);

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

        public static Shape PullOnSurface(this Shape shape, SurfaceFunc fn, double dxy = 0.0001) => new Shape
        {
            Points3 = shape.Points
                .Select(p => new { fXY = fn(p.x, p.y), f1 = fn(p.x + dxy, p.y), f2 = fn(p.x, p.y + dxy), z = p.z })
                .Select(v => v.fXY + new Plane(v.f1, v.f2, v.fXY).Normal.ToLen(v.z)).ToArray(),
            Convexes = shape.Convexes
        };

        public static Shape PullOnSurface90(this Shape shape, SurfaceFunc fn, double dxy = 0.0001) => new Shape
        {
            Points3 = shape.Points
                .Select(p => new { fXY = fn(p.y, p.x), f1 = fn(p.y + dxy, p.x), f2 = fn(p.y, p.x + dxy), z = p.z })
                .Select(v => v.fXY + new Plane(v.f1, v.f2, v.fXY).Normal.ToLen(v.z)).ToArray(),
            Convexes = shape.Convexes
        };

        // todo: менять плохие треугольники на хорошие
        public static Shape SplitPlanes(this Shape shape, double? minSize = null, int? count = null)
        {
            var k = minSize.HasValue ? (count ?? 20) : (count ?? 5);
            shape = shape.SimpleTriangulateOddPlanes();

            //int[][] GetSplitConvexes(int[] c)
            //{
            //    return new[]
            //    {
            //        new[] {c[5], c[0], c[1]},
            //        new[] {c[1], c[2], c[3]},
            //        new[] {c[3], c[4], c[5]},
            //        new[] {c[1], c[3], c[5]},
            //    };
            //}

            while (k-- != 0)
            {
                var newShape = shape.SplitLongLine(true, minSize);
                if (shape.PointsCount == newShape.PointsCount)
                    break;

                shape = newShape;
                //shape.Convexes = shape.Convexes.SelectMany(GetSplitConvexes).ToArray();
            }

            return shape.Normalize();
        }

        private static Shape SimpleTriangulateOddPlanes(this Shape shape)
        {
            // Odd - нечетные
            // Even - четные
            var convexes =
                shape.Convexes.SelectMany(convex => (convex.Length == 3 || convex.Length.IsEven())
                    ? convex.SelectCircleTriple((i, j, k) => new[] {i, j, k}).Odds()
                    : convex.SelectCircleTriple((i, j, k) => new[] {i, j, k}).Odds()
                        .Concat(new[] {new[] {convex[0], convex[1], convex[3]}})).ToArray();

            return new Shape()
            {
                Points = shape.Points,
                Convexes = convexes
            };
        }

        public static Polygon ToPolygon(this Shape shape) => new Polygon()
        {
            Points = shape.Points2
        };

        public static Shape Smooth(this Shape shape) => new Shape()
        {
            Points3 = shape.Points3.SelectCircleTriple((a,b,c)=>(a+b+c)/3).ToArray(),
            Convexes = shape.Convexes,
            Materials = shape.Materials
        };

        public static Shape CutOutside(this Shape shape, Polygon cutPolygon) => shape.Cut(cutPolygon, true);

        public static Shape Cut(this Shape shape, Polygon cutPolygon, bool needOutside = false)
        {
            var ss = shape.ToShape2().ToSuperShape();
            ss.Cut(cutPolygon, !needOutside);

            return ss.ToShape().ToShape3();
        }

        public static int[][] GetPerimeters(this Shape shape) => PerimeterEngine.FindPerimeter(shape);

        public static Shape[] SplitByConvexes(this Shape shape, bool withVolume = true)
        {
            return shape.Convexes.Select((c,i) => new Shape()
            {
                Points = c.Select(i => shape.Points[i]).ToArray(),
                Convexes = withVolume
                    ? new[]
                    {
                        c.Index().ToArray(),
                        c.Index().Reverse().ToArray()
                    }
                    : new[]
                    {
                        c.Index().ToArray()
                    },
                Materials = shape.Materials == null ? null : (withVolume ? new[]{shape.Materials[i], shape.Materials[i] } : new[] { shape.Materials[i] })
            }).ToArray();
        }

        public static Shape ToPerimeterShape(this Shape shape)
        {
            var perimeter = PerimeterEngine.FindPerimeter(shape);

            return new Shape()
            {
                Points = shape.Points,
                Convexes = perimeter.SelectMany(p => p.SelectCirclePair((i, j) => new[] {i, j})).ToArray()
            };
        }
    }
}
