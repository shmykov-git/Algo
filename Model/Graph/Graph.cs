using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Graph
{
    public class Graph
    {
        public IEnumerable<(int i, int j)> Edges => edges.Select(edge => edge.e);
        public IEnumerable<int> Nodes => nodes.Select(n => n.i);

        public List<Node> nodes;
        public List<Edge> edges;

        public class Edge
        {
            public (int i, int j) e => (a.i, b.i);
            public Node a;
            public Node b;
        }

        public class Node
        {
            public int i;
            public List<Edge> edges;
        }

        public Graph(IEnumerable<(int i, int j)> edges)
        {
            var n = edges.Select(v => Math.Max(v.i, v.j)).Max() + 1;
            this.nodes = Enumerable.Range(0, n).Select(i => new Node { i = i, edges = new List<Edge>() }).ToList();

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

        public IEnumerable<Node> Visit(Node node = null)
        {
            var visited = new bool[nodes.Count];
            var queue = new Queue<Node>(nodes.Count);

            queue.Enqueue(node ?? nodes[0]);

            do
            {
                var n = queue.Dequeue();
                if (!visited[n.i])
                {
                    visited[n.i] = true;

                    yield return n;

                    foreach(var edge in n.edges)
                    {
                        queue.Enqueue(edge.a);
                        queue.Enqueue(edge.b);
                    }
                }
            } while (queue.Count > 0);
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
