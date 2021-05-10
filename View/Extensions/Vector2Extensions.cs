using Model;
using System.Drawing;

namespace View.Extensions
{
    static class Vector2Extensions
    {
        public static Rectangle ToRectangle(this Vector2 p, Vector2 s)
        {
            var halfS = s * 0.5;

            return new Rectangle((int)(p.X - halfS.X), (int)(p.Y - halfS.Y), (int)s.X, (int)s.Y);
        }

        public static Point ToPoint(this Model.Vector2 p)
        {
            return new Point((int)p.X, (int)p.Y);
        }
    }
}
