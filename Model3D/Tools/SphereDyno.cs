using Model.Graphs;
using Model3D.Extensions;
using System;

namespace Model3D.Tools
{
    public class SphereDyno : Dyno
    {
        public SphereDyno(Vector3[] points, (int i, int j)[] connections, Func<Graph.Node, DynoFunc> ruleFn) : base(points, connections, ruleFn)
        {
            InitRules();
        }

        protected override DynoFunc RuleFn(DynoFunc fn) => (f, ps, n) => SphereRule(fn(f, ps, n));

        Vector3 SphereRule(Vector3 goal)
        {
            return goal.ToLen(1);
        }
    }
}
