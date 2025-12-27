using Model.Extensions;
using System;
using System.Linq;

namespace Model.Libraries
{
    public static class Parquets
    {
        public static Shape2 Squares(int m, int n, double tileLen = 1) => ShiftParquet(tileLen, Tiles.Squares, m, n);
        public static Shape2 Triangles(int m, int n, double tileLen = 1) => ShiftParquet(tileLen, Tiles.Triangles, m, n);
        public static Shape2 Hexagons(int m, int n, double tileLen, bool normalize = true) => ShiftParquet(tileLen, Tiles.Hexagon, m, n, normalize);
        public static Shape2 Triangles(double tileLen) => ShiftParquet(tileLen, Tiles.Triangles);
        public static Shape2 Triangles2(double tileLen) => ShiftParquet(tileLen, Tiles.Triangles2);
        public static Shape2 Hexagon(double tileLen) => ShiftParquet(tileLen, Tiles.Hexagon);
        public static Shape2 PentagonalKershner8Native(double tileLen, double angleD, double dx = 1, double dy = 1, bool normalize = true) => ShiftParquet(tileLen, Tiles.PentagonalKershner8(angleD), dx, dy, normalize);

        public static Shape2 PentagonalKershner8(int n, int m, double angleD = 1.7, bool normalize = true)
        {
            var tileLen = 0.2;
            var tile = Tiles.PentagonalKershner8(angleD, normalize);
            var shiftX = tile.ShiftX.Center() * tileLen;
            var shiftY = tile.ShiftY.Center() * tileLen;
            var angle = Math.Atan2(shiftX.y, shiftX.x);
            var paruet = ShiftParquet(tileLen, tile, n * Math.PI / 3, Math.PI / 3, normalize);
            var mult = 2 * Math.PI / (n * shiftX.Len);
            var shape = paruet.Mult(mult).Rotate(-angle);

            var dY = Rotates2.Rotate(-angle) * shiftY * mult;
            var planeShape = (m).SelectRange(i => shape.Move(i * dY)).Aggregate((a, b) => a.Join(b));

            return normalize ? planeShape.Normalize() : planeShape;
        }

        public static Shape2 ShiftParquet(double tileLen, Tile tile, int m, int n, bool normalize = true)
        {
            Vector2 Shift(int i, int j)
            {
                var x = (j).SelectRange(k => tile.ShiftX[k % tile.ShiftX.Length]).Sum();
                var y = (i).SelectRange(k => tile.ShiftY[k % tile.ShiftY.Length]).Sum();
                return x + y;
            }

            var points = (m, n).SelectRange((i, j) => tile.Points.Select(p => tileLen * (p + Shift(i, j)))).SelectMany(v => v).ToArray();
            var convexes = (m, n).SelectRange((i, j) => (i * n + j) * tile.Count).SelectMany(shift => tile.Convexes.Shift(shift)).ToArray();

            var s = new Shape2
            {
                Points = points.Centered(),
                Convexes = convexes
            };

            return normalize ? s.Normalize() : s;
        }

        public static Shape2 ShiftParquet(double tileLen, Tile tile, double dx = 1, double dy = 1, bool normalize = true)
        {
            var m = (int)(dy / (tile.Size.y * tileLen));
            var n = (int)(dx / (tile.Size.x * tileLen));

            return ShiftParquet(tileLen, tile, m, n, normalize);
        }
    }
}
