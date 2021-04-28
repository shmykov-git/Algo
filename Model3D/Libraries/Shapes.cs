using Aspose.ThreeD.Utilities;
using System.Linq;

namespace Model.Libraries
{
    public static class Shapes
    {
        public static Shape Chesss(int n) => new Shape
        {
            Points = Ranges.Range(n, n).SelectMany(pair => Poligons.Square(1).Points.Select(p => (p / 2 + new Vector2(pair)) / (n - 1) - new Vector2(0.5, 0.5))
                .Select(v => new Vector4(v.X, v.Y, 0, 1))).ToArray(),

            Convexes = Ranges.Range(n, n).Select(pair => 4 * n * pair.Item1 + 4 * pair.Item2).Select(i => new int[] { i, i + 1, i + 2, i + 3 }).ToArray()
        };
    }
}
