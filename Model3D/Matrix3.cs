using Model3D;

namespace Model3
{
    public struct Matrix3
    {
        public readonly double m00, m01, m02;
        public readonly double m10, m11, m12;
        public readonly double m20, m21, m22;

        public static Matrix3 UnitE = new Matrix3(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);

        public Matrix3(
            double m00, double m01, double m02,
            double m10, double m11, double m12,
            double m20, double m21, double m22)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
        }

        public Matrix3(Vector3 a, Vector3 b, Vector3 c) : this(
            a.x, a.y, a.z,
            b.x, b.y, b.z,
            c.x, c.y, c.z)
        { }

        public Vector3 Row0 => new Vector3(m00, m01, m02);
        public Vector3 Row1 => new Vector3(m10, m11, m12);
        public Vector3 Row2 => new Vector3(m20, m21, m22);

        public Vector3 Col0 => new Vector3(m00, m10, m20);
        public Vector3 Col1 => new Vector3(m01, m11, m21);
        public Vector3 Col2 => new Vector3(m02, m12, m22);

        public double[] Values => new[]
        {
            m00, m10, m20,
            m01, m11, m21,
            m02, m12, m22
        };

        public Matrix3 Transp()
        {
            return new Matrix3(
                m00, m10, m20,
                m01, m11, m21,
                m02, m12, m22);
        }

        public static Matrix3 operator -(Matrix3 a)
        {
            return new Matrix3(
                -a.m00,
                -a.m01,
                -a.m02,
                -a.m10,
                -a.m11,
                -a.m12,
                -a.m20,
                -a.m21,
                -a.m22);
        }

        public static Matrix3 operator -(Matrix3 a, Matrix3 b)
        {
            return new Matrix3(
                a.m00 - b.m00,
                a.m01 - b.m01,
                a.m02 - b.m02,
                a.m10 - b.m10,
                a.m11 - b.m11,
                a.m12 - b.m12,
                a.m20 - b.m20,
                a.m21 - b.m21,
                a.m22 - b.m22);
        }

        public static Matrix3 operator +(Matrix3 a, Matrix3 b)
        {
            return new Matrix3(
                a.m00 + b.m00,
                a.m01 + b.m01,
                a.m02 + b.m02,
                a.m10 + b.m10,
                a.m11 + b.m11,
                a.m12 + b.m12,
                a.m20 + b.m20,
                a.m21 + b.m21,
                a.m22 + b.m22);
        }


        public static Matrix3 operator *(Matrix3 a, Matrix3 b)
        {
            return new Matrix3(
                a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20,
                a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21,
                a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22,
                a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20,
                a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21,
                a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22,
                a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20,
                a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21,
                a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22);
        }

        public static Matrix3 operator *(double k, Matrix3 a)
        {
            return new Matrix3(
                k * a.m00,
                k * a.m01,
                k * a.m02,
                k * a.m10,
                k * a.m11,
                k * a.m12,
                k * a.m20,
                k * a.m21,
                k * a.m22);
        }

        public static Vector3 operator *(Matrix3 m, Vector3 v)
        {
            return new Vector3(
                m.m00 * v.x + m.m01 * v.y + m.m02 * v.z,
                m.m10 * v.x + m.m11 * v.y + m.m12 * v.z,
                m.m20 * v.x + m.m21 * v.y + m.m22 * v.z);
        }

        public static Matrix3 operator *(Matrix3 a, double k)
        {
            return k * a;
        }

        public static Matrix3 operator /(Matrix3 a, double k)
        {
            return 1.0 / k * a;
        }

        public override string ToString()
        {
            return $"({m00:0.###}   {m01:0.###}   {m02:0.###}), \r\n" +
                   $"({m10:0.###}   {m11:0.###}   {m12:0.###}), \r\n" +
                   $"({m20:0.###}   {m21:0.###}   {m22:0.###})";
        }
    }
}
