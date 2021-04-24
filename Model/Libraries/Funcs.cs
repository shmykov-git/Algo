using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Model.Libraries
{
    public static class Funcs2
    {
        public static Vector2 Spiral(double t) => (t * Math.Sin(t), t * Math.Cos(t));
    }
}
