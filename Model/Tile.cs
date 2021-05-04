using System;

namespace Model
{
    public class Tile
    {
        public Vector2 ShiftX;
        public Vector2 ShiftY;
        public Vector2 Size => ShiftX + ShiftY;
        public int Count => Points.Length;
        public Vector2[] Points;
        public int[][] Convexes;
    }
}
