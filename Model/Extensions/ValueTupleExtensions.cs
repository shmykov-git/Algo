using System;

namespace Model3D.Extensions
{
    public static class ValueTupleExtensions
    {
        public static (int i, int j) OrderedEdge(this (int i, int j) e)
        {
            return (Math.Min(e.i, e.j), Math.Max(e.i, e.j));
        }

        public static (int i, int j) ReversedEdge(this (int i, int j) e)
        {
            return (e.j, e.i);
        }
    }
}
