using System;

namespace Model
{
    public class PolygonFillException : Exception
    {
        public readonly int[][] IntersectConvexes;

        public PolygonFillException()
        {
        }

        public PolygonFillException(int[][] convexes)
        {
            this.IntersectConvexes = convexes;
        }
    }
}
