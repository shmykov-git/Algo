using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Extensions;
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
            var plane = Parquets.Triangles(5, 10, 0.1).ToShape3().Centered();

            //var plane = Surfaces.Plane(11, 11).Mult(1.0/11).Centered();

            Vector3 NothingFn(int frameCount, Vector3 position, IEnumerable<Vector3> siblings) => position;

            var r = 0.2;
            var mult = 0.5;
            Vector3 GravityFn(int frameCount, Vector3 position, IEnumerable<Vector3> siblings)
            {
                var moves = siblings.Select(v => (v - position).ToLen(l => l - r)).ToArray();
                var move = mult / frameCount * moves.Sum();

                //Debug.WriteLine(move);

                //if (move.Length > 0.25)
                //    Debugger.Break();

                return position + move;
            }

            //var rules = plane.Points.Index().Select(_ => (DynoFunc)GravityFn).ToArray();

            DynoFunc[] GetRules(Graph g) => g.nodes.Select(n =>
            {
                if (n.edges.Count < 6)
                    return (DynoFunc)NothingFn;
                else
                    return (DynoFunc)GravityFn;
            }).ToArray();

            var d = new Dyno(plane.Points2, plane.OrderedEdges, GetRules, SurfaceFuncs.HyperboloidZ);

            d.Animate(frameCount);

            return new Shape()
            {
                Points3 = d.Points,
                Convexes = plane.Convexes
            };
        }
    }
}
