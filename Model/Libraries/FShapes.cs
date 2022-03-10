using System;
using System.Linq;
using Model.Extensions;
using Model.Fourier;

namespace Model.Libraries
{
    public static class FShapes
    {
        // todo: большой радиус - малая частота, малый радиус - большая частота
        public static Fr[] Generate(int seed, int count, int range, params Fr[] frs)
        {
            var rnd = new Random(seed);

            var res = (count).SelectRange(_ =>
            {
                var n = rnd.Next(2 * range + 1) - range;
                var rr = (range - n.Abs());
                var r = rnd.Next(2 * rr + 1) - rr;

                return new Fr
                {
                    n = n,
                    r = r,
                };
            }).Concat(frs).ToArray();

            res.WriteToDebug("generated frs: ");

            return res;
        }
    }
}
