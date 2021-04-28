using System;

namespace Model
{
    public struct Vector2
    {
        public double X;
        public double Y;

        public double Len2 => X * X + Y * Y;
        public double Len => Math.Sqrt(Len2);
        public Vector2 Normed => this / Len;

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2((int, int) a)
        {
            X = a.Item1;
            Y = a.Item2;
        }

        public static implicit operator Vector2((double, double) a)
        {
            return new Vector2 { X = a.Item1, Y = a.Item2 };
        }

        public static Vector2 operator +(Vector2 a, Size b)
        {
            return new Vector2
            {
                X = a.X + b.Width,
                Y = a.Y + b.Height
            };
        }
        public static Vector2 operator -(Vector2 a, Size b)
        {
            return new Vector2
            {
                X = a.X - b.Width,
                Y = a.Y - b.Height
            };
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };
        }

        public static Vector2 operator *(Vector2 a, double k)
        {
            return new Vector2()
            {
                X = a.X * k,
                Y = a.Y * k
            };
        }

        public static Vector2 operator *(double k, Vector2 a)
        {
            return new Vector2()
            {
                X = a.X * k,
                Y = a.Y * k
            };
        }

        public static Vector2 operator /(Vector2 a, double k)
        {
            return new Vector2()
            {
                X = a.X / k,
                Y = a.Y / k
            };
        }

        public static double operator *(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
