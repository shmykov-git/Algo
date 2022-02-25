using System.Collections.Generic;

namespace Model.Fourier
{
    public enum FrStrategy
    {
        Fill,
        Split,
        Radius,
    }

    public class FrOptions
    {
        public FrStrategy Strategy = FrStrategy.Fill;

        public (double x, double y, double r)[] Excludes;
        public (double x, double y, double r)[] Includes;
    }
}