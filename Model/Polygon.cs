using System.Collections;
using Model.Extensions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Model
{
    public class Polygon : IEnumerable<Vector2>
    {
        public Vector2 this[int i] => Points[i];

        public Vector2[] Points;
        public IEnumerable<Line2> Lines => Points.SelectCirclePair((a, b) => new Line2(a, b));

        public double Len => Points.Length > 1 ? Points.SelectCirclePair((a, b) => (b - a).Len).Sum() : 0;
        
        public double Square
        {
            get
            {
                var c = Points.Center();

                return 0.5 * Points.SelectCirclePair((a, b) => (a - c).Square(b - a)).Sum();
            }
        }

        public double FormPerfect => 4 * Math.PI * Square.Abs() / Len.Pow2();

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

        public Vector2 Center => Points.Center();
        public IEnumerator<Vector2> GetEnumerator()
        {
            foreach (var point in Points)
                yield return point;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
