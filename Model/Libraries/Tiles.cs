using Model.Extensions;

namespace Model.Libraries
{
    public static class Tiles
    {
        private static readonly double h3 = 3.0.Sqrt() / 2;

        public static Tile Triangles => new Tile
        {
            Shift = (1, 2*h3),
            Points = new Vector2[]
            {
                (0, 0), (1, 0), 
                (0.5, h3), (1.5, h3),
                (0, 2*h3), (1, 2*h3)
            },
            Convexes = new int[][]
            {
                new int[] {0, 1, 2},
                new int[] {1, 3, 2},
                new int[] {2, 5, 4},
                new int[] {2, 3, 5}
            }
        };
    }
}
