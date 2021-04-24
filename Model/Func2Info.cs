using System;
using System.Linq;

namespace Model
{
    public class Func2Info
    {
        public double From;
        public double To;
        public int N;
        public Func<double, Vector2> Fn;

        public Vector2[] GetPoints()
        {
            var step = (To - From) / (N - 1);

            return Enumerable.Range(0, N).Select(i => Fn(From + step * i)).ToArray();
        }
    }
}
