using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

namespace Model3D.Libraries
{
    public static class MazeratorStrateges
    {
        public static GraphVisitStrategy DirectionAndGravityRandom(Vector3[] points, Func<int, Model.Vector2> positionFn, Func<int, int[]> convexFn, int seed = 0, int directionProportion = 1, int gravityProportion = 3, double power = 0.2)
        {
            var rnd = new Rnd(seed);

            Vector3 Normal(int i)
            {
                var convex = convexFn(i);

                if (convex.Length < 3)
                    return Vector3.ZAxis;

                var plane = new Plane(points[convex[0]], points[convex[1]], points[convex[2]]);

                return plane.Normal;
            }

            Model.Vector2 Direction(int i, int j) => positionFn(j) - positionFn(i);
            Model.Vector2 Gravity(int i) => Normal(i).ToV2().Normed;
            var gravityFix = Model.Vector2.One * 0.0001;

            int[] OrderVisitedNodes(int from, int to, int[] nodeIndices)
            {
                if (from == -1 || nodeIndices.Length <= 1)
                    return nodeIndices;

                var mainDir = Model.Vector2.Zero;
                if (directionProportion != 0)
                    mainDir += Direction(from, to).ToLen(directionProportion);
                if (gravityProportion != 0)
                    mainDir += (Gravity(to) + gravityFix).ToLen(gravityProportion);
                mainDir = mainDir.Normed;

                var dirs = nodeIndices.Select(i => Direction(to, i).Normed).ToArray();

                var projections = dirs.Select(dir => (mainDir * dir + 1).Pow(power)).ToArray();
                var sum = projections.Sum();

                var probabilities = projections.Select(p => p / sum).ToArray();

                //var randomOrderCount = nodeIndices.Length == 2 ? 1 : 2;

                //Debug.WriteLine($"{points[to].ToV2()}: {string.Join(", ", probabilities.Select(position => $"{position:F1}"))}");

                return rnd.RandomIndices(probabilities, probabilities.Length - 1).Select(i => nodeIndices[i]).ToArray();
            }

            return OrderVisitedNodes;
        }
    }
}
