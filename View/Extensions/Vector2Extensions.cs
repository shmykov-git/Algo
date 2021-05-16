using Model;
using System.Drawing;

namespace View.Extensions
{
    static class Vector2Extensions
    {
        public static Rectangle ToRectangle(this Vector2 p, Vector2 s)
        {
            var halfS = s * 0.5;

            return new Rectangle((int)(p.x - halfS.x), (int)(p.y - halfS.y), (int)s.x, (int)s.y);
        }

        public static Point ToPoint(this Model.Vector2 p)
        {
            return new Point((int)p.x, (int)p.y);
        }
    }
}
