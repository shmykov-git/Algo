using Model.Extensions;
using System;

namespace Model
{
    public struct Vector2 : IEquatable<Vector2>
    {
        public double X;
        public double Y;

        public double Len2 => X * X + Y * Y;
        public double Len => Math.Sqrt(Len2);
        public Vector2 Normed => this / Len;

        public Vector2 Normal => (Y, -X);

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

        public Vector2 Scale(Vector2 v) => new Vector2(X * v.X, Y * v.Y);

        public static Vector2 Zero => (0, 0);

        public static implicit operator Vector2((double a, double b) v)
        {
            return new Vector2 { X = v.a, Y = v.b };
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

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2
            {
                X = -a.X,
                Y = -a.Y
            };
        }

        private const double Epsilon = 0.000000001;
        private const decimal EpsilonM = 0.000000001m;

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return (b.X - a.X).Abs() < Epsilon && (b.Y - a.Y).Abs() < Epsilon;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return !(a == b);
        }

        private bool Abs()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public bool Equals(Vector2 other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return Equals((Vector2)obj);
        }

        public override int GetHashCode()
        {
            var x = Math.Round((decimal)X / EpsilonM) * EpsilonM;
            var y = Math.Round((decimal)Y / EpsilonM) * EpsilonM;
            
            return HashCode.Combine(x, y);
        }
    }
}
