using Model.Extensions;
using System;

namespace Model
{
    public struct Vector2 : IEquatable<Vector2>, INetKey
    {
        public double x;
        public double y;

        double INetKey.x => x;
        double INetKey.y => y;

        public double Len2 => x * x + y * y;
        public double Len => Math.Sqrt(Len2);
        public Vector2 Normed => this / Len;

        public Vector2 Normal => (y, -x);

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2((int, int) a)
        {
            x = a.Item1;
            y = a.Item2;
        }

        public Vector2 Scale(Vector2 v) => new Vector2(x * v.x, y * v.y);

        public static Vector2 Zero => (0, 0);

        public static implicit operator Vector2((double a, double b) v)
        {
            return new Vector2 { x = v.a, y = v.b };
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2
            {
                x = a.x + b.x,
                y = a.y + b.y
            };
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2
            {
                x = a.x - b.x,
                y = a.y - b.y
            };
        }

        public static Vector2 operator *(Vector2 a, double k)
        {
            return new Vector2()
            {
                x = a.x * k,
                y = a.y * k
            };
        }

        public static Vector2 operator *(double k, Vector2 a)
        {
            return new Vector2()
            {
                x = a.x * k,
                y = a.y * k
            };
        }

        public static Vector2 operator /(Vector2 a, double k)
        {
            return new Vector2()
            {
                x = a.x / k,
                y = a.y / k
            };
        }

        public static double operator *(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2
            {
                x = -a.x,
                y = -a.y
            };
        }

        private const double Epsilon = 0.000000001;
        private const decimal EpsilonM = 0.000000001m;

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return (b.x - a.x).Abs() < Epsilon && (b.y - a.y).Abs() < Epsilon;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
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
            var x = Math.Round((decimal)this.x / EpsilonM) * EpsilonM;
            var y = Math.Round((decimal)this.y / EpsilonM) * EpsilonM;
            
            return HashCode.Combine(x, y);
        }
    }
}
