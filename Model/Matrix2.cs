namespace Model
{
    public struct Matrix2
    {
        public double a00;
        public double a01;
        public double a10;
        public double a11;

        public static Vector2 operator *(Matrix2 m, Vector2 v)
        {
            return new Vector2(v.X * m.a00 + v.Y * m.a01, v.X * m.a10 + v.Y * m.a11);
        }
    }
}
