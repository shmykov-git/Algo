using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Diagnostics;
using System.Linq;

namespace Model3D.Tools
{
    public static class Tuber
    {
        public static Shape MakeTube(Func3 fn, Shape tube)
        {
            var anyPoint = new Vector3(9.93, 0.001, 0.03);

            var from = tube.Points.Min(v => v.z);
            var to = tube.Points.Max(v => v.z);

            var dt = 0.001;
            var lineScale = (10).SelectRange(i => from + i * 0.1 * (to - from)).Select(t => (fn(t + dt) - fn(t)).Length).Average() / dt;
            var scale =  2 * Math.PI * lineScale;

            Vector3 Rotate(Vector3 p)
            {
                var t = p.z;

                var a = fn(t - dt);
                var b = fn(t);

                var dz = (b-a).Normalize();

                return Quaternion.FromRotation(Vector3.ZAxis, dz) * new Vector3(p.x, p.y, 0);
            }

            return new Shape
            {
                Points3 = tube.Points3.Select(p=>new Vector3(p.x, p.y, (p.z - from) / scale)).Select(p => Rotate(p) + fn(p.z)).ToArray(),
                Convexes = tube.Convexes
            };
        }
    }
}
