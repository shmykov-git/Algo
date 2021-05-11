using Aspose.ThreeD.Utilities;
using Model;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Linq;

namespace Model3D.Tools
{
    public static class Tuber
    {
        public static Shape MakeTube(Func3 fn, Shape2 tubeShape)
        {
            var from = tubeShape.Points.Min(v => v.X);
            var to = tubeShape.Points.Max(v => v.X);
            var tube = tubeShape.ToShape3().ToTube().Scale(0.1, 0.1, 1);

            var dt = (to - from) / 10000;
            Vector3 Rotate(Vector3 p)
            {
                var t = p.x;
                var a = fn(t - dt);
                var b = fn(t);
                var c = fn(t + dt);
                var ab = b - a;
                var cb = b - c;

                var dz = ab.Normalize();
                //var dy = ((ab + cb) / 2).Normalize();
                //var dx = dz * dy;

                return Quaternion.FromRotation(Vector3.ZAxis, dz) * p;
            }

            return new Shape
            {
                Points3 = tube.Points3.Select(p => Rotate(new Vector3(p.x, p.y, 0)) + fn(p.z/(2*Math.PI))).ToArray(),
                Convexes = tube.Convexes
            };
        }
    }
}
