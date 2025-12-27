using Model.Extensions;
using Model.Graphs;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class Shape
    {
        public Vector4[] Points;

        public int[][] Convexes;
        public Material[] Materials;
        public Vector2[][] TexturePoints;
        public List<Shape> Shapes = new();
        public MasterPoint[] MasterPoints = [];

        private Dictionary<int, int[]> _links;

        //public Shape()
        //{
        //    Task.Run(async () =>
        //    {
        //        await Task.Delay(10);

        //        if (CompositeShapes.Any(s => !s.HasSingleMaterial || (s.Convexes ?? []).Select(c => c.Length).Distinct().Count() != 1))
        //        {
        //            var debug = CompositeShapes.Select(s => $"{s.PointsCount}-({s.Convexes?.GroupBy(c => c.Length).Select(gc => $"{gc.Key}-{gc.Count()}").SJoin(",") ?? "0"}-{s.Materials?.Distinct().Count() ?? 0})").SJoin(", ");
        //            Debug.WriteLine(debug);
        //        }
        //    });
        //}

        public IEnumerable<Shape> CompositeShapes
        {
            get
            {
                if (!IsRootEmpty)
                    yield return this;

                foreach (var shape in Shapes.SelectMany(s => s.CompositeShapes))
                    yield return shape;
            }
        }

        public IEnumerable<Shape> CompositeMaterialShapes
        {
            get
            {
                foreach (var s in CompositeShapes)
                {
                    if (s.HasSingleMaterial)
                        yield return s;
                    else
                        foreach (var sM in s.SplitByMaterial())
                        {
                            yield return sM;
                        }
                }
            }
        }

        public bool IsComposite => Shapes.Count > 0;
        public bool IsRootEmpty => (Points?.Length ?? 0) == 0;
        public bool IsEmpty => IsRootEmpty && !IsComposite;
        public bool HasMasterPoints => MasterPoints.Length > 0;
        public bool HasSingleMaterial => (Materials?.Distinct().Count() ?? 0) <= 1;
        public int PointsCount => Points.Length;
        public IEnumerable<int> PointIndices => Points.Index();
        public IEnumerable<IEnumerable<(int, int)>> ConvexesIndices => Convexes == null ? [] : Convexes.Select(convex => convex.Length == 2 ? [(convex[0], convex[1])] : convex.SelectCirclePair((i, j) => (i, j)));
        public (int i, int j)[] Edges => ConvexesIndices.SelectMany(edges => edges).ToArray();
        public (int i, int j)[] OrderedEdges => ConvexesIndices.SelectMany(edges => edges.Select(e => e.OrderedEdge())).Distinct().ToArray();
        public Dictionary<int, int[]> Links => _links ??= ConvexesIndices.SelectMany(vs => vs.SelectMany(v => new[] { v, v.ReversedEdge() })).Distinct().GroupBy(v => v.Item1).ToDictionary(vv => vv.Key, vv => vv.Select(v => v.Item2).ToArray());
        public Line3[] Lines3 => OrderedEdges.Select(e => new Line3(Points[e.Item1].ToV3(), Points[e.Item2].ToV3())).ToArray();
        public Line2[] Lines2 => OrderedEdges.Select(e => new Line2(Points[e.Item1].ToV2(), Points[e.Item2].ToV2())).ToArray();

        public Shape Clone()
        {
            var clone = new Shape()
            {
                Points = Points.ToArray(),
                Convexes = Convexes.ToArray(),
                Materials = Materials?.ToArray(),
                TexturePoints = TexturePoints?.ToArray(),
                MasterPoints = MasterPoints?.ToArray(),
            };

            if (IsComposite)
                clone.Shapes = Shapes.Select(s => s.Clone()).ToList();

            return clone;
        }

        public Graph EdgeGraph => new Graph(OrderedEdges);
        public Graph DirectEdgeGraph => new Graph(Edges);

        public Graph ConvexGraph
        {
            get
            {
                var nodes = Convexes.Select((c, i) => new ConvexNode { i = i, edges = c.SelectCirclePair((i, j) => (i, j).OrderedEdge()).ToArray() }).ToArray();

                void SetNodes(ConvexNode[] ns)
                {
                    ns.ForEach(a => ns.Where(b => a != b).ForEach(b => a.ns.Add(b)));
                }

                nodes.SelectMany(n => n.edges.Select(e => (n, e)))
                    .GroupBy(v => v.e)
                    .ForEach(vg => SetNodes(vg.Select(v => v.n).Distinct().ToArray()));

                var edges = nodes.SelectMany(a => a.ns.Select(b => (a.i, b.i).OrderedEdge())).Distinct().ToArray();

                return new Graph(Convexes.Length, edges);
            }
        }

        public double AverageEdgeLength { get { var ps = Points3; var oEdges = OrderedEdges; return oEdges.Length > 0 ? OrderedEdges.Select(e => (ps[e.i] - ps[e.j]).Length).Average() : 0; } }

        public Vector3[][] Planes
        {
            get
            {
                var ps = Points3;

                return Convexes.Select(c => c.Select(i => ps[i]).ToArray()).ToArray();
            }
        }

        public Vector3[] Points3
        {
            get => Points.Select(p => p.ToV3()).ToArray();
            set => Points = value.Select(p => p.ToV4()).ToArray();
        }

        public static IEnumerable<int> TriangleSchemaList(int count) => (count - 1)
            .SelectRange(i => i + 1).SelectPair()
            .SelectMany(v => new[] { 0, v.a, v.b });

        public static IEnumerable<(int a, int b, int c)> TriangleSchema(int count) => (count - 1)
            .SelectRange(i => i + 1).SelectPair()
            .Select(v => (0, v.a, v.b));

        public IEnumerable<int> Triangles => Convexes.SelectMany(c => TriangleSchemaList(c.Length).Select(i => c[i]));
        public int[][] ConvexTriangles => Convexes.SelectMany(c => TriangleSchema(c.Length).Select(v => new[] { c[v.a], c[v.b], c[v.c] })).ToArray();

        public (Material m, int[] ts)[] TrianglesWithMaterials => Convexes
            .Select((c, cI) => (t: TriangleSchemaList(c.Length).Select(i => c[i]).ToArray(), m: Materials[cI]))
            .GroupBy(v => v.m)
            .Select(gv => (gv.Key, gv.SelectMany(v => v.t).ToArray()))
            .ToArray();

        public IEnumerable<Vector2> TriangleTexturePoints => TexturePoints?.SelectMany(c => TriangleSchemaList(c.Length).Select(i => c[i]));

        // todo: check
        public Vector3[] PointNormals
        {
            get
            {
                var ps = Points3;

                var normals = Convexes.SelectMany(c => TriangleSchema(c.Length)).Select(t => (t, n: new Plane(ps[t.a], ps[t.b], ps[t.c]).NOne))
                    .SelectMany(t => new[] { (t.t, t.n, k: t.t.a), (t.t, t.n, k: t.t.b), (t.t, t.n, k: t.t.c) })
                    .GroupBy(v => v.k)
                    .OrderBy(gv => gv.Key)
                    .Select(gv => -gv.Select(v => v.n).Center())
                    .ToArray();

                return normals;
            }
        }

        public Vector3[] Normals
        {
            get
            {
                var points = Points3;
                return Convexes.Where(c => c.Length > 2).Select(c => new Plane(points[c[0]], points[c[1]], points[c[2]]).Normal).ToArray();
            }
        }

        public Vector2[] Points2
        {
            get => Points.Select(p => p.ToV2()).ToArray();
            set => Points = value.Select(p => p.ToV4()).ToArray();
        }

        public static Shape Empty => new Shape()
        {
            Points = [],
            Convexes = []
        };

        public double GetRadius()
        {
            var points = Points3;
            var center = points.Center();

            return points.Max(p => (p - center).Length);
        }

        public (Vector3 min, Vector3 max) GetBorders() => (
            new Vector3(Points.Min(p => p.x), Points.Min(p => p.y), Points.Min(p => p.z)),
            new Vector3(Points.Max(p => p.x), Points.Max(p => p.y), Points.Max(p => p.z)));

        public Vector3 Size
        {
            get
            {
                var b = GetBorders();

                return new Vector3(b.max.x - b.min.x, b.max.y - b.min.y, b.max.z - b.min.z);
            }
        }

        public Vector3 PointCenter => Points3.Center();

        public Vector3 SizeCenter
        {
            get
            {
                var bX = BorderX;
                var bY = BorderY;
                var bZ = BorderZ;

                return new Vector3(.5 * (bX.a + bX.b), .5 * (bY.a + bY.b), .5 * (bZ.a + bZ.b));
            }
        }

        public (double a, double b) BorderX => (Points.Length == 0 ? 0 : Points.Min(p => p.x), Points.Length == 0 ? 0 : Points.Max(p => p.x));
        public (double a, double b) BorderY => (Points.Length == 0 ? 0 : Points.Min(p => p.y), Points.Length == 0 ? 0 : Points.Max(p => p.y));
        public (double a, double b) BorderZ => (Points.Length == 0 ? 0 : Points.Min(p => p.z), Points.Length == 0 ? 0 : Points.Max(p => p.z));

        public double SizeX => Points.Length == 0 ? 0 : Points.Max(p => p.x) - Points.Min(p => p.x);
        public double SizeY => Points.Length == 0 ? 0 : Points.Max(p => p.y) - Points.Min(p => p.y);
        public double SizeZ => Points.Length == 0 ? 0 : Points.Max(p => p.z) - Points.Min(p => p.z);


        public double[] Masses
        {
            get
            {
                var ps = Points3;

                double GetConvexArea(int[] c)
                {
                    if (c.Length < 3)
                        return 0;

                    var a = ps[c[1]] - ps[c[0]];
                    var b = ps[c[2]] - ps[c[1]];

                    return 0.5 * a.MultV(b).Length;
                }

                var masses = Convexes.SelectMany(c => c.Select(i => (i, c))).GroupBy(v => v.i)
                    .Select(gv => (i: gv.Key, m: gv.Select(v => GetConvexArea(v.c)).Average())).OrderBy(v => v.i)
                    .Select(v => v.m).ToArray();

                var avg = masses.Average();

                return masses.Select(m => m / avg).ToArray();
            }
        }

        public Vector3 MassCenter
        {
            get
            {
                var ms = Masses;

                return Points3.Select((p, i) => ms[i] * p).Center();
            }
        }

        public Vector3 TopY
        {
            get
            {
                var ps = Points3;
                var center = ps.Center();
                var top = ps.Where(p => Vector3.YAxis.MultS(p - center) > 0).OrderByDescending(p => (p - center).Length2).First();

                return top;
            }
        }

        public Vector3 BottomY
        {
            get
            {
                var ps = Points3;
                var center = ps.Center();
                var bottom = ps.Where(p => Vector3.YAxis.MultS(p - center) < 0).OrderByDescending(p => (p - center).Length2).First();

                return bottom;
            }
        }

        public Vector3 ProjectionBottomY
        {
            get
            {
                var ps = Points3;
                var center = ps.Center();
                var bottom = ps.Select(p => (p, proj: Vector3.YAxis.MultS(p - center))).Where(v => v.proj < 0).OrderBy(v => v.proj).First().p;

                return bottom;
            }
        }

        public (Vector3 top, Vector3 bottom) TopsY
        {
            get
            {
                var ps = Points3;
                var center = ps.Center();
                var top = ps.Where(p => Vector3.YAxis.MultS(p - center) >= 0).OrderByDescending(p => (p - center).Length2).First();
                var bottom = ps.Where(p => Vector3.YAxis.MultS(p - center) <= 0).OrderByDescending(p => (p - center).Length2).First();

                return (top, bottom);
            }
        }

        public double Volume0
        {
            get
            {
                var ps = Points3;
                return Convexes.SelectMany(c => c.SelectCircleTriple((i, j, k) => ps[i].GetVolume0(ps[j], ps[k]))).Sum();
            }
        }

        public bool IsD3 => (Volume0 - this.Move(ExVector3.XyzAxis).Volume0).Abs() < Values.Epsilon9;

        public static Shape operator +(Shape a, Shape b)
        {
            return a.Join(b);
        }

        public Vector3[] ConvexPoints(int iConvex) => Convexes[iConvex].Select(i => Points[i].ToV3()).ToArray();

        public Vector3 ConvexNormal(int iConvex)
        {
            var ps = ConvexPoints(iConvex);

            return (ps[0] - ps[2]).MultV(ps[1] - ps[2]);
        }

        public Func<Vector3, Vector3> ProjectionFn(int iConvex)
        {
            var ps = ConvexPoints(iConvex);
            var n = ConvexNormal(iConvex);

            return x => x - n * (n.MultS(x - ps[2]) / n.Length2);
        }

        public Func<Vector3, Vector3, Vector3?> IntersectConvexFn(int iConvex)
        {
            var insideFn = IsInsideConvexFn(iConvex);
            var ps = ConvexPoints(iConvex);
            var intersectFn = new Plane(ps[0], ps[1], ps[2]).IntersectionFn;

            return (a, b) =>
            {
                var p = intersectFn(a, b);
                return p.HasValue && insideFn(p.Value) ? p : null;
            };
        }

        public Func<Vector3, bool> IsInsideConvexFn(int iConvex)
        {
            var ps = ConvexPoints(iConvex);
            var n = ConvexNormal(iConvex);

            return x => ps.SelectCirclePair((a, b) => (a - x).MultS((b - a).MultV(n)).Sgn()).Sum().Abs() == ps.Length;
        }

        public Func<Vector3, double> ConvexDistanceFn(int iConvex)
        {
            var ps = ConvexPoints(iConvex);
            var n = ConvexNormal(iConvex).Normalize();

            return x => n.MultS(x - ps[2]);
        }

        private class ConvexNode
        {
            public int i;
            public (int, int)[] edges;
            public HashSet<ConvexNode> ns = new HashSet<ConvexNode>();
        }

        public class MasterPoint : IEquatable<MasterPoint>
        {
            public Vector4 point;
            public int[] links;

            public override int GetHashCode()
            {
                return point.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as MasterPoint);
            }

            public bool Equals(MasterPoint other)
            {
                return point == other.point;
            }
        }
    }
}
