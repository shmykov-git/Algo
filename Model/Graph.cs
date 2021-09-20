using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Model
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
            public Node Another(Node n) => n == a ? b : a;
        }

        public class Node
        {
            public int i;
            public List<Edge> edges;
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

        public IEnumerable<Edge> VisitEdges(int seed = 0, GraphVisitStrategy directionFn = null, Node node = null)
        {
            directionFn ??= GraphVisitStrateges.SimpleRandom(seed);

            var visited = new bool[nodes.Count];
            var stack = new Stack<(Node to, Edge eFrom)>(nodes.Count);

            stack.Push((node ?? nodes[0], null));

            do
            {
                var move = stack.Pop();
                var to = move.to;

                if (!visited[to.i])
                {
                    visited[to.i] = true;

                    var from = move.eFrom?.Another(to);
                    if (move.eFrom != null)
                        yield return move.eFrom;

                    var edgeIndices = to.edges.Index().Select(ind => (ind, i: to.edges[ind].Another(to).i)).Where(v => !visited[v.i]).ToDictionary(v => v.i, v => v.ind);
                    var dirs = directionFn(from?.i??-1, move.to.i, edgeIndices.Keys.ToArray());

                    foreach (var dir in dirs)
                    {
                        var edge = to.edges[edgeIndices[dir]];
                        stack.Push((edge.Another(to), edge));
                    }

                    //Debug.WriteLine(string.Join(", ", stack.Select(v => $"({v.eFrom.a.i}, {v.eFrom.b.i})")));
                }
            } while (stack.Count > 0);
        }

        public IEnumerable<Node> FindPath(int from, int to) => FindPath(nodes[from], nodes[to]);

        public IEnumerable<Node> FindPath(Node from = null, Node to = null)
        {
            from ??= nodes[0];
            to ??= nodes[^1];

            var distance = new int[nodes.Count];
            var queue = new Queue<(Node,Node)>(nodes.Count);

            queue.Enqueue((to, to));

            do
            {
                var (prev, n) = queue.Dequeue();

                if (distance[n.i] == 0)
                {
                    distance[n.i] = distance[prev.i] + 1;

                    if (n == from)
                        break;

                    foreach (var edge in n.edges)
                    {
                        queue.Enqueue((n, edge.Another(n)));
                    }
                }
            } while (queue.Count > 0);

            var node = from;
            while(node != to)
            {
                yield return node;

                node = node.edges.Select(e => e.Another(node)).Where(n => distance[n.i] > 0).OrderBy(n => distance[n.i]).First();
            }

            yield return to;
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

                    foreach (var edge in n.edges)
                    {
                        queue.Enqueue(edge.Another(n));
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
