using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Model;
using Model.Extensions;
using Model.Libraries;
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

        public IEnumerable<Vector3> points
        {
            get
            {
                yield return a;
                yield return b;
                yield return c;
            }
        }

        public Vector3 ca => a - c;
        public Vector3 cb => b - c;
        public Vector3 Normal => ca.MultV(cb);
        public Vector3 NOne => Normal.Normalize();
        public Vector3 Center => (a + b + c) / 3;
        public double Size => Math.Max((b-a).Length, Math.Max((c-a).Length, (c-b).Length));

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

        // see IntersectConvexFn
        public Func<Vector3, bool> IsPointInsideFn
        {
            get
            {
                var n = NOne;

                return x => points.SelectCirclePair((a, b) => (a - x).MultS((b - a).MultV(n)).Sgn()).Sum().Abs() == 3;
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

                    if (n_xx.Abs() < Values.Epsilon9)
                        return null;

                    var xr = r0 - x0;
                    var n_xr = n.MultS(xr);

                    return x0 + (n_xr / n_xx) * xx;
                };
            }
        }

        // works perfect too, don`t remember why it is
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

                    if (nl.Abs() < Values.Epsilon9)
                        return null;

                    var t0 = -(n.MultS(x0) + -n.MultS(r0)) / nl; // d??

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

        public Func<Vector3, double[]> EdgeDistancesFn => x => points.SelectCirclePair((a, b) => Math.Sqrt((x - a).Length2 - (b - a).MultS(x - a).Pow2())).ToArray();
        public Func<Vector3, Vector3[]> EdgeNearPointsFn => x => points.SelectCirclePair((a, b) => a + (b - a).Proj(x - a)).ToArray();

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Plane Flip() => new Plane(b, a, c);
    }
}
