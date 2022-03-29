using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model;
using Model.Extensions;
using Model.Graphs;
using Model.Libraries;
using Model3D.Extensions;
using Vector2 = Model.Vector2;

namespace Model3D.Tools
{
    public static class PerimeterEngine
    {
        public static int[][] FindPerimeter(Shape shape)
        {
            var points = shape.Points2;
            var perimeters = new List<int[]>();

            Way GetWay(Vector2 a, Vector2 b, Graph.Node n, Graph.Edge ee = null)
            {
                return n.edges
                    .Where(e => ee == null || e != ee)
                    .Select(e => (w: new Way(e.Another(n), e), ang: Angle.LeftDirection(a, b, points[e.Another(n).i])))
                    .OrderBy(v => v.ang)
                    .First().w;
            }

            int[] GetPerimeter(Graph g)
            {
                var nodes = g.edges.Select(e => e.a).Select(n => (n.i, n, p: points[n.i])).ToArray();
                var rightNode = nodes.OrderByDescending(n => n.p.x).First();
                var startWay = GetWay(rightNode.p + Vector2.OneX, rightNode.p, g.nodes[rightNode.i]);

                var way = startWay;

                var perimeter = new List<int>();
                do
                {
                    perimeter.Add(way.b.i);
                    way = GetWay(points[way.a.i], points[way.b.i], way.b, way.e);
                } while (startWay.e != way.e);

                return perimeter.ToArray();
            }


            var graph = shape.ToGraph();

            while (!graph.IsEmpty)
            {
                var p = GetPerimeter(graph);
                perimeters.Add(p);

                var removeNodes = graph.Visit(graph.nodes[p[0]]).Select(n => n.i).ToHashSet();
                var removeEdges = graph.edges.Where(e => removeNodes.Contains(e.e.i) || removeNodes.Contains(e.e.j)).ToArray();
                removeEdges.ForEach(e => graph.RemoveEdge(e));
            }

            return perimeters.ToArray();
        }

        struct Way
        {
            public Graph.Node b;
            public Graph.Edge e;
            public Graph.Node a => e.Another(b);

            public Way(Graph.Node b, Graph.Edge e)
            {
                this.b = b;
                this.e = e;
            }

            public override string ToString() => $"({a.i}, {b.i})";
        }
    }
}