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
            return new Vector2(v.x * m.a00 + v.y * m.a01, v.x * m.a10 + v.y * m.a11);
        }
    }
}
