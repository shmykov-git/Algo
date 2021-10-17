using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Model.Graphs
{
    public partial class Graph
    {
        public IEnumerable<Node> FindPath(int from, int to) => FindPath(nodes[from], nodes[to]);

        public IEnumerable<Node> FindPath(Node from = null, Node to = null)
        {
            from ??= nodes[0];
            to ??= nodes[^1];

            var distance = new int[nodes.Count];
            var queue = new Queue<(Node, Node)>(nodes.Count);

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
            while (node != to)
            {
                yield return node;

                node = node.edges.Select(e => e.Another(node)).Where(n => distance[n.i] > 0).OrderBy(n => distance[n.i]).First();
            }

            yield return to;
        }

        public IEnumerable<Node> FindPathAstar(Node from = null, Node to = null)
        {
            // минимизация числа посещенных нодов
            // оптимизация сложности алгоритма от o(n^2) к o(nlogn)

            return null;
        }
    }
}