using System;
using System.Linq;
using Model;
using Model.Graphs;
using Model.Tools;
using Model3D.Libraries;
using Vector3 = Model3D.AsposeModel.Vector3;

namespace Model3D.Tools
{
    public class SurfaceDyno : Dyno
    {
        private readonly SurfaceFunc surfaceFunc;
        private readonly Vector2[] uvPoints;

        public SurfaceDyno(SurfaceFunc surfaceFunc, Vector2[] uvPoints, (int i, int j)[] connections, Func<Graph.Node, DynoFunc> ruleFn) 
            : base(uvPoints.Select(p => surfaceFunc(p.x, p.y)).ToArray(), connections, ruleFn)
        {
            this.surfaceFunc = surfaceFunc;
            this.uvPoints = uvPoints;

            InitRules();
        }

        protected override DynoFunc RuleFn(DynoFunc fn) => (f, ps, n) => SurfaceRule(n, fn(f, ps, n));

        Vector3 SurfaceRule(Graph.Node node, Vector3 goal)
        {
            var uv = Approximate(uvPoints[node.i], goal);
            uvPoints[node.i] = uv;

            return surfaceFunc(uv.x, uv.y);
        }

        private Vector2 Approximate(Vector2 uv0, Vector3 goal)
        {
            double Fn(double u, double v) => (goal - surfaceFunc(u, v)).Length2;

            var uv = Minimizer.Minimize(Fn, uv0, 0.001);

            return uv;
        }
    }
}
