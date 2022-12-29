using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;

namespace Model3D.Tools
{
    public static class Surfer
    {
        public static Shape FindSurface(Func<Vector3, bool> solidFn, double precision = 0.002)
        {
            var bound0 = (i:FindBoundI(solidFn, precision), j:0, k:0);

            Vector3 ToV3((int i, int j, int k) p) => new Vector3(p.i * precision, p.j * precision, p.k * precision);

            var net = new HashSet<(int, int, int)>();
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

                stack.Push((p.i + 1, p.j, p.k));
                stack.Push((p.i, p.j + 1, p.k));
                stack.Push((p.i, p.j, p.k + 1));
                stack.Push((p.i - 1, p.j, p.k));
                stack.Push((p.i, p.j - 1, p.k));
                stack.Push((p.i, p.j, p.k - 1));
            }

            // todo: построить нормальный shape

            return new Shape()
            {
                Points3 = net.Select(ToV3).ToArray()
            };
        }

        private static int FindBoundI(Func<Vector3, bool> solidFn, double precision)
        {
            var i = 0;
            
            while (solidFn(new Vector3(precision * i, 0, 0)))
                i++;

            return i;
        }

        //struct Cood
        //{
        //    public int i;
        //    public int j;
        //    public int k;
        //}
    }
}
