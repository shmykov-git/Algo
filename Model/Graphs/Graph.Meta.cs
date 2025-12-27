using Model.Extensions;
using System;
using System.Linq;

namespace Model.Graphs
{
    public partial class Graph
    {
        public static int[] JoinMetas(int[] a, int[] b)
        {
            if (a[0] == b[0])
                return a[1..].ReverseLinq().Concat(b).ToArray();

            if (a[^1] == b[0])
                return a[..^1].Concat(b).ToArray();

            if (a[0] == b[^1])
                return a[1..].ReverseLinq().Concat(b.ReverseLinq()).ToArray();

            if (a[^1] == b[^1])
                return a[..^1].Concat(b.ReverseLinq()).ToArray();

            throw new ArgumentException("Cannot join metas");
        }

        public void GroupNode(Node n)
        {
            if (n.edges.Count != 2)
                throw new ArgumentException($"Cannot group node {n.i} with {n.edges.Count} edges");

            var a = n.edges[0];
            var b = n.edges[1];

            var nextEdge = a.b.i == b.a.i ? (a.a.i, b.b.i) : (b.a.i, a.b.i);
            var meta = a.b.i == b.a.i ? JoinMetas(a.meta, b.meta) : JoinMetas(b.meta, a.meta);

            RemoveEdge(a);
            RemoveEdge(b);
            var e = AddConnectedEdge(nextEdge);

            e.meta = meta;
        }

        public bool MetaGroup()
        {
            var res = false;

            while (true)
            {
                var n = nodes.FirstOrDefault(n => n.edges.Count == 2 && n.edges.GroupBy(e => e.e).Count() == 2);

                if (n == null)
                    break;

                var a = n.edges[0];
                var b = n.edges[1];
                var nextEdge = a.b.i == b.a.i ? (a.a.i, b.b.i) : (b.a.i, a.b.i);

                if (Edges.Count(e => e == nextEdge) >= 2)
                    break;

                GroupNode(n);
                //this.WriteToDebug($"{n} <- ");

                res = true;
            }

            return res;
        }
    }
}