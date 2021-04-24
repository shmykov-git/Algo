using System;
using System.Linq;

namespace Model.Libraries
{
    public static class PointsGen
    {
        public static Vector2[] GetPoints(double from, double to, int count, Func<double, Vector2> fn)
        {
            var step = (to - from) / (count - 1);

            return Enumerable.Range(0, count).Select(i => fn(from + step * i)).ToArray();
        }
    }
}
