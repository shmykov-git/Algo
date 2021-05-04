using System;

namespace Model
{
    public class Tile
    {
        public Vector2 Shift;
        public int Count => Points.Length;
        public Vector2[] Points;
        public int[][] Convexes;
    }
}
