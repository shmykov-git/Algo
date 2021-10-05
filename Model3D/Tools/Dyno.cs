using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model;
using Model.Extensions;
using Model.Tools;
using Model3D.Extensions;
using Model3D.Libraries;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;

namespace Model3D.Tools
{

    public delegate Vector3 DynoFunc(int frameCount, Vector3 position, IEnumerable<Vector3> siblings);

    public class Dyno
    {
        private readonly Vector2[] uvPoints;
        private readonly DynoFunc[] rules;
        private readonly Vector3[] points;
        private readonly (int i, int j)[] connections;
        private readonly SurfaceFunc surfaceFunc;
        private bool hasSurface;
        private Graph g;

        public Vector3[] Points => points;

        public Dyno(Vector2[] planePoints, (int i, int j)[] connections, Func<Graph, DynoFunc[]> rulesFn, SurfaceFunc surfaceFunc = null)
            : this(planePoints, connections, (DynoFunc[])null, surfaceFunc)
        {
            this.rules = rulesFn(g);
        }

        public Dyno(Vector2[] planePoints, (int i, int j)[] connections, DynoFunc[] rules, SurfaceFunc surfaceFunc = null)
        {
            hasSurface = surfaceFunc != null;
            this.surfaceFunc = surfaceFunc ?? ((double x, double y) => new Vector3(x, y, 0));

            this.uvPoints = planePoints;
            this.rules = rules;
            this.points = planePoints.Select(p => this.surfaceFunc(p.x, p.y)).ToArray();
            this.connections = connections;

            g = new Graph(connections);
        }

        public void Animate(int maxFrameCount)
        {
            for (var frameCount = 0; frameCount < maxFrameCount; frameCount++)
            {
                var values = new List<(Vector2 uv, Vector3 p)>();

                foreach(var node in g.nodes)
                {
                    var goal = rules[node.i](frameCount, points[node.i], node.Siblings.Select(sNode => points[sNode.i]));
                    if (hasSurface)
                    {
                        var uv = Approximate(uvPoints[node.i], goal);
                        values.Add((uv, surfaceFunc(uv.x, uv.y)));
                        //uvPoints[node.i] = uv;
                        //points[node.i] = surfaceFunc(uv.x, uv.y);
                    }
                    else
                    {
                        values.Add((goal.ToV2(), goal));
                        //points[node.i] = goal;
                        //uvPoints[node.i] = goal.ToV2();
                    }
                }

                foreach (var node in g.nodes)
                {
                    uvPoints[node.i] = values[node.i].uv;
                    points[node.i] = values[node.i].p;
                }
            }
        }

        private Vector2 Approximate(Vector2 uv0, Vector3 goal)
        {
            double Fn(double u, double v) => (goal - surfaceFunc(u, v)).Length2;

            var uv = Minimizer.Minimize(Fn, uv0, 0.001);

            return uv;
        }
    }
}
