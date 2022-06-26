using System;
using Aspose.ThreeD.Utilities;

namespace Model3D.Extensions
{
    public static class ValueTupleExtensions
    {
        public static Vector3 ToCircleYV3(this (int, int) v, double alfa0 = 0)
        {
            var (i, n) = v;
            var alfa = 2 * Math.PI * i / n + alfa0;

            return new Vector3(Math.Cos(alfa), 0, Math.Sin(alfa));
        }
    }
}