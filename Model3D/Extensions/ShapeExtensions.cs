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

        public static Shape TransformPoints(this Shape shape, Func<Vector3, Vector3> changePointFn)
        {
            return new Shape
            {
                Points3 = shape.Points3.Select(changePointFn).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

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

        public static Shape WhereEllipse(this Shape shape, double a, double b) =>
            shape.Where(v => (v.x / a).Pow2() + (v.y / b).Pow2() < 1);

        public static Shape WhereEllipse4(this Shape shape, double a, double b) =>
            shape.Where(v => (v.x / a).Pow2().Pow2() + (v.y / b).Pow2().Pow2() < 1);

        public static Shape Where(this Shape shape, Func<Vector3, bool> whereFunc)
        {
            var pBi = shape.Points3.WhereBi(whereFunc);

            return new Shape
            {
                Points3 = pBi.items.ToArray(),
                Convexes = shape.Convexes.Transform(i => pBi.bi[i]).CleanBi()
            }.CleanPoints();
        }

        private static Shape CleanPoints(this Shape shape)
        {
            var indices = shape.Convexes.SelectMany(c => c).Distinct().ToHashSet();
            var indBi = shape.Points.Index().WhereBi(i => indices.Contains(i));

            return new Shape()
            {
                Points = indBi.items.Select(i => shape.Points[i]).ToArray(),
                Convexes = shape.Convexes.Transform(i => indBi.bi[i]),
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

        public static Shape AddVolumeX(this Shape shape, double xVolume) => AddVolume(shape, xVolume, MoveX);
        public static Shape AddVolumeY(this Shape shape, double yVolume) => AddVolume(shape, yVolume, MoveY);
        public static Shape AddVolumeZ(this Shape shape, double zVolume) => AddVolume(shape, zVolume, MoveZ);

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
                    perimeters.SelectMany(p=>p.SelectCirclePair((i,j)=>new int[] { j, j + shape.Points.Length, i + shape.Points.Length, i })),
                }.SelectMany(v=>v).ToArray()
            };
        }


        public static Shape AddSphereVolume(this Shape shape, double thicknessMult = 1.1)
        {
            var up = thicknessMult > 0;

            var s = new Shape
            {
                Points3 = shape.Points3.Concat(shape.Points3.Select(p => thicknessMult * p)).ToArray(),

                Convexes = shape.Convexes.SelectMany(convex => new int[][]
                {
                    up ? convex.Reverse().ToArray() : convex,
                    (up ? convex : convex.Reverse()).Select(i => i + shape.Points.Length).ToArray()
                }).ToArray() //.Concat(convex.SelectCirclePair((i, j) => new int[] { i, i + shape.Points.Length, j + shape.Points.Length, j }).ToArray())).ToArray(),
            };

            return s;
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
            var c = s.PointCenter;
            var move = (mult - 1) * c;

            return s.RotateMassCenter(angle).Move(move);
        }).ToSingleShape();

        public static Shape ToRandomBlowedShape(this IEnumerable<Shape> shapeList, double mult = 2, int seed = 0)
        {
            var rnd = new Random(seed);

            return shapeList.Select(s =>
            {
                var c = s.PointCenter;
                var move = (mult - 1) * c;

                return s.RotateMassCenter(2 * Math.PI * rnd.NextDouble()).Move(move);
            }).ToSingleShape();
        }

        // using nothing
        //public static Shape ToSingleShape1(this IEnumerable<Shape> shapeList)
        //{
        //    var shapes = shapeList.ToArray();

        //    while (shapes.Length > 1)
        //    {
        //        shapes = shapes.SelectByPair((a, b) => a + (b ?? Shape.Empty)).ToArray();
        //    }

        //    return shapes[0];
        //}

        // using tasks
        //public static Shape ToSingleShape2(this IEnumerable<Shape> shapeList)
        //{
        //    var shapes = shapeList.ToArray();

        //    if (shapes.Length == 0)
        //        return Shape.Empty;

        //    while (shapes.Length > 1)
        //    {
        //        var tasks = shapes.SelectByPair((a, b) => Task.Run(()=>a + (b ?? Shape.Empty))).ToArray();
        //        //Debug.WriteLine(tasks.Length);
        //        Task.WhenAll(tasks).Wait();
        //        shapes = tasks.Select(t => t.Result).ToArray();
        //    }

        //    return shapes[0];
        //}

        // using algo threadpool
        public static Shape ToSingleShape(this IEnumerable<Shape> shapeList)
        {
            var shapes = shapeList.ToArray();

            if (shapes.Length == 0)
                return Shape.Empty;

            while (shapes.Length > 1)
            {
                shapes = shapes.SelectByPair((a, b) => (a, b: b ?? Shape.Empty)).ToArray()
                    .SelectInParallel(v => v.a + v.b);
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

            var points = shape.Points3;
            var dic = points.Select((p, i) => (p, i)).GroupBy(v => v.p).ToDictionary(gp => gp.Key, gp => 0);
            
            var textSize = 100;

            foreach (var (i, p) in shape.Points3.IndexValue())
            {
                var k = dic[p]++;
                var iText = vectorizer.GetText(i.ToString(), textSize, scale:false).Mult(0.1*mult/textSize).Move(p).Move(mult*new Vector3(0, 0.11 * k, 0)).ApplyColor(numColor.Value);
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

        public static Shape ToLines(this Shape shape, double mult = 1, Color? color = null, bool direct = false) =>
            direct
                ? shape.ToDirectLines(mult, color)
                : shape.ToLines3WithMaterial(mult, color.HasValue ? Materials.GetByColor(color.Value) : null);

        public static Shape ToDirectLines(this Shape shape, double mult = 1, Color? color = null)
        {
            var lineShape = Surfaces.Cylinder(5, 2) + Surfaces.ConeM(5, 2).Scale(3,3,0.2).MoveZ(0.6).WithBackPlanes();

            return shape.ToLines3WithMaterial(mult, color.HasValue ? Materials.GetByColor(color.Value) : null, true, lineShape, true);
        }

        public static Shape ToShapedLines(this Shape shape, Shape lineShape, double mult = 1, Color? color = null) => shape.ToLines3WithMaterial(mult, color.HasValue ? Materials.GetByColor(color.Value) : null, true, lineShape);

        public static Shape ToLines3WithMaterial(this Shape shape, double mult = 1, Material material = null, bool stretch = true, Shape lineShape = null, bool direct = false)
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

            var lines = direct ? shape.Edges.Select(GetLine).ToArray() : shape.OrderedEdges.Select(GetLine).ToArray();

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

        public static Shape AddBorder(this Shape shape, double b)
        {
            var border = shape.GetBorders();
            var size = border.max - border.min;

            return shape.Move(-border.min).Scale(1.0 + 2 * b / size.x, 1.0 + 2 * b / size.y, 1.0 + 2 * b / size.z)
                .Move(border.min - b * new Vector3(1, 1, 1));
        }

        public static Shape ResizeByNormals(this Shape shape, double distance)
        {
            var ps = shape.Points3;

            Vector3 GetN(int[] c) => new Plane(ps[c[0]], ps[c[1]], ps[c[2]]).NOne;
            Vector3 GetNP(IEnumerable<Vector3> vs) => vs.Center().ToLenWithCheck(ln => distance / ln);

            var psMoves = shape.Convexes.SelectMany(c => c.Select(i => (i, c))).GroupBy(v => v.i).Select(gv =>
                    (i: gv.Key,
                        n: GetNP(gv.Select(v => GetN(v.c)))))
                .OrderBy(v => v.i).ToDictionary(v=>v.i, v => v.n);

            return new Shape()
            {
                Points3 = ps.Select((p, i) => p + (psMoves.TryGetValue(i, out Vector3 n) ? n : Vector3.Origin)).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape Mult(this Shape shape, double k)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(k * p.x, k * p.y, k * p.z, p.w)).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }
        
        public static Shape Multiplicate(this Shape shape, Func<Shape, Shape>[] funcs)
        {
            return funcs.Select(fn => fn(shape)).ToSingleShape();
        }

        public static Shape AddVolume(this Shape shape, double distance, Func<Shape, double, Shape> moveFn, bool centered = true)
        {
            var up = distance > 0;
            var ln = shape.Points.Length;

            var moveA = centered ? -distance / 2 : 0;
            var moveB = centered ? distance / 2 : distance;
            var aShape = moveFn(shape, moveA);
            var bShape = moveFn(shape, moveB);

            var edges = shape.Convexes
                .SelectMany((c, cI) => c.SelectCirclePair((i, j) => (cI, e: (i, j), oe: (i, j).OrderedEdge())))
                .GroupBy(v => v.oe).Where(gv => gv.Count() == 1).Select(gv => gv.First()).ToArray();

            Material[] materials = null;
            if (shape.Materials != null)
            {
                materials = new[]
                {
                    up ? shape.Materials.Reverse() : shape.Materials,
                    up ? shape.Materials : shape.Materials.Reverse(),
                    up ? edges.Select(e=>shape.Materials[e.cI]) : edges.Select(e=>shape.Materials[e.cI]).Reverse()
                }.ManyToArray();
            }

            return new Shape()
            {
                Points = aShape.Points.Concat(bShape.Points).ToArray(),
                Convexes = new[]
                {
                    shape.Convexes.ReverseConvexes(up),
                    bShape.Convexes.Transform(i => i + ln).ReverseConvexes(!up),
                    edges.Select(e => new[] {e.e.i, e.e.j, e.e.j + ln, e.e.i + ln}).ReverseConvexes(!up)
                }.ManyToArray(),
                Materials = materials
            };
        }
        public static Shape AddNormalVolume(this Shape shape, double distance)
        {
            var up = distance > 0;
            var ln = shape.Points.Length;

            var sizedShape = shape.ResizeByNormals(distance);

            var edges = shape.Convexes
                .SelectMany(c => c.SelectCirclePair((i, j) => (e: (i, j), oe: (i, j).OrderedEdge())))
                .GroupBy(v => v.oe).Where(gv => gv.Count() == 1).Select(gv => gv.First()).ToArray();

            return new Shape()
            {
                Points = shape.Points.Concat(sizedShape.Points).ToArray(),
                Convexes = new[]
                {
                    shape.Convexes.ReverseConvexes(up),
                    sizedShape.Convexes.Transform(i => i + ln).ReverseConvexes(!up),
                    edges.Select(e => new[] {e.e.i, e.e.j, e.e.j + ln, e.e.i + ln}).ReverseConvexes(!up)
                }.ManyToArray()
            };
        }

        public static Shape ScaleXY(this Shape shape, double multX, double multY) => Scale(shape, multX, multY, 1);
        public static Shape ScaleX(this Shape shape, double mult) => Scale(shape, mult, 1, 1);
        public static Shape ScaleY(this Shape shape, double mult) => Scale(shape, 1, mult, 1);
        public static Shape ScaleZ(this Shape shape, double mult) => Scale(shape, 1, 1, mult);

        public static Shape Scale(this Shape shape, Vector3 v) => shape.Scale(v.x, v.y, v.z);
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

        public static Shape Move(this Shape shape, Func<Vector3, Vector3> moveFn)
        {
            return new Shape
            {
                Points3 = shape.Points3.Select(moveFn).ToArray(),
                Convexes = shape.Convexes,
                Materials = shape.Materials
            };
        }

        public static Shape ToOx(this Shape shape) => shape.Rotate(Rotates.Z_X);
        public static Shape ToOxM(this Shape shape) => shape.Rotate(Rotates.Z_mX);
        public static Shape ToOy(this Shape shape) => shape.Rotate(Rotates.Z_Y);
        public static Shape ToOyM(this Shape shape) => shape.Rotate(Rotates.Z_mY);
        public static Shape OxToOy(this Shape shape) => shape.Rotate(Rotates.X_Y);

        public static Shape RotateY(this Shape shape, Vector3 v) =>
            shape.Rotate(Quaternion.FromRotation(Vector3.YAxis, v));

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
        public static Shape RotateOy(this Shape shape, double alfa) => Rotate(shape, Quaternion.FromAngleAxis(alfa, Vector3.YAxis));
        public static Shape Rotate(this Shape shape, Vector3 center, double alfa) => shape.Move(-center).Rotate(Quaternion.FromAngleAxis(alfa, Vector3.ZAxis)).Move(center);
        public static Shape RotateMassCenter(this Shape shape, double alfa) => shape.Rotate(shape.PointCenter, alfa);
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

        public static Shape PutOn(this Shape shape, double y = 0) => shape.Align(0.5, 0, 0.5).MoveY(y);
        public static Shape PutUnder(this Shape shape, double y = 0) => shape.Align(0.5, 1, 0.5).MoveY(y);

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
            var bi = shape.Points3.Select(p => p.ToV3D()).ToArray().DistinctBi();
            
            var points = shape.Points.Where((_, i) => bi.filter[i]).ToArray();
            var convexes = shape.Convexes.Transform(i => bi.bi[i])
                .Select(convex => convex.OrderSafeDistinct().ToArray())
                .Where(convex => convex.Length >= 3).ToArray();

            return new Shape()
            {
                Points = points, 
                Convexes = convexes
            };
        }

        public static Shape ApplyMaterial(this Shape shape, Material material, Func<Vector3, bool> filterFn = null) =>
            shape.ApplyMaterial(material == null ? null : _ => material, filterFn);

        public static Shape ApplyMaterial(this Shape shape, Func<int[], Material> materialFn, Func<Vector3, bool> filterFn = null)
        {
            if (materialFn == null)
                return shape;

            if (filterFn == null)
            {
                shape.Materials = shape.Convexes.Select(materialFn).ToArray();

                return shape;
            }

            var ps = shape.Points3;
            var cs = shape.Convexes;

            if (shape.Materials == null)
                shape.Materials = shape.Convexes.Select(c => c.All(i => filterFn(ps[i])) ? materialFn(c) : null).ToArray();
            else
                shape.Materials
                    .ToArray().ForEach((m, i) => shape.Materials[i] = cs[i].All(j => filterFn(ps[j])) ? materialFn(cs[i]) : m);

            return shape;
        }

        public static Shape ApplyColor(this Shape shape, Color color, Func<Vector3, bool> filterFn = null)
        {
            shape.ApplyMaterial(Materials.GetByColor(color), filterFn);

            return shape;
        }

        public static Shape ApplyColor(this Shape shape, Func<int[], Color> colorFn)
        {
            shape.ApplyMaterial(c=>Materials.GetByColor(colorFn(c)));

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

        public static Shape ApplyColorSphereRGradient(this Shape shape, double radius, Vector3 center, params Color?[] colors) =>
            ApplyColorGradient(shape, b => (b.ToV3() - center).Length, 0, radius, colors);

        private static Shape ApplyColorGradient(this Shape shape, Func<Vector4, double> valueFn, params Color?[] colors) =>
            shape.ApplyColorGradient(valueFn, null, null, colors);

        private static Shape ApplyColorGradient(this Shape shape, Func<Vector4, double> valueFn, double? from, double? to, params Color?[] colors)
        {
            var centers = shape.Convexes.Select(convex => convex.Select(i => shape.Points[i]).Center()).ToArray();

            var min = from??centers.Min(valueFn);
            var max = to??centers.Max(valueFn);
            max += (max - min) * 0.00001;

            Vector4 GetColor(double z, Color current)
            {
                var n = colors.Length - 1;
                var k = (z - min) / (max - min); // [0, 1)
                var j = (int)(k * n);
                if (j >= colors.Length-1)
                    j = colors.Length - 2;
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

        public static Shape FilterPlanes(this Shape shape, Func<Plane, bool> filterFn)
        {
            var ps = shape.Points3;

            return new Shape()
            {
                Points = shape.Points,
                Convexes = shape.Convexes.Where(c => filterFn(new Plane(ps[c[0]], ps[c[1]], ps[c[2]]))).ToArray(),
            }.CleanPoints();
        }

        public static Shape FilterConvexPlanes(this Shape shape, Func<Vector3[], Plane, bool> filterFn)
        {
            var ps = shape.Points3;

            return new Shape()
            {
                Points = shape.Points,
                Convexes = shape.Convexes.Where(c => filterFn(c.Select(i=>ps[i]).ToArray(), new Plane(ps[c[0]], ps[c[1]], ps[c[2]]))).ToArray(),
            }.CleanPoints();
        }

        public static Shape FilterConvexes(this Shape shape, Func<int[], bool> filterFn)
        {
            return new Shape()
            {
                Points = shape.Points,
                Convexes = shape.Convexes.Where(filterFn).ToArray(),
            }.CleanPoints();
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

        public static Polygon ToPolygonByConvexes(this Shape shape)
        {
            var inds = shape.Convexes.SelectMany(c => c.SkipLast(1)).ToArray();
            var ps = shape.Points2;

            return new Polygon()
            {
                Points = inds.Select(i=>ps[i]).ToArray()
            };
        }

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

        public static Shape ToPerimeterShape(this Shape shape, int? filterPointsCount = null)
        {
            var perimeters = PerimeterEngine.FindPerimeter(shape);

            return new Shape()
            {
                Points = shape.Points,
                Convexes = perimeters
                    .Where(p => !filterPointsCount.HasValue || p.Length >= filterPointsCount.Value)
                    .SelectMany(p => p.SelectCirclePair((i, j) => new[] {i, j})).ToArray()
            };
        }

        public static Polygon[] ToPerimeterPolygons(this Shape shape, int? filterPointsCount = null)
        {
            var ps = shape.Points2;
            var perimeters = PerimeterEngine.FindPerimeter(shape);

            return perimeters
                .Where(p => !filterPointsCount.HasValue || p.Length >= filterPointsCount.Value)
                .Select(p => new Polygon()
                {
                    Points = p.Select(i => ps[i]).ToArray()
                }.ToLeft()).ToArray();
        }

        public static Shape MovePlanes(this Shape shape, double distance, double mult = 1)
        {
            return shape.SplitByConvexes(false).Select(s =>
            {
                var c = s.PointCenter;

                return s.Move(-c).Mult(mult).Move(c).Move(s.Normals[0].ToLen(distance));
            }).ToSingleShape();
        }

        public static Shape ReversePlanes(this Shape shape, bool needReverse = true)
        {
            return new Shape()
            {
                Points = shape.Points,
                Convexes = needReverse ? shape.Convexes.ReverseConvexes().ToArray() : shape.Convexes,
                Materials = shape.Materials,
            };
        }

        public static Shape WithBackPlanes(this Shape shape, Color? color = null)
        {
            return new Shape()
            {
                Points = shape.Points,
                Convexes = new[]
                {
                    shape.Convexes,
                    shape.Convexes.ReverseConvexes()
                }.ManyToArray(),
                Materials = shape.Materials == null
                    ? null
                    : new[]
                    {
                        shape.Materials,
                        color == null ? shape.Materials : shape.Materials.Select(_=>Materials.GetByColor(color.Value))
                    }.ManyToArray(),
            };
        }

        public static bool IsInside(this Shape shape, Vector3 p)
        {
            return shape.Planes
                .Select(points => new Plane(points[0], points[1], points[2]).Fn(p) < 0 ? -1 : 1)
                .Sum()
                .Abs() == shape.Convexes.Length;
        }

        public static Shape RotateToTopY(this Shape shape) => shape.RotateToTopY(out _);

        public static Shape RotateToTopY(this Shape shape, out Quaternion q)
        {
            var (top, bottom) = shape.TopsY;

            q = Quaternion.FromRotation((top - bottom).Normalize(), Vector3.YAxis);

            return shape.Rotate(q);
        }

        public static Shape RotateToMassY(this Shape shape) => shape.RotateToMassY(out _);

        public static Shape RotateToMassY(this Shape shape, out Quaternion q)
        {
            var massCenter = shape.MassCenter;
            var bottom = shape.BottomY;

            q = Quaternion.FromRotation((massCenter - bottom).Normalize(), Vector3.YAxis);

            return shape.Rotate(q);
        }

        public static Shape ToStone(this Shape shape, int n = 4, int seed = 0, double power = 1)
        {
            var rnd = new Random(seed);

            var dirs = (n).SelectRange(_ => rnd.NextCenteredV3().Normalize()).ToArray();

            Vector3 TransformFn(Vector3 v)
            {
                return v + power * dirs.Select(dir =>
                {
                    var m = v.MultS(dir);

                    if (m < 0)
                        return Vector3.Origin;

                    return m * dir;
                }).Sum();
            }

            return shape.TransformPoints(TransformFn);
        }
    }
}
