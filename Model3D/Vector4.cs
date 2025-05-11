using System;
using System.Drawing;
using Model.Libraries;

namespace Model3D;

public struct Vector4
{
    private const double epsilon = Values.Epsilon9;
    private const double epsilonPow2 = epsilon * epsilon;

    public double x;
    public double y;
    public double z;
    public double w;

    public double Length2 => x * x + y * y + z * z + w * w;
    public double Length => Math.Sqrt(Length2);

    public Vector4(double x, double y, double z, double w = 1.0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Vector4(Color color)
    {
        x = color.R / 255.0;  // Red
        y = color.G / 255.0;  // Green
        z = color.B / 255.0;  // Blue
        w = color.A / 255.0;  // Alpha
    }

    public Vector4 MultV(Vector4 b) => new Vector4(x * b.x, y * b.y, z * b.z, w * b.w);
    public Vector4 MultV(Vector3 b) => new Vector4(x * b.x, y * b.y, z * b.z, w);

    public static bool operator ==(Vector4 a, Vector4 b) => (b - a).Length2 < epsilonPow2;

    public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);

    // Унарный минус
    public static Vector4 operator -(Vector4 v)
    {
        return new Vector4(-v.x, -v.y, -v.z, -v.w);
    }

    // Бинарный минус (вычитание)
    public static Vector4 operator -(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    }

    // Бинарное сложение
    public static Vector4 operator +(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }

    // Бинарное умножение на число
    public static Vector4 operator *(Vector4 v, double scalar)
    {
        return new Vector4(v.x * scalar, v.y * scalar, v.z * scalar, v.w * scalar);
    }

    // Умножение числа на вектор
    public static Vector4 operator *(double scalar, Vector4 v)
    {
        return v * scalar;
    }

    // Бинарное деление на число
    public static Vector4 operator /(Vector4 v, double scalar)
    {
        if (scalar == 0)
            throw new DivideByZeroException("Cannot divide by zero.");
        return new Vector4(v.x / scalar, v.y / scalar, v.z / scalar, v.w / scalar);
    }

    public override string ToString()
    {
        return $"Vector4({x}, {y}, {z}, {w})";
    }
}
