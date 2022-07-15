using System;
using System.ComponentModel.DataAnnotations;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3;
using Model3D.Extensions;
using Model3D.Libraries;

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

        public SurfaceFunc PointsFn
        {
            get
            {
                var uOne = ca.Normalize();
                var vOne = NOne.MultV(uOne);

                return (u, v) => c + u * uOne + v * vOne;
            }
        }

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
        /// Пересечение прямой и плоскости
        /// https://intuit.ru/studies/courses/70/70/lecture/2096?page=3
        /// x = x0 + (n * (r0 - x0) / n * (x1 - x0)) * (x1 - x0)
        /// x0, x1 - точки построения прямой
        /// r0, n - точка плоскости и единичная нормаль плоскости
        /// x - точка пересечения прямой и плоскости
        /// </summary>
        public Func<Vector3, Vector3, Vector3?> IntersectionFn
        {
            get
            {
                var n = NOne;
                var r0 = c;

                return (x0, x1) =>
                {
                    var xx = x1 - x0;
                    var n_xx = n.MultS(xx);

                    if (n_xx.Abs() < 0.000000001)
                        return null;

                    var xr = r0 - x0;
                    var n_xr = n.MultS(xr);

                    return x0 + (n_xr / n_xx) * xx;
                };
            }
        }

        public Func<Vector3, Vector3, Vector3?> IntersectionFn1
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

                    var t0 = -(n.MultS(x0) + -n.MultS(r0)) / nl;

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
