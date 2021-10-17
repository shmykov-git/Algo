using System;
using System.Collections.Generic;
using System.Linq;
using Model.Extensions;
using Model.Libraries;

namespace Model.Graphs
{
    public partial class Graph
    {
        public IEnumerable<(int i, int j)> Edges => edges.Select(edge => edge.e);
        public IEnumerable<int> Nodes => nodes.Select(n => n.i);
        public Dictionary<int, int> GetBackIndices() => nodes.IndexValue().ToDictionary(v => v.value.i, v => v.index);

        public List<Node> nodes;
        public List<Edge> edges;

        public Graph(IEnumerable<(int i, int j)> edges)
        {
            var n = edges.Select(v => Math.Max(v.i, v.j)).Max() + 1;
            nodes = Enumerable.Range(0, n).Select(i => new Node { i = i, edges = new List<Edge>() }).ToList();

            this.edges = edges.Select(e =>
            {
                var edge = new Edge()
                {
                    a = nodes[e.i],
                    b = nodes[e.j]
                };

                edge.a.edges.Add(edge);
                edge.b.edges.Add(edge);

                return edge;
            }).ToList();
        }

        public void TakeOutNode(Node node)
        {
            node.edges.Select(e => e.Another(node)).ToArray().ForEachCirclePair((a, b) => AddEdge(a, b));

            foreach (var e in node.edges.ToArray())
                RemoveEdge(e);
            
            nodes.Remove(node);
        }

        public void AddEdge(Node a, Node b)
        {
            AddEdge(new Edge() { a = a, b = b });
        }

        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
            edge.a.edges.Add(edge);
            edge.b.edges.Add(edge);
        }

        public void RemoveEdge(Edge edge)
        {
            edges.Remove(edge);
            edge.a.edges.Remove(edge);
            edge.b.edges.Remove(edge);
        }
    }
}
