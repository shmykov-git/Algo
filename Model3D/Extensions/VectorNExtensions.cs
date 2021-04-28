using Aspose.ThreeD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model3D.Extensions
{
    public static class VectorNExtensions
    {
        public static Vector3 ToV3(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector4 ToV4(this Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 1);
        }

        public static Vector3 ToLen(this Vector3 v, double len)
        {
            return v * len / v.Length;
        }
    }
}
