using System;

namespace Model.Libraries
{
    public static class Angle
    {
        public static double LeftDirection(Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = (b - a).Normed;
            var bc = (c - b).Normed;

            return Math.Atan2(ab.NormalM * bc, ab * bc);
        }

        public static bool IsLeftDirection(Vector2 a, Vector2 b, Vector2 c) => LeftDirection(a, b, c) >= 0;
    }
}