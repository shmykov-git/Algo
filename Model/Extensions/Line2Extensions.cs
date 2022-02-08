using System.Linq;

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
            var m = new Matrix(new[] { a.Normal.values.ToArray(), b.Normal.values.ToArray() });
            var c = new Vector(a.A * a.Normal, b.A * b.Normal);
            var p = m.Kramer(c);
            
            return p.ToV2();
        }
    }
}
