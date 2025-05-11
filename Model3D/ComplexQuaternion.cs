using System.Numerics;
using MathNet.Numerics;

namespace Model4D;

// https://codeforces.com/blog/entry/46744?locale=ru
public struct ComplexQuaternion
{
    public Complex A;
    public Complex B;

    public double x => A.Real;
    public double y => A.Imaginary;
    public double z => B.Real;
    public double w => B.Imaginary;
    public Model3D.Vector3 V3 => new(x, y, z);

    public ComplexQuaternion(double x, double y, double z, double w)
    {
        A = new Complex(x, y);
        B = new Complex(z, w);
    }

    public ComplexQuaternion(Complex a, Complex b)
    {
        A = a;
        B = b;
    }

    public double Len2 => A.MagnitudeSquared() + B.MagnitudeSquared();

    public static ComplexQuaternion operator *(ComplexQuaternion a, ComplexQuaternion b)
    {
        return new ComplexQuaternion(
            a.A * b.A - a.B * b.B.Conjugate(),
            a.A * b.B + a.B * b.A.Conjugate()
        );
    }

    public static ComplexQuaternion operator +(ComplexQuaternion a, ComplexQuaternion b)
    {
        return new ComplexQuaternion(a.A + b.A, a.B + b.B);
    }
}