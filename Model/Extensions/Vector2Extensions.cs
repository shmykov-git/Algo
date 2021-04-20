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
    }
}
