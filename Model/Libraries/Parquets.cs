using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Linq;

namespace Model.Libraries
{
    public static class Parquets
    {
        public static Shape2 Triangles(double tileLen) => ShiftParquet(tileLen, Tiles.Triangles);
        public static Shape2 Triangles2(double tileLen) => ShiftParquet(tileLen, Tiles.Triangles2);
        public static Shape2 Hexagon(double tileLen) => ShiftParquet(tileLen, Tiles.Hexagon);
        public static Shape2 PentagonalKershner8(double tileLen, double angleD, double dx = 1, double dy = 1) => ShiftParquet(tileLen, Tiles.PentagonalKershner8(angleD), dx, dy);
        
        //todo: shift angle
        public static Shape2 PentagonalKershner8ForTube(int n, double angleD)
        {
            var tileLen = 0.2;
            var tile = Tiles.PentagonalKershner8(angleD);
            var shift = tile.ShiftX.Center() * 0.2;
            var angle = Math.Atan2(shift.Y, shift.X);
            var paruet = ShiftParquet(tileLen, tile, n * Math.PI / 3, Math.PI / 3);
            var shape = paruet.Mult(2 * Math.PI / (n * shift.Len)).Rotate(-angle);
            
            return shape;
        }

        public static Shape2 ShiftParquet(double tileLen, Tile tile, double dx = 1, double dy = 1)
        {
            var m = (int)(dy / (tile.Size.Y * tileLen));
            var n = (int)(dx / (tile.Size.X * tileLen));
            
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
