using Model3D.AsposeModel;
using Model.Extensions;
using Model3D.Libraries;
using System;
using System.Linq;

namespace Model
{
    public class Func3Info
    {
        public double From;
        public double To;
        public int N;
        public Func3 Fn;
        public Func<double, double> TFn = t => t;
        public bool Closed;

        public Vector3[] GetPoints()
        {
            double n = (Closed ? N : (N - 1));
            var step = (To - From) / n;

            return (N).SelectRange(i => Fn(From + step * n * TFn(i / n))).ToArray();
        }
    }
}
