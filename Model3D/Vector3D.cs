using Model.Extensions;
using System;

namespace Model
{
    public struct Vector3D : IEquatable<Vector3D>
    {
        public double x;
        public double y;
        public double z;

        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        private const double Epsilon = 0.000000001;
        private const decimal EpsilonM = 0.000000001m;

        public static bool operator ==(Vector3D a, Vector3D b)
        {
            return (b.x - a.x).Abs() < Epsilon && (b.y - a.y).Abs() < Epsilon && (b.z - a.z).Abs() < Epsilon;
        }

        public static bool operator !=(Vector3D a, Vector3D b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"({x:0.##}, {y:0.##}, {z:0.##})";
        }

        public bool Equals(Vector3D other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return Equals((Vector3D)obj);
        }

        public override int GetHashCode()
        {
            var hashX = Math.Round((decimal)x / EpsilonM) * EpsilonM;
            var hashY = Math.Round((decimal)y / EpsilonM) * EpsilonM;
            var hashZ = Math.Round((decimal)z / EpsilonM) * EpsilonM;

            return HashCode.Combine(hashX, hashY, hashZ);
        }
    }
}
