using System;
using Model.Extensions;
using Model.Libraries;

namespace Model3D.AsposeModel;

public struct Vector3
{
    public double x;
    public double y;
    public double z;

    public double Length2 => x * x + y * y + z * z;
    public double Length => Math.Sqrt(Length2);

    public static readonly Vector3 Origin = new Vector3(0, 0, 0);
    public static readonly Vector3 XAxis = new Vector3(1, 0, 0);
    public static readonly Vector3 YAxis = new Vector3(0, 1, 0);
    public static readonly Vector3 ZAxis = new Vector3(0, 0, 1);

    public Vector3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void Set(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 Normalize()
    {
        var l2 = Length2;

        if ((l2 - 1).Abs() < Values.Epsilon9)
            return this;

        return this / Math.Sqrt(l2);
    }

    public static double Dot(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static Vector3 Cross(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }
    public override bool Equals(object obj)
    {
        if (obj is Vector3 v)
        {
            return x == v.x && y == v.y && z == v.z;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }

    public override string ToString()
    {
        return $"Vector3({x}, {y}, {z})";
    }


    public static Vector3 operator -(Vector3 v)
    {
        return new Vector3(-v.x, -v.y, -v.z);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3 operator *(Vector3 v, double scalar)
    {
        return new Vector3(v.x * scalar, v.y * scalar, v.z * scalar);
    }

    public static Vector3 operator *(double scalar, Vector3 v)
    {
        return v * scalar;
    }

    public static Vector3 operator /(Vector3 v, double scalar)
    {
        return new Vector3(v.x / scalar, v.y / scalar, v.z / scalar);
    }

    public static bool operator ==(Vector3 a, Vector3 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Vector3 a, Vector3 b)
    {
        return !a.Equals(b);
    }
}
