using Model.Extensions;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class Shape2
    {
        public Polygon Polygon = new Polygon();
        public int[][] Convexes;
        public bool IsValid;

        public bool IsFilled => Convexes != null;
        public Vector2[] Points
        {
            get => Polygon.Points;
            set => Polygon.Points = value;
        }

        public IEnumerable<int> PointIndices => Points.Index();
        public IEnumerable<IEnumerable<(int, int)>> ConvexesIndices => Convexes.Select(convex => convex.SelectCirclePair((i, j) => (i, j)));
        public (int, int)[] OrderedEdges => ConvexesIndices.SelectMany(edges => edges.Select(e => e.OrderedEdge())).Distinct().ToArray();
        public Line2[] Lines => OrderedEdges.Select(e => new Line2(Points[e.Item1], Points[e.Item2])).ToArray();

        public Shape2 ModifyPolygon(Polygon polygon) => new Shape2
        {
            Polygon = polygon,
            IsValid = this.IsValid,
            Convexes = this.Convexes
        };
    }
}
