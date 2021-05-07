namespace Model.Extensions
{
    public static class Line2Extensions
    {
        public static bool IsSectionIntersectedBy(this Line2 a, Line2 b)
        {
            return a.IsLeft(b.A) != a.IsLeft(b.B) && b.IsLeft(a.A) != b.IsLeft(a.B);
        }
    }
}
