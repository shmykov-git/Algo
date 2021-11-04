using System;
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

                node = node.edges.Select(e => e.Another(node)).Where(n => distance[n.i] > 0).OrderBy(n => distance[n.i])
                    .First();
            }

            yield return to;
        }

        // how to: https://www.youtube.com/watch?v=-L-WgKMFuhE
        // todo: можно оптимизировать заменив double на long, и для равноудаленных узлов брать ближайший к цели (как на видео)
        public IEnumerable<Node> FindPathAStar(Func<Node, Node, double> distanceFn, Node from = null, Node to = null)
        {
            from ??= nodes[0];
            to ??= nodes[^1];

            var infos = new Dictionary<Node, Info>();
            var openSet = new SortedStack<Node>(nodes.Count);
            var closeSet = new HashSet<Node>(nodes.Count);

            void UpdateOpenSetItem(Node prev, Node n)
            {
                var prevPathDistance = infos.TryGetValue(prev, out Info prevInfo) ? prevInfo.PathDistanceFrom : 0;

                if (!infos.TryGetValue(n, out Info info))
                {
                    info = new Info()
                    {
                        DistanceTo = distanceFn(n, to),
                        PathDistanceFrom = prevPathDistance + distanceFn(prev, n),
                        Prev = prev
                    };
                    infos.Add(n, info);
                    openSet.Push(n, info.PathDistance);
                }
                else
                {
                    var pathDistance = prevPathDistance + distanceFn(prev, n);
                    if (pathDistance < info.PathDistanceFrom)
                    {
                        info.PathDistanceFrom = pathDistance;
                        info.Prev = prev;
                        openSet.Update(n, info.PathDistance);
                    }
                }
            }

            UpdateOpenSetItem(from, from);

            do
            {
                var n = openSet.Pop();
                closeSet.Add(n);

                if (n == to)
                    break;

                foreach (var nn in n.edges.Select(e=>e.Another(n)).Where(node=>!closeSet.Contains(node)))
                {
                    UpdateOpenSetItem(n, nn);
                }
            } while (!openSet.IsEmpty);

            Debug.WriteLine($"{openSet.Count} + {closeSet.Count} = {openSet.Count + closeSet.Count}");

            var node = to;
            while (node != from)
            {
                yield return node;

                node = infos[node].Prev;
            }

            yield return from;
        }

        private struct Info
        {
            public double DistanceTo;
            public double PathDistanceFrom;
            public Node Prev;
            public double PathDistance => DistanceTo + PathDistanceFrom;
        }
    }
}