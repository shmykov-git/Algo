using System;
using System.ComponentModel.DataAnnotations;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3;
using Model3D.Extensions;

namespace Model3D
{
    public class Plane
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 ca => a - c;
        public Vector3 cb => b - c;
        public Vector3 Normal => ca.MultV(cb);
        public Vector3 NOne => Normal.Normalize();
        public Func<Vector3, double> Fn
        {
            get
            {
                var n = NOne;

                return x => n.MultS(x - c);
            }
        }

        public Func<Vector3, Vector3> ProjectionFn
        {
            get
            {
                var n = NOne;

                return x => x - n * n.MultS(x - c);
            }
        }

        /// <summary>
        /// https://intuit.ru/studies/courses/70/70/lecture/2096?page=3
        /// </summary>
        public Func<Vector3, Vector3, Vector3?> IntersectionFn
        {
            get
            {
                var n = NOne;
                var r0 = c;
                var d = -n.MultS(r0);

                return (x0, x1) =>
                {
                    var l = x1 - x0;
                    var nl = n.MultS(l);

                    if (nl.Abs() < 0.000000001)
                        return null;

                    var t0 = -(n.MultS(x0) + d) / nl;

                    return x0 + t0 * l;
                };
            }
        }

        public Func<Line3, Vector3?> LineIntersectionFn
        {
            get
            {
                var fn = Fn;
                var intersectionFn = IntersectionFn;

                return l => fn(l.a).Sgn() == fn(l.b).Sgn() ? null : intersectionFn(l.a, l.b);
            }
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
