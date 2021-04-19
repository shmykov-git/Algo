using System.Drawing;

namespace View.Extensions
{
    static class PointExtensions
    {
        public static Rectangle ToRectangle(this Model.Point p, Model.Size s)
        {
            var halfS = s * 0.5;

            return new Rectangle((int)(p.X - halfS.Width), (int)(p.Y - halfS.Height), (int)s.Width, (int)s.Height);
        }
        public static Point ToPoint(this Model.Point p)
        {
            return new Point((int)p.X, (int)p.Y);
        }
    }
}
