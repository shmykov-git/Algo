using System;

namespace Model3D.AsposeModel;

public struct Vector3
{
    public double x;
    public double y;
    public double z;

    public static readonly Vector3 Origin = new Vector3(0, 0, 0);

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

    public void Normalize()
    {
        double length = Math.Sqrt(x * x + y * y + z * z);
        if (length > 1e-6)
        {
            x /= length;
            y /= length;
            z /= length;
        }
    }

    public static double Dot(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
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
