using System.Numerics;
using MathNet.Numerics;

namespace Model4D;

// https://codeforces.com/blog/entry/46744?locale=ru
public struct Quaternion
{
    public Complex A;
    public Complex B;

    public double x => A.Real;
    public double y => A.Imaginary;
    public double z => B.Real;
    public double w => B.Imaginary;
    public Aspose.ThreeD.Utilities.Vector3 V3 => new(x, y, z);

    public Quaternion(double x, double y, double z, double w)
    {
        A = new Complex(x, y);
        B = new Complex(z, w);
    }

    public Quaternion(Complex a, Complex b)
    {
        A = a;
        B = b;
    }

    public double Len2 => A.MagnitudeSquared() + B.MagnitudeSquared();

    public static Quaternion operator *(Quaternion a, Quaternion b)
    {
        return new Quaternion(
            a.A * b.A - a.B * b.B.Conjugate(),
            a.A * b.B + a.B * b.A.Conjugate()
        );
    }

    public static Quaternion operator +(Quaternion a, Quaternion b)
    {
        return new Quaternion(a.A + b.A, a.B + b.B);
    }
}