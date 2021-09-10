using Model;
using Model.Extensions;
using Model.Graph;
using Model3D.Extensions;
using System.Linq;

namespace Model3D.Tools
{
    public static class Mazerator
    {
        public static Shape MakeMazeXY(Shape shape, int seed = 0)
        {
            var points = shape.Points2;

            (int i, int j)? GetBound((int i, int j)[] a, (int i, int j)[] b)
            {
                return a.SelectMany(v1 => b.Select(v2 => (v1, v2))).Where(v => v.v1 == v.v2).Select(v => ((int, int)?)v.v1).FirstOrDefault();
            };

            bool IsHole((int i, int j)[] holes, int i, int j)
            {
                return holes.Any(v => (v.i == i && v.j == j) || (v.i == j && v.j == i));
            }

            var items = shape.Convexes.Index().Select(i =>
            {
                var convex = shape.Convexes[i];
                var center = convex.Select(i => points[i]).Center();

                return new
                {
                    i,
                    convex = convex,
                    set = convex.SelectCirclePair((i, j) => i > j ? (i: j, j: i) : (i, j)).OrderBy(v => v.i).ThenBy(v => v.j).ToArray(),
                    center = center,
                    radius = convex.Select(i => (points[i] - center).Len).Max()
                };
            }).ToArray();

            var net = new Net<Vector2, int>(items.Select(v => (v.center, v.i)), 6 * items.Max(v => v.radius));

            var nodes = items.Select(a => new
            {
                a.i,
                a.convex,
                a.set,
                a.center,
                edges = net.SelectNeighbors(a.center)
                            .Select(i => items[i])
                            .Where(b => b != a)
                            .Select(b => (b, bound: GetBound(a.set, b.set)))
                            .Where(v => v.bound.HasValue)
                            .Select(v => (e: a.i < v.b.i ? (i: a.i, j: v.b.i) : (i: v.b.i, j: a.i), bound: v.bound.Value))
                            .ToArray()
            }).ToArray();

            var g = new Graph(nodes.SelectMany(n => n.edges.Select(e => e.e)).Distinct());
            var holes = g.RandomVisitEdges(seed).Select(e => e.e).ToArray();

            var bounds = holes.SelectMany(h => nodes[h.i].edges.Select(e => e.bound).Intersect(nodes[h.j].edges.Select(e => e.bound))).ToList();

            var n = shape.Points.Length;

            //bounds = bounds.Concat(new[] { (6, 7), (n - 6, n - 5) }).ToList(); // exits

            var mazeShape = new Shape()
            {
                Points = shape.Points,
                Convexes = nodes.SelectMany(n => n.set.Where(e => !bounds.Contains(e))).Distinct().Select(e => new[] { e.i, e.j }).ToArray()
            };

            return mazeShape;
        }
    }
}
