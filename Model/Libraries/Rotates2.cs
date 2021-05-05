using System;

namespace Model.Libraries
{
    public static class Rotates2
    {
        public static Matrix2 Rotate(double angle) => new Matrix2
        {
            a00 = Math.Cos(angle),
            a01 = -Math.Sin(angle),
            a10 = Math.Sin(angle),
            a11 = Math.Cos(angle),
        };
    }
}
