using System.Linq;

namespace Model.Extensions
{
    public static class Shape2Extensions
    {
        public static Shape2 Normalize(this Shape2 shape)
        {
            var points = shape.Points.Distinct().ToList();
            var convexes = shape.Convexes.Select(convex => convex.Select(i => points.IndexOf(shape.Points[i])).ToArray()).ToArray();

            return new Shape2
            {
                Points = points.ToArray(),
                Convexes = convexes
            };
        }
    }
}
