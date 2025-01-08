using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System.Linq;
using Model.Graphs;
using Model3D.Libraries;
using System;

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

        public static Func<int, int, int, int> GetNum3Fn(int m, int n, int l) => (int i, int j, int k) => i * n * l + j * l + k;

        public static (Shape maze, Shape holes, Shape path) CreateNet3MazeBox(int m, int n, int l, bool closed, (int i, int j, int k)[] outHoles = null, int seed = 0)
        {
            (int i, int j, int k) getInHole(int i, int j, int k) => (i == -1 ? 0 : (i == m ? m - 1 : i), j == -1 ? 0 : (j == n ? n - 1 : j), k == -1 ? 0 : (k == l ? l - 1 : k));
            var ohs = outHoles ?? Array.Empty<(int, int, int)>();
            var hs = ohs.Select(h => (a: h, b: getInHole(h.i, h.j, h.k))).ToArray();

            var num = GetNum3Fn(m, n, l);
            var (graph, nodes) = Graphs.Net3Graph(m, n, l);
            var points = nodes.Select(v => new Vector3(v.i, v.j, v.k)).ToArray();

            var g = new Graph(graph.nodes.SelectMany(n => n.edges.Select(e => e.e)).Distinct());
            var holes = hs.Length > 0
                ? g.VisitEdges(seed, GraphVisitStrateges.SimpleRandom(seed), graph.nodes.First(n => n.i == num(hs[0].b.i, hs[0].b.j, hs[0].b.k))).Select(e => e.e.OrderedEdge()).ToHashSet()
                : g.VisitEdges(seed, GraphVisitStrateges.SimpleRandom(seed)).Select(e => e.e.OrderedEdge()).ToHashSet();

            var mult = 0.8;

            var wallScale = new Vector3(1, 1, 0.1);
            var holeScale = new Vector3(0.1, 0.1, 1);
            var pathScale = new Vector3(0.105, 0.105, 1.05);
            Shape GetWall(Vector3 a, Vector3 b, Shape shape, Vector3 scale)
            {
                var n = (b - a).Normalize();
                var c = (a + b) / 2;
                var size = (b - a).Length;

                var q = Quaternion.FromRotation(Vector3.ZAxis, n);
                var s = shape.Scale(scale * size).Mult(mult).Rotate(q).Move(c);
                
                return s;
            }

            var edges = graph.Edges.Where(e => !holes.Contains(e.OrderedEdge())).ToArray();

            var brick = Shapes.Cube;
            var maze = edges.Select(e => GetWall(points[e.i], points[e.j], brick, wallScale)).Aggregate((a, b) => a + b);
            var mazeHoles = holes.Select(e => GetWall(points[e.i], points[e.j], brick, holeScale)).Aggregate((a, b) => a + b);
            Shape mazePath = null;

            if (hs.Length == 2)
            {
                var nA = graph.Nodes.First(nI => nI == num(hs[0].b.i, hs[0].b.j, hs[0].b.k));
                var nB = graph.Nodes.First(nI => nI == num(hs[1].b.i, hs[1].b.j, hs[1].b.k));
                var gPath = new Graph(holes);
                var path = gPath.FindPath(nA, nB);
                mazePath = path.SelectPair((nA, nB)=>nA.ToEdge(nB)).Select(e => GetWall(points[e.a.i], points[e.b.i], brick, pathScale)).Aggregate((a, b) => a + b);
            }

            if (closed)
            {
                var wallL1 = (m, n).SelectRange((i, j) => ohs.Contains((i, j, -1)) ? Shape.Empty : GetWall(new Vector3(i, j, 0), new Vector3(i, j, -1), brick, wallScale)).Aggregate((a, b) => a + b);
                var wallL2 = (m, n).SelectRange((i, j) => ohs.Contains((i, j, l)) ? Shape.Empty : GetWall(new Vector3(i, j, l - 1), new Vector3(i, j, l), brick, wallScale)).Aggregate((a, b) => a + b);
                var wallN1 = (m, l).SelectRange((i, k) => ohs.Contains((i, -1, k)) ? Shape.Empty : GetWall(new Vector3(i, 0, k), new Vector3(i, -1, k), brick, wallScale)).Aggregate((a, b) => a + b);
                var wallN2 = (m, l).SelectRange((i, k) => ohs.Contains((i, n, k)) ? Shape.Empty : GetWall(new Vector3(i, n - 1, k), new Vector3(i, n, k), brick, wallScale)).Aggregate((a, b) => a + b);
                var wallM1 = (n, l).SelectRange((j, k) => ohs.Contains((-1, j, k)) ? Shape.Empty : GetWall(new Vector3(0, j, k), new Vector3(-1, j, k), brick, wallScale)).Aggregate((a, b) => a + b);
                var wallM2 = (n, l).SelectRange((j, k) => ohs.Contains((m, j, k)) ? Shape.Empty : GetWall(new Vector3(m - 1, j, k), new Vector3(m, j, k), brick, wallScale)).Aggregate((a, b) => a + b);
                maze = maze + (wallL1 + wallL2 + wallN1 + wallN2 + wallM1 + wallM2);
                
                mazeHoles += hs.Select(h => GetWall(new Vector3(h.a.i, h.a.j, h.a.k), new Vector3(h.b.i, h.b.j, h.b.k), brick, holeScale)).Aggregate((a, b) => a + b);
                
                if (hs.Length == 2)
                {
                    mazePath += hs.Select(h => GetWall(new Vector3(h.a.i, h.a.j, h.a.k), new Vector3(h.b.i, h.b.j, h.b.k), brick, pathScale)).Aggregate((a, b) => a + b);
                }
            }

            return (maze, mazeHoles, mazePath);
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
