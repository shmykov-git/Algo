using Model.Extensions;

namespace AI.Extensions;

public static class MatrixExtensions
{
    public static int AbsSum(this int[][] m)
    {
        int sum = 0;

        for (var i = 0; i < m.Length; i++)
            for (var j = 0; j < m[i].Length; j++)
                sum += m[i][j].Abs();

        return sum;
    }
}
