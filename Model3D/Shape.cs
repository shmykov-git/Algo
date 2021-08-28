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
        public IEnumerable<IEnumerable<(int, int)>> ConvexesIndices => Convexes.Select(convex => convex.SelectCirclePair((i, j) => (i, j)));
        public (int, int)[] OrderedEdges => ConvexesIndices.SelectMany(edges => edges.Select(e=>e.OrderedEdge())).Distinct().ToArray();
        public Line3[] Lines3 => OrderedEdges.Select(e => new Line3(Points[e.Item1].ToV3(), Points[e.Item2].ToV3())).ToArray();

        public Vector3[] Points3
        {
            get => Points.Select(p => p.ToV3()).ToArray();
            set => Points = value.Select(p => p.ToV4()).ToArray();
        }

        public Vector2[] Points2
        {
            get => Points.Select(p => p.ToV2()).ToArray();
            set => Points = value.Select(p => p.ToV4()).ToArray();
        }

        public double GetRadius()
        {
            var points = Points3;
            var center = points.Center();

            return points.Max(p => (p - center).Length);
        }

        public static Shape operator +(Shape a, Shape b)
        {
            return a.Join(b);
        }
    }
}
