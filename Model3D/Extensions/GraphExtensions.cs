using Model;
using Model.Graph;
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
                Points2 = nodes.Select(k => new Vector2(k / n, k % n)).ToArray(),
                Convexes = graph.Edges.Select(v => new[] { v.i, v.j }).ToArray()
            };
        }
    }
}
