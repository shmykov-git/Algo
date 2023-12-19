using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System.Linq;
using Model.Graphs;
using Model3D.Libraries;

namespace Model3D
{
    public static class Mazes
    {
        public static Shape CrateKershner8Maze(double tileLen, double angleD, double rotationAngle, int seed = 0, MazeType type = MazeType.SimpleRandom)
        {
            return Parquets.PentagonalKershner8Native(tileLen, angleD).Rotate(rotationAngle).ToShape3().ToMaze(seed, type, new[] { (6, 7), (-6, -5) });
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

        public static Shape CreateNet3MazeBox(int m, int n, int l, int seed = 0)
        {
            var (graph, nodes) = Graphs.Net3Graph(m, n, l);
            var points = nodes.Select(v => new Vector3(v.i, v.j, v.k)).ToArray().Centered();

            var g = new Graph(graph.nodes.SelectMany(n => n.edges.Select(e => e.e)).Distinct());
            var holes = g.VisitEdges(seed, GraphVisitStrateges.SimpleRandom(seed)).Select(e => e.e.OrderedEdge())
                .ToHashSet();

            var mult = 0.8;

            Shape GetWall(Vector3 a, Vector3 b, Shape shape)
            {
                var n = (b - a).Normalize();
                var c = (a + b) / 2;
                var size = (b - a).Length;

                var q = Quaternion.FromRotation(Vector3.ZAxis, n);
                var s = shape.Scale(size, size, 0.1 * size).Mult(mult).Rotate(q).Move(c);
                
                return s;
            }

            var edges = graph.Edges.Where(e => !holes.Contains(e.OrderedEdge())).ToArray();

            var brick = Shapes.Cube;
            var shape = edges.Select(e => GetWall(points[e.i], points[e.j], brick)).Aggregate((a, b) => a + b);

            return shape;
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
