using Model.Extensions;
using System;

namespace Model
{
    public class Tile
    {
        public Vector2[] ShiftX;
        public Vector2[] ShiftY;
        public Vector2 Size => ShiftX.Center() + ShiftY.Center();
        public int Count => Points.Length;
        public Vector2[] Points;
        public int[][] Convexes;
    }
}
