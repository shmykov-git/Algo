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

    public static Quaternion FromAngleAxis(double angle, Vector3 axis)
    {
        axis.Normalize();
        double halfAngle = angle * 0.5;
        double sinHalfAngle = Math.Sin(halfAngle);
        double cosHalfAngle = Math.Cos(halfAngle);
        return new Quaternion(
            axis.x * sinHalfAngle,
            axis.y * sinHalfAngle,
            axis.z * sinHalfAngle,
            cosHalfAngle
        );
    }

    public static Quaternion FromRotation(Vector3 from, Vector3 to)
    {
        Vector3 f = from;
        Vector3 t = to;
        f.Normalize();
        t.Normalize();

        double dot = Vector3.Dot(f, t);
        if (dot >= 1.0)
        {
            return Identity;
        }

        if (dot < -0.999999)
        {
            Vector3 orthogonal = new Vector3(1, 0, 0);
            orthogonal = Vector3.Cross(f, orthogonal);
            if (orthogonal.Length < 1e-6)
            {
                orthogonal = new Vector3(0, 1, 0);
                orthogonal = Vector3.Cross(f, orthogonal);
            }
            orthogonal.Normalize();
            return FromAngleAxis(Math.PI, orthogonal);
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

    public static Vector3 operator *(Quaternion rotation, Vector3 point)
    {
        // Кватернион-представление вектора (с w = 0)
        Quaternion v = new Quaternion(point.x, point.y, point.z, 0);

        // q * v * q⁻¹
        Quaternion q = rotation;
        Quaternion qInv = rotation.Inverse();

        Quaternion result = q * v * qInv;
        return new Vector3(result.x, result.y, result.z);
    }

    public static Quaternion operator *(Quaternion a, Quaternion b)
    {
        return new Quaternion(
            a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
            a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
            a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
            a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
        );
    }

    public static Vector4 operator *(Quaternion rotation, Vector4 v)
    {
        // Представляем вектор как кватернион с w = 0
        Quaternion q = new Quaternion(v.x, v.y, v.z, 0);

        // q * v * q⁻¹
        Quaternion qInv = rotation.Inverse();

        Quaternion result = rotation * q * qInv;
        return new Vector4(result.x, result.y, result.z, v.w); // сохраняем w компоненты
    }

    public static Vector3 operator *(Vector3 v, Quaternion rotation)
    {
        // Представляем вектор как кватернион с w = 0
        Quaternion q = new Quaternion(v.x, v.y, v.z, 0);

        // v * q = q * v * q⁻¹ (используем кватернион с обратным умножением)
        Quaternion qInv = rotation.Inverse();

        Quaternion result = q * rotation * qInv;
        return new Vector3(result.x, result.y, result.z);
    }
}
