using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Model.Extensions;

namespace Model.Tools
{
    public static class Triangulator2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double GetFactor(Vector2 a, Vector2 b, Vector2 c)
        {
            if (a == b || b == c)
                return 0;

            var ab = (b - a)/*.Normed*/;
            var bc = (c - b)/*.Normed*/;

            var scalar = ab.Normal * bc;
            var isOuter = scalar < 0;

            return isOuter ? double.MaxValue : (c - a).Len2;
        }

        static int[] GetTriangle(Node n) => new[] { n.prev.i, n.i, n.next.i };

        class Node
        {
            public int i;
            public Vector2 p;
            public Node prev;
            public Node next;

            // каким должен быть параметр сортировки, чтобы в правильной последовательности взять все углы
            // брать узкие углы, малые по объему площади
            public double Factor => GetFactor(prev.p, p, next.p);

            public override string ToString() => $"{i}: {Factor}, {p}";
        }

        public static int[][] Triangulate(Polygon polygon)
        {
            var nodes = polygon.Points.Select((p, i) => new Node() { i = i, p = p }).ToArray();
            
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
                //return;

                var n = sortedStack.Peek();
                
                if (n.Factor > 0)
                    Debug.WriteLine($"{i}|{n}");
            }

            var count = 1000;
            var i = 0;

            DebugStack(i);

            while (sortedStack.Count > 2)
            {
                if (i++ == count)
                    break;

                var n = Pop();
                DebugStack(i);

                triangles.Add(GetTriangle(n));
            }

            return triangles.ToArray();
        }
    }
}