using Model.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class Polygon
    {
        public Vector2 this[int i] => Points[i];

        public Vector2[] Points;
        public IEnumerable<Line2> Lines => Points.SelectCirclePair((a, b) => new Line2(a, b));

        public double MaxLinesLen => Points.Length > 1 ? Lines.Max(l => l.Len) : 0;
        public double MinLinesLen => Points.Length > 1 ? Lines.Min(l => l.Len) : 0;

        public (Vector2 a, Vector2 b) Border => Points.Length > 0
            ? 
            (
                new Vector2(Points.Min(p => p.x), Points.Min(p => p.y)),
                new Vector2(Points.Max(p => p.x), Points.Max(p => p.y))
            )
            : (Vector2.Zero, Vector2.Zero);

        public Vector2 Size
        {
            get
            {
                var b = Border;

                return b.b - b.a;
            }
        }
    }
}
