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
        public static Shape CrateKershner8Maze()
        {
            var s = Parquets.PentagonalKershner8(0.03, 1.7).Rotate(-1.09);

            (int i, int j)? GetBound((int i, int j)[] a, (int i, int j)[] b)
            {
                return a.SelectMany(v1 => b.Select(v2 => (v1, v2))).Where(v => v.v1 == v.v2).Select(v => ((int, int)?)v.v1).FirstOrDefault();
            };

            bool IsHole((int i, int j)[] holes, int i, int j)
            {
                return holes.Any(v => (v.i == i && v.j == j) || (v.i == j && v.j == i));
            }

            var items = s.Convexes.Index().Select(i => new
            {
                i,
                convex = s.Convexes[i],
                set = s.Convexes[i].SelectCirclePair((i, j) => i > j ? (i: j, j: i) : (i, j)).OrderBy(v => v.i).ThenBy(v => v.j).ToArray(),
                center = s.Convexes[i].Select(i => s.Points[i]).Center().ToV3()
            }).ToArray();

            var nodes = items.Select(a => new
            {
                a.i,
                a.convex,
                a.set,
                a.center,
                edges = items
                            .Select(b => (b, bound: GetBound(a.set, b.set)))
                            .Where(v => v.bound.HasValue)
                            .Select(v => (e: (i: a.i, j: v.b.i), bound: v.bound.Value))
                            .ToArray()
            }).ToArray();

            var g = new Graph(nodes.SelectMany(n => n.edges.Select(e => e.e).Distinct()));
            g.MinimizeConnections();
            var holes = g.Edges.ToArray();

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
