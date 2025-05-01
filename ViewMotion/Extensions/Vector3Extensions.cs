using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Model3D.AsposeModel;

namespace ViewMotion.Extensions
{
    static class Vector3Extensions
    {
        public static Point3D ToP3D(this Vector3 v) => new(v.x, v.y, v.z);
        public static Vector3D ToV3D(this Vector3 v) => new(v.x, v.y, v.z);
    }
}
