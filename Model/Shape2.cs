using Model.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class Shape2
    {
        public int[][] Convexes;
        public Vector2[] Points;

        public Vector2 this[int i] => Points[i];

        public IEnumerable<int> PointIndices => Points.Index();
        public IEnumerable<IEnumerable<(int, int)>> ConvexesIndices => Convexes.Select(convex => convex.SelectCirclePair((i, j) => (i, j)));
        public (int, int)[] OrderedEdges => ConvexesIndices.SelectMany(edges => edges.Select(e => e.OrderedEdge())).Distinct().ToArray();
        public Line2[] Lines => OrderedEdges.Select(e => new Line2(Points[e.Item1], Points[e.Item2])).ToArray();
        public int Length => Points.Length;

        public (Vector2 min, Vector2 max) GetBorders() => (
            new Vector2(Points.Min(p => p.x), Points.Min(p => p.y)),
            new Vector2(Points.Max(p => p.x), Points.Max(p => p.y)));

        public Vector2 Size
        {
            get
            {
                var b = GetBorders();

                return new Vector2(b.max.x - b.min.x, b.max.y - b.min.y);
            }
        }
    }
}
