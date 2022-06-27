using System;
using System.Numerics;

namespace View3D
{
    class Test
    {
        public static void Do()
        {
            //System.Numerics.Quaternion.CreateFromYawPitchRoll()

            var a = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 10);
            var b = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 100);

            var v = new Vector3(1, 2, 3);
            //var vv = new Vector4(1, 2, 3, 1);

            var v1 = Vector3.Transform(v, a);
            var v2 = Vector3.Transform(v, b * a);
            var v3 = Vector3.Transform(v, b * b * a);
            var v22 = Vector3.Transform(Vector3.Transform(v, a), b);
            var v33 = Vector3.Transform(v22, b);

            var ln = (v3 - v33).Length();
        }
    }
}