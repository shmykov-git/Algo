using Model.Extensions;
using System.Collections.Generic;

namespace Model
{

    public class Poligon
    {
        public Vector2[] Points;
        public IEnumerable<Line2> Lines => Points.SelectCirclePair((a, b) => new Line2(a, b));
    }
}
