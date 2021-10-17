using System.Collections.Generic;
using System.Linq;

namespace Model.Graphs
{
    public partial class Graph
    {
        public class Edge
        {
            public Node a;
            public Node b;

            public Node Another(Node n) => n == a ? b : a;
            public (int i, int j) e => (a.i, b.i);
        }

        public class Node
        {
            public int i;
            public List<Edge> edges;

            public bool IsConnected(Node n) => edges.Any(e => e.Another(this) == n);

            public IEnumerable<Node> Siblings => edges.Select(e => e.Another(this));
        }
    }
}