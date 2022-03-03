using System;
using System.Collections.Generic;
using System.Linq;
using Model.Extensions;

namespace Model.Graphs
{
    public partial class Graph
    {
        public void GroupNode(Node n)
        {
            if (n.edges.Count != 2)
                throw new ArgumentException($"Cannot group node {n.i} with {n.edges.Count} edges");

            var eA = n.edges[0];
            var eB = n.edges[1];

            var a = eA.Another(n);
            var b = eB.Another(n);

            var metaA = a.i == eA.meta[0] ? eA.meta : eA.meta.Reverse().ToArray();
            var metaN = n.i == eB.meta[0] ? eB.meta : eB.meta.Reverse().ToArray();

            RemoveEdge(eA);
            RemoveEdge(eB);
            var e = AddConnectedEdge((a.i, b.i).OrderedEdge());

            e.meta = a.i < b.i 
                ? metaA[0..^1].Concat(metaN).ToArray()
                : metaA[0..^1].Concat(metaN).Reverse().ToArray();
        }

        public bool MetaGroup()
        {
            var res = false;

            while (true)
            {
                var n = nodes.FirstOrDefault(n => n.edges.Count == 2 && n.edges.GroupBy(e=>e.e).Count() == 2);

                if (n == null)
                    break;

                var nextEdge = (n.edges[0].Another(n).i, n.edges[1].Another(n).i).OrderedEdge();
                
                if (Edges.Count(e => e == nextEdge) >= 2)
                    break;

                GroupNode(n);
                this.WriteToDebug($"{n} <- ");

                res = true;
            }

            return res;
        }
    }
}