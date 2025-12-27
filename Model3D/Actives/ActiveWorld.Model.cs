using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plane3 = Model3D.Plane;

namespace Model3D.Actives;

public partial class ActiveWorld
{
    public const double Epsilon = Values.Epsilon6;
    public const double Epsilon2 = Epsilon * Epsilon;

    public class Model
    {
        // <static>
        public (Vector3 min, Vector3 max) borders0;
        public Net3<Node> net;      // dynamic values

        //<material>
        public double volume0;
        public int skeletonPointCount;
        public int skeletonPointStart;
        //</material>

        // <interaction>
        public double forceInteractionRadius;
        // </interaction>
        // </static>

        // <dynamic>
        public Vector3 center;
        public Vector3 speed;

        // <material>
        public double volume;
        public Vector3 angleSpeed;
        // </material>
        // </dynamic>
    }

    public class Interactor
    {
        public Func<Vector3, Pack> pointPackFn;
        public Action<Pack, double, Vector3> applyCollideForce;

        // <self>
        public HashSet<int> selfInteractions;
        // </self>

        // <plane>
        public bool isColliding;
        public int collideCount = 0;
        public Vector3 collideForce = Vector3.Origin;
        public Vector3 rejectionDirSum = Vector3.Origin;
        public Vector3 nRejectionDir = Vector3.Origin;
        public bool isInsideMaterial;
        public Pack pack;
        // </plane>

        public struct Pack
        {
            public double[] k;
            public double mass;
            public Vector3 speed;
        }
    }

    public class Node : Interactor, INet3Item
    {
        public Node()
        {
            pointPackFn = _ => new Pack
            {
                k = new[] { 1.0 },
                mass = mass,
                speed = speed,
            };

            applyCollideForce = (pack, collideMass, collideForce) =>
            {
                base.pack = pack;
                base.collideForce += (collideMass / pack.mass) * collideForce;
                collideCount++;
            };
        }

        // <static>
        public int i;
        public Model model;         // dynamic values
        public Node[] nodes;        // dynamic values
        public Edge[] edges;
        public Plane[] planes;
        public Vector3 position0;
        public double mass = 1; //private double _mass = 1; public double mass { get => _mass; set { _mass = value; if (_mass == 0) Debugger.Break(); } }
        public bool locked;
        // </static>

        // <dynamic>
        public Vector3 position { get; set; }
        // <plane>
        public bool hasnDir;
        public Vector3 nDir;
        public Vector3 collidePosition;
        // </plane>
        public Func<Vector3> PositionFn => () => position - model.center;
        public Vector3 speed;
        //private Vector3 _speed = Vector3.Origin; public Vector3 acc { get => _speed; set { _speed = debugSpeed(value); } }
        // <ground>
        public double speedY = 0;
        // </ground>

        // </dynamic>

        #region debug
        private static double __speedMax;
        Vector3 debugSpeed(Vector3 value)
        {
            if (double.IsNaN(value.x) || double.IsInfinity(value.x))
                Debugger.Break();

            if (value.Length > 0.05)
                Debugger.Break();

            var speed = Math.Round(value.Length * 100) * 0.01;
            if (speed > __speedMax)
            {
                __speedMax = speed;
                Debug.WriteLine($"Speed: {speed}");
            }

            return value;
        }
        #endregion
    }

    public enum EdgeType
    {
        Material,
        Skeleton
    }

    public class Edge : Interactor
    {
        public Edge()
        {
            pointPackFn = p =>
            {
                var k = GetPointK(p);

                return new Pack
                {
                    k = k,
                    mass = k[0] * ni.mass + k[1] * nj.mass,
                    speed = k[0] * ni.speed + k[1] * nj.speed,
                };
            };

            applyCollideForce = (pack, collideMass, collideForce) =>
            {
                ni.collideForce += (collideMass * pack.k[0] / ni.mass) * collideForce;
                ni.collideCount++;
                nj.collideForce += (collideMass * pack.k[1] / nj.mass) * collideForce;
                nj.collideCount++;
            };
        }

        public Model model;         // dynamic values
        public Node[] nodes;        // dynamic values
        public (int i, int j) key;
        public int i;
        public int j;
        public double fA;
        public EdgeType type = EdgeType.Material;

        private Node _ni;
        private Node _nj;
        public Node ni => _ni ??= nodes[i];
        public Node nj => _nj ??= nodes[j];
        public Vector3 positionI => ni.position;
        public Vector3 positionJ => nj.position;
        public Vector3 positionCenter => 0.5 * (positionI + positionJ);

        public double[] GetPointK(Vector3 p)
        {
            var a = (positionI - p).Length;
            var b = (positionJ - p).Length;
            var s = a + b;

            return new[] { b / s, a / s };
        }
    }

    public class Plane : Interactor
    {
        public Plane()
        {
            pointPackFn = p =>
            {
                var k = GetPointK(p);

                return new Pack
                {
                    k = k,
                    mass = c.Select((ci, i) => k[i] * nodes[ci].mass).Sum(),
                    speed = c.Select((ci, i) => k[i] * nodes[ci].speed).Sum()
                };
            };

            applyCollideForce = (pack, collideMass, collideForce) =>
            {
                c.ForEach((ci, i) =>
                {
                    var n = nodes[ci];
                    n.collideForce += (collideMass * pack.k[i] / n.mass) * collideForce;
                    n.collideCount++;
                });
            };
        }

        public Model model;         // dynamic values
        public Node[] nodes;        // dynamic values
        public int[] c;
        public int i;
        public int j;
        public int k;

        private Node _ni;
        private Node _nj;
        private Node _nk;
        public Node ni => _ni ??= nodes[i];
        public Node nj => _nj ??= nodes[j];
        public Node nk => _nk ??= nodes[k];
        public Vector3 positionI => ni.position;
        public Vector3 positionJ => nj.position;
        public Vector3 positionK => nk.position;

        public double[] GetPointK(Vector3 p)
        {
            var lens = c.Select(i => (nodes[i].position - p).Length).ToArray();
            var mLens = lens.Select((l, i) => lens.Where((_, j) => i != j).Aggregate((a, b) => a * b)).ToArray();
            var s = mLens.Sum();

            return mLens.Select(mL => mL / s).ToArray();
        }

        public Plane3 collidePlane => new Plane3(ni.collidePosition, nj.collidePosition, nk.collidePosition);

        public Func<Vector3, double, bool> IsInsideFn(Vector3 n) => (x, d) => c.Select(i => nodes[i].collidePosition + nodes[i].nDir * d).SelectCirclePair((a, b) => (a - x).MultS((b - a).MultV(n)).Sgn()).Sum().Abs() == c.Length;
        public Func<Vector3, Vector3[]> EdgeNearPointsFn => x => c.Select(i => nodes[i].collidePosition).SelectCirclePair((a, b) => a + (b - a) * ((b - a).MultS(x - a) / (b - a).Length2)).ToArray();
    }

}
