using Model.Extensions;
using System;

namespace Model.Libraries
{
    public static class Densities
    {
        public static Func<double, double> CentralDensity(double density)
        {
            return (double t) =>
            {
                var x = 2 * (t - 0.5);
                var y = x.Sign() * x.Abs().Pow(density);
                return 0.5 * y + 1;
            };
        }
    }
}
