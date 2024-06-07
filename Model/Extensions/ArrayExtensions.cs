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

    public static int[] ReverseReverses(this int[] reverses)
    {
        return (reverses.Length).Range(j => (i: reverses[j], j)).OrderBy(v => v.i).Select(v => v.j).ToArray();
    }

    /// <summary>
    /// From what ordered array these values can be get by forward reverses
    /// </summary>
    public static int[] ToBackReverses(this IEnumerable<int> values)
    {
        return values.Select((v, i) => (i, v)).OrderBy(v => v.v).Select(v => v.i).ToArray();
    }

    /// <summary>
    /// How to reverse values to get ordered array
    /// </summary>
    public static int[] ToReverses(this IEnumerable<int> values)
    {
        return values.ToBackReverses().ReverseReverses();
    }

    public static int[] JoinReverses(this int[] reversesA, int[] reversesB)
    {
        return reversesB.Select(i => reversesA[i]).ToArray();
    }

    public static int[] ToTranslateReverses(this IEnumerable<int> valuesA, IEnumerable<int> valuesB)
    {
        return valuesA.ToBackReverses().JoinReverses(valuesB.ToReverses());
    }

    public static int[] ToBackTranslateReverses(this IEnumerable<int> valuesA, IEnumerable<int> valuesB)
    {
        return valuesB.ToBackReverses().JoinReverses(valuesA.ToBackReverses().ReverseReverses());
    }

    /// <summary>
    /// What order values should be placed
    /// </summary>
    public static TItem[] ReverseForward<TItem>(this TItem[] values, int[] reverses)
    {
        return reverses.Select(i => values[i]).ToArray();
    }

    /// <summary>
    /// From what order values were placed to this
    /// </summary>
    public static TItem[] ReverseBack<TItem>(this TItem[] values, int[] backReverses)
    {
        return values.ReverseForward(ReverseReverses(backReverses));
    }
}
