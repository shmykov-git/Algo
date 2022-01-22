using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model;
using Model.Extensions;
using Model.Graphs;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;

namespace Model3D.Tools
{
    public static class Dynos
    {
        public static Shape Test(int frameCount)
        {
            var plane = Shapes.Cube.SplitSphere().SplitSphere().SplitSphere(1.8).SplitSphere(2.1);
            //return plane;
            //var plane = Surfaces.Plane(11, 11).Mult(1.0/11).MassCentered();

            Vector3 NothingFn(int frameCount, Vector3[] points, Graph.Node node) => points[node.i];

            var r = 0.6;
            var mult = 1;
            Vector3 GravityFn(int frameCount, Vector3[] points, Graph.Node node)
            {
                var position = points[node.i];
                var moves = node.Siblings.Select(s => points[s.i]).Select(v => (v - position).ToLen(l => l - r)).ToArray();
                var move = mult * moves.Sum();

                //Debug.WriteLine(move);

                //if (move.Length > 0.25)
                //    Debugger.Break();

                return position + move;
            }

            var d = new SphereDyno(plane.Points3, plane.OrderedEdges, _=> GravityFn);

            d.Animate(frameCount);

            //var ps = d.Points;
            //foreach (var c in plane.Convexes)
            //    Debug.WriteLine(string.Join(", ", c.SelectCirclePair((i, j) => (ps[j] - ps[i]).Length.ToString("F2"))));

            return new Shape()
            {
                Points3 = d.Points,
                Convexes = plane.Convexes
            };
        }

        public static Shape SurfaceTest(int frameCount)
        {
            var plane = Parquets.Triangles(5, 10, 0.1).ToShape3().MassCentered();

            //var plane = Surfaces.Plane(11, 11).Mult(1.0/11).MassCentered();

            Vector3 NothingFn(int frameCount, Vector3[] points, Graph.Node node) => points[node.i];

            var r = 0.2;
            var mult = 0.1;
            Vector3 GravityFn(int frameCount, Vector3[] points, Graph.Node node)
            {
                var position = points[node.i];
                var moves = node.Siblings.Select(s => points[s.i]).Select(v => (v - position).ToLen(l => l - r)).ToArray();
                var move = mult * moves.Sum();

                //Debug.WriteLine(move);

                //if (move.Length > 0.25)
                //    Debugger.Break();

                return position + move;
            }

            //var rules = plane.Points.Index().Select(_ => (DynoFunc)GravityFn).ToArray();

            DynoFunc GetRule(Graph.Node n) 
            {
                if (n.edges.Count < 6)
                    return (DynoFunc)NothingFn;
                else
                    return (DynoFunc)GravityFn;
            }

            var d = new SurfaceDyno(SurfaceFuncs.HyperboloidZ, plane.Points2, plane.OrderedEdges, GetRule);

            d.Animate(frameCount);

            return new Shape()
            {
                Points3 = d.Points,
                Convexes = plane.Convexes
            };
        }
    }
}
