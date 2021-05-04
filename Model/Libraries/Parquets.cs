using Model.Extensions;
using Model3D.Extensions;
using System.Linq;

namespace Model.Libraries
{
    public static class Parquets
    {
        public static Shape2 Triangles(double tileLen) => ShiftParquet(tileLen, Tiles.Triangles);

        public static Shape2 ShiftParquet(double tileLen, Tile tile)
        {
            var m = (int)(1 / (tile.Size.Y * tileLen));
            var n = (int)(1 / (tile.Size.X * tileLen));

            var points = (m, n).SelectRange((i, j) => tile.Points.Select(p => tileLen * (p + j * tile.ShiftX + i * tile.ShiftY))).SelectMany(v => v).ToArray();
            var convexes = (m, n).SelectRange((i, j) => (i * n + j) * tile.Count).SelectMany(shift => tile.Convexes.Shift(shift)).ToArray();

            return new Shape2
            {
                Points = points.Centered(),
                Convexes = convexes
            }.Normalize();
        }
    }
}
