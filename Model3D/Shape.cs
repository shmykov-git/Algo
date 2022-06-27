using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Model3D;

namespace Model
{
    public class Shape
    {
        public Vector4[] Points;

        public int[][] Convexes;
        public Material[] Materials;
        
        public int PointsCount => Points.Length;
        public IEnumerable<int> PointIndices => Points.Index();
        public IEnumerable<IEnumerable<(int, int)>> ConvexesIndices => Convexes == null ? new (int, int)[0][] : Convexes.Select(convex => convex.Length == 2 ? new[] { (convex[0], convex[1]) } : convex.SelectCirclePair((i, j) => (i, j)));
        public (int, int)[] OrderedEdges => ConvexesIndices.SelectMany(edges => edges.Select(e=>e.OrderedEdge())).Distinct().ToArray();
        public Line3[] Lines3 => OrderedEdges.Select(e => new Line3(Points[e.Item1].ToV3(), Points[e.Item2].ToV3())).ToArray();
        public Line2[] Lines2 => OrderedEdges.Select(e => new Line2(Points[e.Item1].ToV2(), Points[e.Item2].ToV2())).ToArray();

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
            Points = Array.Empty<Vector4>(),
            Convexes = Array.Empty<int[]>()
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
                var bottom = ps.Select(p=>(p,proj: Vector3.YAxis.MultS(p - center))).Where(v => v.proj < 0).OrderBy(v => v.proj).First().p;

                return bottom;
            }
        }

        public (Vector3 top, Vector3 bottom) TopsY
        {
            get
            {
                var ps = Points3;
                var center = ps.Center();
                var top = ps.Where(p => Vector3.YAxis.MultS(p - center) > 0).OrderByDescending(p => (p - center).Length2).First();
                var bottom = ps.Where(p => Vector3.YAxis.MultS(p - center) < 0).OrderByDescending(p => (p - center).Length2).First();

                return (top, bottom);
            }
        }

        public static Shape operator +(Shape a, Shape b)
        {
            return a.Join(b);
        }
    }
}
