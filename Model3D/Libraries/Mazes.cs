using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Graph;
using Model.Libraries;
using Model3D.Extensions;
using System.Linq;

namespace Model3D
{
    public static class Mazes
    {
        public static Shape CrateKershner8Maze(double tileLen, double angleD, double rotationAngle, int seed = 0)
        {
            var s = Parquets.PentagonalKershner8(tileLen, angleD).Rotate(rotationAngle);

            (int i, int j)? GetBound((int i, int j)[] a, (int i, int j)[] b)
            {
                return a.SelectMany(v1 => b.Select(v2 => (v1, v2))).Where(v => v.v1 == v.v2).Select(v => ((int, int)?)v.v1).FirstOrDefault();
            };

            bool IsHole((int i, int j)[] holes, int i, int j)
            {
                return holes.Any(v => (v.i == i && v.j == j) || (v.i == j && v.j == i));
            }

            var items = s.Convexes.Index().Select(i =>
            {
                var convex = s.Convexes[i];
                var center = convex.Select(i => s.Points[i]).Center();

                return new
                {
                    i,
                    convex = convex,
                    set = convex.SelectCirclePair((i, j) => i > j ? (i: j, j: i) : (i, j)).OrderBy(v => v.i).ThenBy(v => v.j).ToArray(),
                    center = center,
                    radius = convex.Select(i=>(s.Points[i]-center).Len).Max()
                };
            }).ToArray();

            var net = new Net<Model.Vector2, int>(items.Select(v => (v.center, v.i)), 6 * items.Max(v => v.radius));

            var nodes = items.Select(a => new
            {
                a.i,
                a.convex,
                a.set,
                a.center,
                edges = net.SelectNeighbors(a.center)
                            .Select(i=>items[i])
                            .Where(b=>b != a)
                            .Select(b => (b, bound: GetBound(a.set, b.set)))
                            .Where(v => v.bound.HasValue)
                            .Select(v => (e: a.i < v.b.i ? (i: a.i, j: v.b.i) : (i: v.b.i, j: a.i), bound: v.bound.Value))
                            .ToArray()
            }).ToArray();

            var g = new Graph(nodes.SelectMany(n => n.edges.Select(e => e.e)).Distinct());
            var holes = g.RandomVisitEdges(seed).Select(e => e.e).ToArray();

            var bounds = holes.SelectMany(h => nodes[h.i].edges.Select(e => e.bound).Intersect(nodes[h.j].edges.Select(e => e.bound))).ToList();

            var n = s.Points.Length;

            bounds = bounds.Concat(new[] { (6, 7), (n - 6, n - 5) }).ToList(); // exits

            var shape = new Shape()
            {
                Points2 = s.Points,
                Convexes = nodes.SelectMany(n => n.set.Where(e => !bounds.Contains(e))).Distinct().Select(e => new[] { e.i, e.j }).ToArray()
            };

            return shape;
        }

        public static Shape CreateNet3Maze(int m, int n, int l)
        {
            var (graph, nodes) = Graphs.Net3Graph(m, n, l);
            
            graph.MinimizeConnections();

            return new Shape
            {
                Points3 = nodes.Select(v => new Vector3(v.i, v.j, v.k)).ToArray(),
                Convexes = graph.Edges.Select(v => new[] { v.i, v.j }).ToArray()
            };
        }

        public static Shape CreateNet2Maze(int m, int n)
        {
            var (graph, nodes) = Graphs.Net2Graph(m, n);

            graph.MinimizeConnections();

            return new Shape
            {
                Points3 = nodes.Select(v => new Vector3(v.i, v.j, 0)).ToArray(),
                Convexes = graph.Edges.Select(v => new[] { v.i, v.j }).ToArray()
            };
        }
    }
}
