using Model;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model3D.Tools
{
    public static class Surfer
    {
        public static Shape FindSurface(Func<Vector3, bool> solidFn, double precision = 0.002)
        {
            var bound0 = (i: FindBoundI(solidFn, precision), j: 0, k: 0);
            var siblings = new[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };
            (int i, int j, int k) Add((int i, int j, int k) a, (int i, int j, int k) b) => (a.i + b.i, a.j + b.j, a.k + b.k);
            (int i, int j, int k) Sub((int i, int j, int k) a, (int i, int j, int k) b) => (a.i - b.i, a.j - b.j, a.k - b.k);
            (int i, int j, int k) Mult((int i, int j, int k) a, (int i, int j, int k) b) => (a.i * b.i, a.j * b.j, a.k * b.k);
            bool IsOrt((int i, int j, int k) a, (int i, int j, int k) b) => Mult(a, b) == default;
            //int Distance((int gi, int j, int k) a, (int gi, int j, int k) b) => (a.gi - b.gi).Abs() + (a.j - b.j).Abs() + (b.k - b.k).Abs();

            IEnumerable<(int i, int j, int k)> Siblings((int i, int j, int k) a) => siblings.Select(b => Add(a, b));

            Vector3 ToV3((int i, int j, int k) p) => new Vector3(p.i * precision, p.j * precision, p.k * precision);
            Vector3 ToCenterV3((int i, int j, int k) p) => ToV3(p) - 0.5 * new Vector3(precision, precision, precision);

            var net = new HashSet<(int i, int j, int k)>();
            IEnumerable<(int i, int j, int k)> NetSiblings((int i, int j, int k) a) => Siblings(a).Where(s => net.Contains(s));

            IEnumerable<(int i, int j, int k)> OrthogonalNetSiblings((int i, int j, int k) a, (int i, int j, int k) b)
            {
                var ab = Sub(b, a);
                return NetSiblings(b).Where(s => IsOrt(ab, Sub(s, b)));
            }

            // 3я точка ищется в 2х плоскостях точке a, b среди 8 точек
            //(int gi, int j, int k) FindThird((int gi, int j, int k) a, (int gi, int j, int k) b) => NetSiblings(a).First(s => s!=b && Distance(s, b))
            var cash = new Dictionary<(int, int, int), int>();

            int SolidFn((int i, int j, int k) p)
            {
                if (cash.TryGetValue(p, out int value))
                {
                    if (value > 0)
                        value++;
                    else
                        value--;

                    cash[p] = value;

                    if (value.Abs() == 8)
                        cash.Remove(p);

                    return value > 0 ? 1 : 0;
                }

                var isSolid = solidFn(ToV3(p));
                value = isSolid ? 1 : -1;
                cash.Add(p, value);

                return value > 0 ? 1 : 0;
            }

            bool IsBound((int i, int j, int k) p)
            {
                var v =
                    SolidFn((p.i, p.j, p.k)) +
                    SolidFn((p.i - 1, p.j, p.k)) +
                    SolidFn((p.i, p.j - 1, p.k)) +
                    SolidFn((p.i, p.j, p.k - 1)) +
                    SolidFn((p.i - 1, p.j - 1, p.k)) +
                    SolidFn((p.i, p.j - 1, p.k - 1)) +
                    SolidFn((p.i - 1, p.j, p.k - 1)) +
                    SolidFn((p.i - 1, p.j - 1, p.k - 1));

                return v > 0 && v < 8;
            }



            var stack = new Stack<(int i, int j, int k)>();
            stack.Push(bound0);

            while (stack.Count > 0)
            {
                var p = stack.Pop();

                if (net.Contains(p) || !IsBound(p))
                    continue;

                net.Add(p);
                Siblings(p).ForEach(stack.Push);
            }

            var ps = new Vector3[net.Count];
            var psInd = net.Select((p, i) => (p, i)).ToDictionary(v => v.p, v => v.i);
            psInd.ForEach(v => ps[v.Value] = ToCenterV3(v.Key));

            // найти первые 2 вершины, далее присоединять одну и отказываться от одной из 2х, повторить
            var a = bound0;
            //var b = NetSiblings(a).First();

            //int[][] GetConvexes()
            //{

            //}

            // направление?
            // todo: обход сетки
            //var pairs = new ((int gi, int j, int k) a, (int gi, int j, int k) b)[]
            //{
            //    ((1, 0, 0), (0, 1, 0)),
            //    ((1, 0, 0), (0, 0, 1)),
            //    ((1, 0, 0), (0, -1, 0)),
            //    ((1, 0, 0), (0, 0, -1)),

            //    ((-1, 0, 0), (0, 1, 0)),
            //    ((-1, 0, 0), (0, 0, 1)),
            //    ((-1, 0, 0), (0, -1, 0)),
            //    ((-1, 0, 0), (0, 0, -1)),

            //    ((0, 1, 0), (0, 0, 1)),
            //    ((0, 0, 1), (0, -1, 0)),
            //    ((0, -1, 0), (0, 0, -1)),
            //    ((0, 0, -1), (0, 1, 0)),
            //};

            //var zr = new Vector3(0, 0, 0);

            //var convexes = pdDic.SelectMany(voxel =>
            //    pairs
            //        .Select(pr => (a: Add(voxel.Key, pr.a), b: Add(voxel.Key, pr.b)))
            //        .Where(pr => net.Contains(pr.a) && net.Contains(pr.b))
            //        .Select(pr => new[] {voxel.Value, pdDic[pr.a], pdDic[pr.b]}))
            //    //.Select(c=>Angle.IsLeftDirection(zr, ps[c[0]], ps[c[1]]))
            //    .ToArray();

            // todo: убрать лишние
            var convexes = net.SelectMany(a => NetSiblings(a).SelectMany(b => OrthogonalNetSiblings(a, b).Select(c => (a, b, c))))
                .Select(v => new[] { psInd[v.a], psInd[v.b], psInd[v.c] })
                .ToArray();
            //.Select(pr => (pdDic[pr.a], pdDic[pr.b]).OrderedEdge()))
            //.Distinct();

            return new Shape()
            {
                Points3 = ps,
                Convexes = convexes
            };
        }

        private static int FindBoundI(Func<Vector3, bool> solidFn, double precision)
        {
            var i = 0;

            while (solidFn(new Vector3(precision * i, 0, 0)))
                i++;

            return i;
        }
    }
}
