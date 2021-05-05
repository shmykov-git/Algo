using Model.Extensions;
using Model3D.Extensions;
using System.Linq;

namespace Model.Libraries
{
    public static class Parquets
    {
        public static Shape2 Triangles(double tileLen) => ShiftParquet(tileLen, Tiles.Triangles);
        public static Shape2 Triangles2(double tileLen) => ShiftParquet(tileLen, Tiles.Triangles2);
        public static Shape2 Hexagon(double tileLen) => ShiftParquet(tileLen, Tiles.Hexagon);
        public static Shape2 PentagonalKershner8(double tileLen, double angleD) => ShiftParquet(tileLen, Tiles.PentagonalKershner8(angleD));

        public static Shape2 ShiftParquet(double tileLen, Tile tile)
        {
            var m = (int)(1 / (tile.Size.Y * tileLen));
            var n = (int)(1 / (tile.Size.X * tileLen));
            
            Vector2 Shift(int i, int j)
            {
                var x = (j).SelectRange(k => tile.ShiftX[k % tile.ShiftX.Length]).Sum();
                var y = (i).SelectRange(k => tile.ShiftY[k % tile.ShiftY.Length]).Sum();
                return x + y;
            }

            var points = (m, n).SelectRange((i, j) => tile.Points.Select(p => tileLen * (p + Shift(i, j)))).SelectMany(v => v).ToArray();
            var convexes = (m, n).SelectRange((i, j) => (i * n + j) * tile.Count).SelectMany(shift => tile.Convexes.Shift(shift)).ToArray();

            return new Shape2
            {
                Points = points.Centered(),
                Convexes = convexes
            }.Normalize();
        }
    }
}
