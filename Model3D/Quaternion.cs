using System;
using Model.Libraries;

namespace Model3D;

public struct Quaternion
{
    private const double Epsilon = Values.Epsilon9;
    private const double EpsilonPow2 = Epsilon * Epsilon;
    private const double Epsilon1 = 1 - Epsilon;

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

    public double Length2 => x * x + y * y + z * z + w * w;
    public double Length => Math.Sqrt(Length2);

    public Quaternion Normalize()
    {
        double length = Length;

        if (length > Epsilon)
        {
            return new Quaternion(x / length, y / length, z / length, w / length);
        }

        return this;
    }

    public Quaternion Conjugate()
    {
        return new Quaternion(-x, -y, -z, w);
    }

    public static Quaternion FromAngleAxis(double angle, Vector3 axis)
    {
        Vector3 normalizedAxis = axis.Normalize();

        double halfAngleRad = 0.5 * angle;

        var sin = Math.Sin(halfAngleRad);
        var cos = Math.Cos(halfAngleRad);

        return new Quaternion(
            normalizedAxis.x * sin,
            normalizedAxis.y * sin,
            normalizedAxis.z * sin,
            cos
        );
    }

    // same as Aspose, checked
    public static Quaternion FromRotation(Vector3 from, Vector3 to)
    {
        if (from.Length2 < EpsilonPow2 || to.Length2 < EpsilonPow2)
            return Identity;

        Vector3 f = from.Normalize();
        Vector3 t = to.Normalize();

        double dot = Vector3.Dot(f, t);
        if (dot >= 1.0)
        {
            return Identity;
        }

        if (dot < -Epsilon1)
        {
            Vector3 orthogonal = new Vector3(1, 0, 0);
            orthogonal = Vector3.Cross(f, orthogonal);

            if (orthogonal.Length < Epsilon)
            {
                orthogonal = new Vector3(0, 1, 0);
                orthogonal = Vector3.Cross(f, orthogonal);
            }

            return FromAngleAxis(Math.PI, orthogonal.Normalize());
        }

        Vector3 cross = Vector3.Cross(f, t);
        double s = Math.Sqrt((1 + dot) * 2);
        double invS = 1 / s;

        return new Quaternion(
            cross.x * invS,
            cross.y * invS,
            cross.z * invS,
            s * 0.5
        ).Normalize();
    }

    public static Quaternion FromEulerAngle(double pitch, double yaw, double roll)
    {
        double halfPitch = pitch * 0.5;
        double halfYaw = yaw * 0.5;
        double halfRoll = roll * 0.5;

        double sinPitch = Math.Sin(halfPitch);
        double cosPitch = Math.Cos(halfPitch);
        double sinYaw = Math.Sin(halfYaw);
        double cosYaw = Math.Cos(halfYaw);
        double sinRoll = Math.Sin(halfRoll);
        double cosRoll = Math.Cos(halfRoll);

        double x = sinPitch * cosYaw * cosRoll - cosPitch * sinYaw * sinRoll;
        double y = cosPitch * sinYaw * cosRoll + sinPitch * cosYaw * sinRoll;
        double z = cosPitch * cosYaw * sinRoll - sinPitch * sinYaw * cosRoll;
        double w = cosPitch * cosYaw * cosRoll + sinPitch * sinYaw * sinRoll;

        return new Quaternion(x, y, z, w);
    }

    public static Quaternion FromEulerAngle(Vector3 angles)
    {
        return FromEulerAngle(angles.x, angles.y, angles.z);
    }

    public Quaternion Inverse()
    {
        double lengthSq = Length2;

        if (lengthSq > EpsilonPow2)
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

    public static Quaternion operator *(Quaternion a, Quaternion b) =>
        new Quaternion(
            a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
            a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
            a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
            a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
        );

    public static Vector4 operator *(Quaternion rotation, Vector4 v)
    {
        // Представляем вектор как кватернион с w = 0
        var q = new Quaternion(v.x, v.y, v.z, 0);

        // q * v * q⁻¹
        Quaternion result = rotation * q * rotation.Inverse();

        return new Vector4(result.x, result.y, result.z, v.w); // сохраняем w компоненты
    }

    // q * v * q⁻¹, optimized
    public static Vector3 operator *(Quaternion q, Vector3 v)
    {
        double vx = v.x, vy = v.y, vz = v.z;
        double qx = q.x, qy = q.y, qz = q.z, qw = q.w;

        double num = qx * 2.0;
        double num2 = qy * 2.0;
        double num3 = qz * 2.0;
        double num4 = qx * num;
        double num5 = qy * num2;
        double num6 = qz * num3;
        double num7 = qx * num2;
        double num8 = qx * num3;
        double num9 = qy * num3;
        double num10 = qw * num;
        double num11 = qw * num2;
        double num12 = qw * num3;

        return new Vector3(
            (1.0 - (num5 + num6)) * vx + (num7 - num12) * vy + (num8 + num11) * vz,
            (num7 + num12) * vx + (1.0 - (num4 + num6)) * vy + (num9 - num10) * vz,
            (num8 - num11) * vx + (num9 + num10) * vy + (1.0 - (num4 + num5)) * vz
        );
    }

    public static Vector3 operator *(Vector3 v, Quaternion q) => q.Inverse() * v;
}
