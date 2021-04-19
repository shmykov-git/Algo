using Model.Extensions;
using System.Collections.Generic;

namespace Model
{

    public class Poligon
    {
        public Point[] Points;
        public IEnumerable<Line> Lines => Points.SelectCirclePair((a, b) => new Line(a, b));
    }
}
