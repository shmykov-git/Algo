using System.Collections.Generic;
using System.Linq;
using Model.Extensions;

namespace Model.Graphs
{
    public partial class Graph
    {
        public class Edge
        {
            public Node a;
            public Node b;

            private int[] _meta;

            public int[] meta
            {
                get => _meta ?? new[] {a.i, b.i};
                set => _meta = value;
            }

            public Node Another(Node n) => n == a ? b : a;
            public (int i, int j) e => (a.i, b.i);

            public override string ToString() =>
                _meta == null ? $"({a.i}, {b.i})" : $"({a.i}, {b.i}) m: {_meta.SJoin(" ")}";
        }

        public class Node
        {
            public int i;
            public List<Edge> edges;

            public Edge ToEdge(Node n) => edges.First(e => e.Another(this) == n);
            public bool IsConnected(Node n) => edges.Any(e => e.Another(this) == n);

            public IEnumerable<Node> Siblings => edges.Select(e => e.Another(this));

            public override string ToString() => $"n{i}:{edges.Count}";
        }
    }
}