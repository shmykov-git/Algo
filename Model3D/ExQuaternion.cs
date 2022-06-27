using System.Numerics;
using Model3D.Systems;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;

namespace Model3D
{
    // todo: Интеграция Aspose с Mirosoft пока Aspose не сделают нужное мне умножение кватернионов
    // todo: мне нужна возможность накапливать кватернион поворота: v2 = q * (q * v0), v2 = (q*q) * v0. q2 = q*q, v2 = q2 * v0
    // todo: Для кватерниона System.Numerics это валидная операция, а для Aspose.ThreeD.Utilities нет.
    // todo: есть отличия в реализации операций умножений кватернионов или кватернионов на вектор, q2 = q*q - невалидная операция в библиотеке Aspose
    public struct ExQuaternion
    {
        public static readonly ExQuaternion Identity = new(Quaternion.Identity);
        
        private Quaternion q;

        public Aspose.ThreeD.Utilities.Quaternion Q => new(q.W, q.X, q.Y, q.Z);

        private ExQuaternion(Quaternion q)
        {
            this.q = q;
        }

        public ExQuaternion(double x, double y, double z)
        {
            q = Quaternion.CreateFromYawPitchRoll((float)y, (float)x, (float)z);
        }

        public ExQuaternion(Vector3 v)
        {
            q = Quaternion.CreateFromYawPitchRoll((float)v.y, (float)v.x, (float)v.z);
        }

        public ExQuaternion Normalize() => new(Quaternion.Normalize(q));

        private static System.Numerics.Vector3 ToNV3(Vector3 v) => new System.Numerics.Vector3((float)v.x, (float)v.y, (float)v.z);
        private static Vector3 ToV3(System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);

        public static ExQuaternion operator *(double x, ExQuaternion q)
        {
            return new ExQuaternion(Quaternion.Multiply(q.q, (float)x));
        }

        public static ExQuaternion operator *(ExQuaternion q, double x)
        {
            return new ExQuaternion(Quaternion.Multiply(q.q, (float)x));
        }

        public static ExQuaternion operator *(ExQuaternion a, ExQuaternion b)
        {
            return new ExQuaternion(a.q * b.q);
        }

        public static Vector3 operator *(ExQuaternion q, Vector3 v)
        {
            return ToV3(System.Numerics.Vector3.Transform(ToNV3(v), q.q));
        }

        public static implicit operator Aspose.ThreeD.Utilities.Quaternion(ExQuaternion q)
        {
            return new Aspose.ThreeD.Utilities.Quaternion(q.q.W, q.q.X, q.q.Y, q.q.Z);
        }

        public override string ToString() => q.ToString();
    }
}