using System.Collections.Generic;
using System.Linq;

namespace Model.Extensions
{

    public static class Vector2Extensions
    {
        public static Vector2 Scale(this Vector2 a, Size aSize, Size bSize)
        {
            return new Vector2
            {
                X = a.X * bSize.Width / aSize.Width,
                Y = a.Y * bSize.Height / aSize.Height
            };
        }

        public static Vector2 Center(this IEnumerable<Vector2> vectors)
        {
            var sum = Vector2.Zero;
            var count = 0;
            foreach(var v in vectors)
            {
                sum += v;
                count++;
            }
            
            return sum / count;
        }

        public static Vector2 Sum(this IEnumerable<Vector2> vectors)
        {
            return vectors.Aggregate(Vector2.Zero, (a, b) => a + b);
        }

        public static Vector2[] Centered(this Vector2[] vectors)
        {
            var center = vectors.Center();
            return vectors.Select(v => v - center).ToArray();
        }
    }
}
