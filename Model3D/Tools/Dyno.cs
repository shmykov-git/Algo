using Model.Graphs;
using System;
using System.Linq;

namespace Model3D.Tools
{
    public delegate Vector3 DynoFunc(int frameCount, Vector3[] points, Graph.Node node);

    public abstract class Dyno
    {
        private DynoFunc[] rules;
        protected Vector3[] points;
        protected readonly Func<Graph.Node, DynoFunc> ruleFn;
        private Graph g;

        public Vector3[] Points => points;

        public Dyno(Vector3[] points, (int i, int j)[] connections, Func<Graph.Node, DynoFunc> ruleFn)
        {
            this.points = points;
            this.ruleFn = ruleFn;
            g = new Graph(connections);
        }

        protected void InitRules()
        {
            rules = g.nodes.Select(n => RuleFn(ruleFn(n))).ToArray();
        }

        public void Animate(int maxFrameCount)
        {
            for (var frameCount = 0; frameCount < maxFrameCount; frameCount++)
            {
                points = g.nodes.Select(node => rules[node.i](frameCount, points, node)).ToArray();
            }
        }

        protected abstract DynoFunc RuleFn(DynoFunc func);
    }
}
