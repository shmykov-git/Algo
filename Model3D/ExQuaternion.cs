using MsQuaternion = System.Numerics.Quaternion;

namespace Model3D
{
    // todo: Интеграция Aspose с Mirosoft пока Aspose не сделают нужное мне умножение кватернионов
    // todo: мне нужна возможность накапливать кватернион поворота: v2 = q * (q * v0), v2 = (q*q) * v0. q2 = q*q, v2 = q2 * v0
    // todo: Для кватерниона System.Numerics это валидная операция, а для Model3D.AsposeModel нет.
    // todo: есть отличия в реализации операций умножений кватернионов или кватернионов на вектор, q2 = q*q - невалидная операция в библиотеке Aspose
    public struct ExQuaternion
    {
        public static readonly ExQuaternion Identity = new(MsQuaternion.Identity);
        
        private MsQuaternion q;

        public MsQuaternion Q => new(q.W, q.X, q.Y, q.Z);

        private ExQuaternion(MsQuaternion q)
        {
            this.q = q;
        }

        public ExQuaternion(double x, double y, double z)
        {
            q = MsQuaternion.CreateFromYawPitchRoll((float)y, (float)x, (float)z);
        }

        public ExQuaternion(Vector3 v)
        {
            q = MsQuaternion.CreateFromYawPitchRoll((float)v.y, (float)v.x, (float)v.z);
        }

        public ExQuaternion Normalize() => new(MsQuaternion.Normalize(q));

        private static System.Numerics.Vector3 ToNV3(Vector3 v) => new System.Numerics.Vector3((float)v.x, (float)v.y, (float)v.z);
        private static Vector3 ToV3(System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);

        public static ExQuaternion operator *(double x, ExQuaternion q)
        {
            return new ExQuaternion(MsQuaternion.Multiply(q.q, (float)x));
        }

        public static ExQuaternion operator *(ExQuaternion q, double x)
        {
            return new ExQuaternion(MsQuaternion.Multiply(q.q, (float)x));
        }

        public static ExQuaternion operator *(ExQuaternion a, ExQuaternion b)
        {
            return new ExQuaternion(a.q * b.q);
        }

        public static Vector3 operator *(ExQuaternion q, Vector3 v)
        {
            return ToV3(System.Numerics.Vector3.Transform(ToNV3(v), q.q));
        }

        public static implicit operator Quaternion(ExQuaternion q)
        {
            return new Model3D.Quaternion(q.q.W, q.q.X, q.q.Y, q.q.Z);
        }

        public override string ToString() => q.ToString();
    }
}