using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Graph
{
    public static class GraphExtensions
    { 
        public static Graph MinimizeConnections(this Graph g, int seed = 0)
        {
            var r = new Random(seed);

            var exclude = new List<Graph.Edge>();

            while (true)
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
                }
            }

            return g;
        }
    }
}
