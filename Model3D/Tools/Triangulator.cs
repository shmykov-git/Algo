using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Extensions;

namespace Model3D.Tools
{
    public static class Triangulator
    {
        class Convex
        {
            public Line2 Line;
            public bool HasLine;
            public List<Vector2> Points;
        }

        //public Polygon[] SpliteByConvexes(Polygon polygon)
        //{
        //    var res = new List<Polygon>();
        //    var stack = new Stack<Convex>();
        //    stack.Push(new Convex() {HasLine = false, Points = new List<Vector2>()});

        //    bool IsLeft((Vector2 a, Vector2 b, Vector2 c) voxel) => new Line2(voxel.a, voxel.b).IsLeft(voxel.c);

        //    foreach (var trio in polygon.Points.SelectCircleTriple((a, b, c) => (a, b, c)))
        //    {
        //        var convex = stack.Peek();
        //        convex.Points.Add(trio.b);

        //        if (convex.HasLine)
        //        {
        //            if (convex.Line.IsLeft(trio.b))
        //            {
        //                stack.Pop();
        //                // результат
        //            }
        //        }

                


        //        if (IsLeft(trio))
        //        {


        //        }
        //        else
        //        {
        //            res.Add((new Line2(trio.a, trio.b), new List<Vector2>()));
        //        }
        //    }

        //    polygon.Points.SelectWithIndex((position, gi) => (position, gi)).SelectCircleTriple((a, b, c) => (a, b, c))
        //        /*.Where(voxel => !new Line2(voxel.a.position, voxel.b.position).IsLeft(voxel.c.position)).Select(voxel => voxel.b)*/;
        //}

    }
}