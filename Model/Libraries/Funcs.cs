using System;

namespace Model.Libraries
{
    public delegate double Func1(double x);

    public static class Funcs
    {
        public static Func1 NormalDistribution() => x => Math.Exp(-x * x) / Math.Sqrt(Math.PI);

        public static Func1 ParametricNormDistribution(double mu, double sigma) 
        {
            var fi = NormalDistribution();

            return x => fi((x - mu) / sigma) / sigma; 
        }
    }
}
