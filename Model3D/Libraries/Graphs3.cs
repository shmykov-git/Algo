using Model.Extensions;
using Model.Graphs;
using System.Linq;

namespace Model3D
{
    public static class Graphs
    {
        public static (Graph graph, (int i, int j)[] nodes) Net2Graph(int m, int n)
        {
            int num(int i, int j) => i * n + j;

            var graph = new Graph((m, n).SelectRange((i, j) =>
            {
                var iBound = i == m - 1;
                var jBound = j == n - 1;

                if (iBound && jBound)
                    return new (int, int)[0];

                else if (iBound && !jBound)
                    return new[] { (num(i, j), num(i, j + 1)) };

                else if (!iBound && jBound)
                    return new[] { (num(i, j), num(i + 1, j)) };

                else
                    return new[] { (num(i, j), num(i + 1, j)), (num(i, j), num(i, j + 1)) };
            }
            ).SelectMany(v => v));

            var nodes = (m, n).SelectRange((i, j) => (i, j)).ToArray();

            return (graph, nodes);
        }

        public static (Graph graph, (int i, int j, int k)[] nodes) Net3Graph(int m, int n, int l)
        {
            int num(int i, int j, int k) => i * n * l + j * l + k;

            var graph = new Graph((m, n, l).SelectRange((i, j, k) =>
            {
                var iBound = i == m - 1;
                var jBound = j == n - 1;
                var kBound = k == l - 1;

                if (iBound && jBound && kBound)
                    return new (int, int)[0];

                else if (iBound && jBound && !kBound)
                    return new[] { (num(i, j, k), num(i, j, k + 1)) };

                else if (iBound && !jBound && kBound)
                    return new[] { (num(i, j, k), num(i, j + 1, k)) };

                else if (iBound && !jBound && !kBound)
                    return new[] { (num(i, j, k), num(i, j, k + 1)), (num(i, j, k), num(i, j + 1, k)) };

                else if (!iBound && jBound && kBound)
                    return new[] { (num(i, j, k), num(i + 1, j, k)) };

                else if (!iBound && jBound && !kBound)
                    return new[] { (num(i, j, k), num(i + 1, j, k)), (num(i, j, k), num(i, j, k + 1)) };

                else if (!iBound && !jBound && kBound)
                    return new[] { (num(i, j, k), num(i + 1, j, k)), (num(i, j, k), num(i, j + 1, k)) };

                else
                    return new[] { (num(i, j, k), num(i + 1, j, k)), (num(i, j, k), num(i, j + 1, k)), (num(i, j, k), num(i, j, k + 1)) };
            }
            ).SelectMany(v => v));

            var nodes = (m, n, l).SelectRange((i, j, k) => (i, j, k)).ToArray();

            return (graph, nodes);
        }
    }
}
