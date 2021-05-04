using Model.Extensions;
using Model3D.Extensions;
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
    }
}
