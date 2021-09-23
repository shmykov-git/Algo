using Model.Extensions;
using System;

namespace Model
{
    public struct Vector3D : IEquatable<Vector3D>
    {
        public double x;
        public double y;
        public double z;

        public static Vector3D Zero = new Vector3D(0, 0, 0);

        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        private const double Epsilon = 0.000001;
        private const decimal EpsilonM = 0.000001m;

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
            var hashX = Math.Abs(x) < Epsilon ? 0m : Math.Round((decimal)x / EpsilonM) * EpsilonM;
            var hashY = Math.Abs(y) < Epsilon ? 0m : Math.Round((decimal)y / EpsilonM) * EpsilonM;
            var hashZ = Math.Abs(z) < Epsilon ? 0m : Math.Round((decimal)z / EpsilonM) * EpsilonM;

            return HashCode.Combine(hashX, hashY, hashZ);
        }
    }
}
