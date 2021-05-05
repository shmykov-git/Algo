using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Tools
{
    public static class SimpleMinimizer
    {
        public static double Minimize(double x0, double dx0, double epsilon, Func<double, double> fn)
        {
            var x = x0;
            var dx = dx0;
            while (fn(x) > epsilon)
            {
                var y1 = fn(x);
                var y2 = fn(x + dx);
                var y3 = fn(x - dx);

                if (y1 < y2)
                    if (y1 < y3)
                        dx = dx / 2;
                    else
                        x = x - dx;
                else
                    x = x + dx;
            }

            return x;
        }
    }
}
