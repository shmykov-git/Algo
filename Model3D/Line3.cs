using System.Collections.Generic;
using Aspose.ThreeD.Utilities;
using Model3D.Extensions;

namespace Model3D
{
    public class Line3
    {
        public Vector3 a;
        public Vector3 b;

        public Vector3 ab => b - a;
        public double Len => ab.Length;
        public double Len2 => ab.Length2;
        public Vector3 Center => (a + b) / 2;
        public Vector3 One => ab.Normalize();

        public bool IsLeft(Vector3 x) => ab.MultV(ab.MultV(x - b)).MultS(x - b) < 0;

        public static Line3 ZLineOne => new Line3(new Vector3(0, 0, 0), new Vector3(0, 0, 1));

        public IEnumerable<Vector3> Points()
        {
            yield return a;
            yield return b;
        }

        public Line3(Vector3 a, Vector3 b)
        {
            this.a = a;
            this.b = b;
        }

        public Line3((Vector3, Vector3) v)
        {
            this.a = v.Item1;
            this.b = v.Item2;
        }

        public static implicit operator Line3((Vector3 a, Vector3 b) l)
        {
            return new Line3(l.a, l.b);
        }

        public static implicit operator Line3(((double x, double y, double z) a, (double x, double y, double z) b) l)
        {
            return new Line3(new Vector3(l.a.x, l.a.y, l.a.z), new Vector3(l.b.x, l.b.y, l.b.z));
        }
    }
}
