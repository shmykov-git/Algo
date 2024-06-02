using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Extensions;

public static class ArrayExtensions
{
    public static void Shaffle<TItem>(this TItem[] values, Random rnd) => values.Shaffle(3 * values.Length + 7, rnd);

    public static void Shaffle<TItem>(this TItem[] values, int shaffleCount, Random rnd)
    {
        foreach (var _ in Enumerable.Range(0, rnd.Next(shaffleCount)))
        {
            var i = rnd.Next(values.Length);
            var j = rnd.Next(values.Length);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}
