using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
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
        
        public int PointsCount => Points.Length;
        public IEnumerable<int> PointIndices => Points.Index();
        public IEnumerable<IEnumerable<(int, int)>> ConvexesIndices => Convexes == null ? new (int, int)[0][] : Convexes.Select(convex => convex.Length == 2 ? new[] { (convex[0], convex[1]) } : convex.SelectCirclePair((i, j) => (i, j)));
        public (int, int)[] OrderedEdges => ConvexesIndices.SelectMany(edges => edges.Select(e=>e.OrderedEdge())).Distinct().ToArray();
        public Line3[] Lines3 => OrderedEdges.Select(e => new Line3(Points[e.Item1].ToV3(), Points[e.Item2].ToV3())).ToArray();

        public Vector3[] Points3
        {
            get => Points.Select(p => p.ToV3()).ToArray();
            set => Points = value.Select(p => p.ToV4()).ToArray();
        }

        public Vector4[] Normals
        {
            get
            {
                var points = Points3;
                return Convexes.Where(c => c.Length > 2).Select(c => new Plane(points[c[0]], points[c[1]], points[c[2]]).Normal.ToV4()).ToArray();
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

        public Vector3 GetSize()
        {
            var b = GetBorders();

            return new Vector3(b.max.x - b.min.x, b.max.y - b.min.y, b.max.z - b.min.z);
        }

        public (double a, double b) BorderX => (Points.Min(p => p.x), Points.Max(p => p.x));
        public (double a, double b) BorderY => (Points.Min(p => p.y), Points.Max(p => p.y));
        public (double a, double b) BorderZ => (Points.Min(p => p.z), Points.Max(p => p.z));

        public double SizeX => Points.Max(p => p.x) - Points.Min(p => p.x);
        public double SizeY => Points.Max(p => p.y) - Points.Min(p => p.y);
        public double SizeZ => Points.Max(p => p.z) - Points.Min(p => p.z);

        public static Shape operator +(Shape a, Shape b)
        {
            return a.Join(b);
        }
    }
}
