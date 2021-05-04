using System;

namespace Model
{
    public class PolygonFillException : Exception
    {
        public readonly int[][] IncerrectConvexes;

        public PolygonFillException()
        {
        }

        public PolygonFillException(int[][] convexes)
        {
            this.IncerrectConvexes = convexes;
        }
    }
}
