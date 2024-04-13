using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Model
{
    public struct Vector2 : IEquatable<Vector2>, INetKey
    {
        public const double Epsilon = 0.000000001;
        public const decimal EpsilonM = 0.000000001m;

        public double x;
        public double y;

        public IEnumerable<double> values
        {
            get
            {
                yield return x;
                yield return y;
            }
        }

        double INetKey.x => x;
        double INetKey.y => y;

        public double Len2 => x * x + y * y;
        public double Len => Math.Sqrt(Len2);
        public Vector2 Normed => this / Len;
        public Vector2 Round(int e) => (x.Round(e), y.Round(e));

        public Vector2 Normal => (y, -x);
        public Vector2 NormalM => (-y, x);
        public Vector2 ToLen(double len) => this * (len / Len);

        public Complex ToZ() => new Complex(x, y);

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
        public static Vector2 One => (1, 1);
        public static Vector2 OneX => (1, 0);
        public static Vector2 OneY => (0, 1);

        public static implicit operator Vector2((double a, double b) v)
        {
            return new Vector2 { x = v.a, y = v.b };
        }

        public static implicit operator Vector2((int a, int b) v)
        {
            return new Vector2 { x = v.a, y = v.b };
        }

        public static implicit operator Vector2((int a, double b) v)
        {
            return new Vector2 { x = v.a, y = v.b };
        }

        public static implicit operator Vector2((double a, int b) v)
        {
            return new Vector2 { x = v.a, y = v.b };
        }

        public static implicit operator (double x, double y)(Vector2 v)
        {
            return (v.x, v.y);
        }

        public static implicit operator Vector2(Complex v)
        {
            return new Vector2 { x = v.Real, y = v.Imaginary };
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

        public static double operator *(Vector2 a, Vector2 b) => a.MultS(b);

        public double MultS(Vector2 b)
        {
            return x * b.x + y * b.y;
        }

        public double Square(Vector2 b)
        {
            return x * b.y - y * b.x;
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2
            {
                x = -a.x,
                y = -a.y
            };
        }

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
            return $"({x.Round(8)}, {y.Round(8)})";
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
