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
        public Func<double, double> TFn = t => t;
        public bool Closed;

        public Vector2[] GetPoints()
        {
            double n = (Closed ? N : (N - 1));
            var step = (To - From) / n;

            return Enumerable.Range(0, N).Select(i => Fn(From + step * n * TFn(i/n))).ToArray();
        }
    }
}
