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

        public (Vector2 min, Vector2 max) Border => (
            new Vector2(Points.Min(p => p.x), Points.Min(p => p.y)),
            new Vector2(Points.Max(p => p.x), Points.Max(p => p.y)));

        public Vector2 Size
        {
            get
            {
                var b = Border;

                return b.max - b.min;
            }
        }
    }
}
