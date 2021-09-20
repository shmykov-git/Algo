using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System.Linq;

namespace Model3D
{
    public static class Mazes
    {
        public static Shape CrateKershner8Maze(double tileLen, double angleD, double rotationAngle, int seed = 0, MazeType type = MazeType.SimpleRandom)
        {
            return Parquets.PentagonalKershner8(tileLen, angleD).Rotate(rotationAngle).ToShape3().ToMaze(seed, type, new[] { (6, 7), (-6, -5) });
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
