using Model.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Model.Tools.Triangulate
{
    public static class Triangulator2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double GetFactor(Vector2 a, Vector2 b, Vector2 c, double factor)
        {
            if (a == b || b == c)
                return 0;

            var ab = b - a/*.Normed*/;
            var bc = c - b/*.Normed*/;

            var scalar = ab.NormalM * bc;
            var isOuter = scalar < double.Epsilon;

            return isOuter ? double.MaxValue : (c - a).Len2;
        }

        public class Options
        {
            public double TriangulationFixFactor { get; set; }
            public int? DebugTriangulationSteps { get; set; }
        }

        static int[] GetTriangle(Node n) => new[] { n.prev.i, n.i, n.next.i };

        class Node
        {
            public int i;
            public Vector2 p;
            public Node prev;
            public Node next;
            public double fixFactor;

            // каким должен быть параметр сортировки, чтобы в правильной последовательности взять все углы
            // брать узкие углы, малые по объему площади
            public double Factor => GetFactor(prev.p, p, next.p, fixFactor);

            public override string ToString() => $"({prev.i}-{i}-{next.i}): {Factor}, {p}";
        }

        public static int[][] Triangulate(Polygon polygon, Options options = null)
        {
            options ??= new Options();

            var nodes = polygon.Points.Select((p, i) => new Node() { i = i, p = p, fixFactor = options.TriangulationFixFactor }).ToArray();

            nodes.ForEach(n =>
            {
                n.prev = nodes[(n.i - 1 + nodes.Length) % nodes.Length];
                n.next = nodes[(n.i + 1) % nodes.Length];
            });

            var sortedStack = new SortedStack<Node>(nodes.Length);
            nodes.ForEach(n => sortedStack.Push(n, n.Factor));
            var triangles = new List<int[]>(nodes.Length - 2);

            Node Pop()
            {
                var n = sortedStack.Pop();

                n.prev.next = n.next;
                n.next.prev = n.prev;

                sortedStack.Update(n.prev, n.prev.Factor);
                sortedStack.Update(n.next, n.next.Factor);

                return n;
            }

            void DebugStack(int i)
            {
                var n = sortedStack.Peek();
                Debug.WriteLine($"{i}|{n}");
            }

            var count = options.DebugTriangulationSteps ?? int.MaxValue;
            var i = 0;

            if (options.DebugTriangulationSteps.HasValue)
                DebugStack(i);

            while (sortedStack.Count > 2)
            {
                if (i++ == count)
                    break;

                var n = Pop();

                if (options.DebugTriangulationSteps.HasValue)
                    DebugStack(i);

                triangles.Add(GetTriangle(n));
            }

            return triangles.ToArray();
        }
    }
}