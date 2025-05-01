using System;

namespace Model3D.AsposeModel;

public struct Quaternion
{
    public double x;
    public double y;
    public double z;
    public double w;

    public static readonly Quaternion Identity = new Quaternion(0, 0, 0, 1);

    public Quaternion(double x, double y, double z, double w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public double Length
    {
        get
        {
            return Math.Sqrt(x * x + y * y + z * z + w * w);
        }
    }

    public Quaternion Normalize()
    {
        double length = Length;
        if (length > 1e-6)
        {
            return new Quaternion(x / length, y / length, z / length, w / length);
        }
        return this;
    }

    public Quaternion Conjugate()
    {
        return new Quaternion(-x, -y, -z, w);
    }

    public Quaternion Inverse()
    {
        double lengthSq = x * x + y * y + z * z + w * w;
        if (lengthSq > 1e-6)
        {
            double invLengthSq = 1.0 / lengthSq;
            return new Quaternion(-x * invLengthSq, -y * invLengthSq, -z * invLengthSq, w * invLengthSq);
        }
        return this;
    }

    public static double Dot(Quaternion a, Quaternion b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    }

    public override bool Equals(object obj)
    {
        if (obj is Quaternion q)
        {
            return x == q.x && y == q.y && z == q.z && w == q.w;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z, w);
    }

    public override string ToString()
    {
        return $"Quaternion({x}, {y}, {z}, {w})";
    }

    public static Quaternion operator +(Quaternion a, Quaternion b)
    {
        return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }

    public static Quaternion operator -(Quaternion a, Quaternion b)
    {
        return new Quaternion(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    }

    public static Quaternion operator *(Quaternion q, double scalar)
    {
        return new Quaternion(q.x * scalar, q.y * scalar, q.z * scalar, q.w * scalar);
    }

    public static Quaternion operator *(double scalar, Quaternion q)
    {
        return q * scalar;
    }

    public static Quaternion operator /(Quaternion q, double scalar)
    {
        return new Quaternion(q.x / scalar, q.y / scalar, q.z / scalar, q.w / scalar);
    }

    public static bool operator ==(Quaternion a, Quaternion b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Quaternion a, Quaternion b)
    {
        return !a.Equals(b);
    }
}
