using System;
using System.Collections.Generic;

namespace Model.Extensions
{
    public static class EnumerableV2
    {
        public static IEnumerable<Vector2> Wedge(int count, bool centered = false)
        {
            var h = Math.Sqrt(3) / 2;

            var pos = Vector2.Zero;
            var offset = Vector2.Zero;
            var k = 0;
            var i = 0;

            var list = new List<Vector2>();

            while (true)
            {
                for (var j = 0; j <= i; j++)
                {
                    list.Add(pos + new Vector2(j, 0) + offset);

                    if (k++ == count)
                        break;
                }

                if (k == count)
                    break;

                i++;

                offset = new Vector2(-0.5 * i, -i * h);
            }

            if (centered)
            {
                var center = list.Center();

                foreach (var v in list)
                    yield return v - center;
            }
            else
            {
                foreach (var v in list)
                    yield return v;
            }
        }
    }
}