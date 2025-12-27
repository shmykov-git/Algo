using Model.Extensions;

namespace Model
{
    public class Tile
    {
        public Vector2[] ShiftX;
        public Vector2[] ShiftY;
        public Vector2 Size => (ShiftX.Center().Len, ShiftY.Center().Len);
        public int Count => Points.Length;
        public Vector2[] Points;
        public int[][] Convexes;
    }
}
