namespace Model.Extensions
{

    public static class PointExtensions
    {
        public static Point Scale(this Point a, Size aSize, Size bSize)
        {
            return new Point
            {
                X = a.X * bSize.Width / aSize.Width,
                Y = a.Y * bSize.Height / aSize.Height
            };
        }
    }
}
