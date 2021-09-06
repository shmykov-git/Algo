using Model.Extensions;
using System.Linq;

namespace Model.Graph
{
    public static class Graphs2
    {
        public static Graph NetGraph(int m, int n)
        {
            int num(int i, int j) => i * n + j;

            return new Graph((m, n).SelectRange((i, j) =>
                {
                    if (i == m - 1)
                    {
                        if (j == n - 1)
                            return new (int i, int j)[] { };
                        else
                            return new[] { (num(i, j), num(i, j + 1)) };
                    }
                    else
                    {
                        if (j == n - 1)
                            return new[] { (num(i, j), num(i + 1, j)) };
                        else
                            return new[] { (num(i, j), num(i, j + 1)), (num(i, j), num(i + 1, j)) };
                    }
                }
            ).SelectMany(v => v));
        }
    }
}
