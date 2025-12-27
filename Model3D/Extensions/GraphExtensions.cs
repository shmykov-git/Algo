using Model;
using Model.Graphs;
using System.Linq;

namespace Model3D.Extensions
{
    public static class GraphExtensions
    {
        public static Shape ToNet2Shape(this Graph graph, int m, int n)
        {
            var nodes = graph.Nodes.ToArray();

            return new Shape
            {
                Points2 = nodes.Select(k => new Model.Vector2(k / n, k % n)).ToArray(),
                Convexes = graph.Edges.Select(v => new[] { v.i, v.j }).ToArray()
            };
        }

        public static Shape ToNet3Shape(this Graph graph, int m, int n, int l)
        {
            var nodes = graph.Nodes.ToArray();

            return new Shape
            {
                Points3 = nodes.Select(k => new Vector3(k / (l * n), (k / l) % n, k % (l * n))).ToArray(),
                Convexes = graph.Edges.Select(v => new[] { v.i, v.j }).ToArray()
            };
        }
    }
}
