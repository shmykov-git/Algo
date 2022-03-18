using System;
using System.Collections.Generic;
using System.Linq;
using Model.Extensions;
using Model.Libraries;

namespace Model.Graphs
{
    public partial class Graph
    {
        public bool IsConnected(Node a, Node b)
        {
            var visited = new bool[nodes.Count];
            var queue = new Queue<Node>(nodes.Count);
            queue.Enqueue(a);

            do
            {
                var n = queue.Dequeue();

                if (n == b)
                    return true;

                if (!visited[n.i])
                {
                    visited[n.i] = true;

                    foreach (var sn in n.Siblings)
                        queue.Enqueue(sn);
                }
            } while (queue.Count > 0);

            return false;
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

        public IEnumerable<Node> PathVisit(Node node = null)
        {
            var visited = new bool[nodes.Count];
            var queue = new Stack<Node>(nodes.Count);

            queue.Push(node ?? nodes[0]);

            do
            {
                var n = queue.Pop();
                if (!visited[n.i])
                {
                    visited[n.i] = true;

                    yield return n;

                    foreach (var edge in n.edges)
                    {
                        queue.Push(edge.Another(n));
                    }
                }
            } while (queue.Count > 0);
        }

        public IEnumerable<Node> FullPathBiVisit()
        {
            var visited = new bool[nodes.Count];
            var queue = new Stack<Node>(nodes.Count);
            var bi = GetBackIndices();
            var startNode = nodes[0];

            do
            {
                queue.Push(startNode);

                do
                {
                    var n = queue.Pop();
                    if (!visited[bi[n.i]])
                    {
                        visited[bi[n.i]] = true;

                        yield return n;

                        foreach (var edge in n.edges)
                        {
                            queue.Push(edge.Another(n));
                        }
                    }
                } while (queue.Count > 0);

                var index = visited.IndexValue().Where(v => !v.value).Select(v => v.index).FirstOrDefault();
                startNode = index == 0 ? null : nodes[index];
            } while (startNode != null);
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
                    var dirs = directionFn(from?.i ?? -1, move.to.i, edgeIndices.Keys.ToArray());

                    foreach (var dir in dirs)
                    {
                        var edge = to.edges[edgeIndices[dir]];
                        stack.Push((edge.Another(to), edge));
                    }

                    //Debug.WriteLine(string.Join(", ", stack.Select(v => $"({v.eFrom.a.i}, {v.eFrom.b.i})")));
                }
            } while (stack.Count > 0);
        }
    }
}