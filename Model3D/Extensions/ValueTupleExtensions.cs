using System;
using System.Linq;

namespace Model3D.Extensions
{
    public static class ValueTupleExtensions
    {
        public static Vector3 ToCircleYV3(this (int, int) v, double alfa0 = 0)
        {
            var (i, n) = v;
            var alfa = 2 * Math.PI * i / n + alfa0;

            return new Vector3(Math.Cos(alfa), 0, Math.Sin(alfa));
        }

        public static Func<int, int> GetMapLevelFn(this (int main, int child)[] map)
        {
            var dic = map.ToDictionary(v => v.child, v => v.main);

            int GetLevel(int child)
            {
                var level = 1;

                while (true)
                {
                    if (dic.TryGetValue(child, out int main))
                    {
                        child = main;
                        level++;
                    }
                    else
                        break;
                }

                return level;
            }

            return GetLevel;
        }
    }
}