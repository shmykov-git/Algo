using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Graph
{
    public static class Maze2
    {
        public static Graph CreateMaze(int m, int n)
        {
            var r = new Random(2);

            var g = Graphs2.NetGraph(m, n);

            var exclude = new List<Graph.Edge>();

            for(var i=0; i<m*n; i++)
            {
                var edges = g.edges.Where(edge => !exclude.Contains(edge)).ToArray();

                if (edges.Length == 0)
                    break;

                var edge = edges[r.Next(edges.Length)];

                g.RemoveEdge(edge);

                if (g.Visit().Count() != g.nodes.Count)
                {
                    g.AddEdge(edge);
                    exclude.Add(edge);
                    i--;
                }
            }

            return g;
        }
    }
}
