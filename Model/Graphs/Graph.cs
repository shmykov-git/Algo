using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public readonly List<Node> nodes;
        public readonly List<Edge> edges;

        public Graph()
        {
            nodes = new List<Node>();
            edges = new List<Edge>();
        }

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

        public Edge AddConnectedEdge((int i, int j) p)
        {
            var a = nodes.First(n => n.i == p.i);
            var b = nodes.First(n => n.i == p.j);

            return AddEdge(a, b);
        }

        public Edge AddEdge(int i, int j)
        {
            var a = new Node { i = i, edges = new List<Edge>() };
            var b = new Node { i = j, edges = new List<Edge>() };
            nodes.Add(a);
            nodes.Add(b);
            
            return AddEdge(a, b);
        }

        public Edge AddEdge(Node a, Node b)
        {
            var e = new Edge() {a = a, b = b};
            AddEdge(e);

            return e;
        }

        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
            edge.a.edges.Add(edge);
            edge.b.edges.Add(edge);
        }

        public void RemoveNode(Node node)
        {
            foreach (var e in node.edges.ToArray())
            {
                RemoveEdge(e);
            }

            nodes.Remove(node);
        }

        public void RemoveEdgeWithEmptyNodes(Edge edge)
        {
            RemoveEdge(edge);

            if (edge.a.edges.Count == 0)
                RemoveNode(edge.a);

            if (edge.b.edges.Count == 0)
                RemoveNode(edge.b);
        }

        public void RemoveEdge(Edge edge)
        {
            edges.Remove(edge);
            edge.a.edges.Remove(edge);
            edge.b.edges.Remove(edge);
        }

        public Graph Clone() => new Graph(Edges);

        public void WriteToDebug(string prefix = null)
        {
            Debug.WriteLine($"{prefix}{string.Join(", ", edges.Select(e => $"{e}"))}");
        }
    }
}
