using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model.Graphs;

namespace Model.Extensions
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


        public static Graph.Node[][] SplitToMinCircles(this Graph graph)
        {
            var main = graph.Clone();

            var res = new List<Graph.Node[]>();

            bool IsEmpty(Graph g) => g.nodes.All(n => n.edges.Count == 0);
            
            Graph.Node[] ExcludePathGraph(Graph g)
            {
                var a = g.nodes.Where(n => n.edges.Count > 0).OrderBy(n=>n.edges.Count).First();
                var edge = a.edges.First();
                var b = edge.Another(a);

                g.RemoveEdge(edge);
                var path = g.FindPath(a, b).ToArray();
                g.AddEdge(edge);

                var pathEdges = path
                    .SelectCirclePair((a, b) => (a, b))
                    .Select(p => p.a.edges.First(e => e.b == p.b || e.a == p.b)).ToArray();

                var removeReverse = false;
                foreach (var e in pathEdges)
                {
                    g.RemoveEdge(e);

                    if (e.a.edges.Count >= 2 || e.b.edges.Count >= 2)
                    {
                        removeReverse = true;
                        break;
                    }
                }

                if (removeReverse)
                {
                    foreach (var e in pathEdges.Reverse())
                    {
                        g.RemoveEdge(e);

                        if (e.a.edges.Count >= 2 || e.b.edges.Count >= 2)
                            break;
                    }
                }

                return path;
            }

            main.WriteToDebug();

            while (!IsEmpty(main))
            {
                var circle = ExcludePathGraph(main);
                res.Add(circle);
                
                Debug.WriteLine(string.Join(", ", circle.Select(n=>$"{n.i}")));
                main.WriteToDebug();
            }

            return res.ToArray();
        }
    }
}
