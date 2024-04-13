using System.Linq;
using Model.Libraries;

namespace Model.Extensions
{
    public static class Line2Extensions
    {
        public static bool IsSectionIntersectedBy(this Line2 a, Line2 b)
        {
            return a.IsLeft(b.A) != a.IsLeft(b.B) && b.IsLeft(a.A) != b.IsLeft(a.B);
        }

        public static Vector2 IntersectionPoint(this Line2 a, Line2 b)
        {
            //a + (b - a) * (b - a) * (c - a) / (b - a).Len2;

            var m = new Matrix(new[] { a.Normal.values.ToArray(), b.Normal.values.ToArray() });
            var c = new Vector(a.A * a.Normal, b.A * b.Normal);
            var p = m.Kramer(c);

            return p.ToV2();
        }

        public static (bool success, Vector2 result) IntersectionPointChecked(this Line2 a, Line2 b, double epsilon = Values.Epsilon9)
        {
            var m = new Matrix([a.Normal.values.ToArray(), b.Normal.values.ToArray()]);
            var c = new Vector(a.A * a.Normal, b.A * b.Normal);
            var (success, p) = m.KramerChecked(c, epsilon);

            return (success, success ? p.ToV2() : default);
        }

        public static Vector2 ProjectionPoint(this Line2 l, Vector2 p)
        {
            return l.A + l.AB * ((p - l.A) * l.AB / l.Len2);
        }
    }
}
