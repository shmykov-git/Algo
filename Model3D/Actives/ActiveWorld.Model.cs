using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Aspose.ThreeD.Utilities;
using Model.Hashes;
using Model.Libraries;
using Model3D.Extensions;
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
        public Vector3 collidePosition;
        public bool isInsideMaterial;
        // </plane>

        public struct Pack
        {
            public Vector3 k;
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
                mass = mass,
                speed = speed,
            };

            applyCollideForce = (pack, collideMass, collideForce) =>
            {
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
        public double mass = 1;
        public bool locked;
        // </static>

        // <dynamic>
        public Vector3 position { get; set; }
        public Func<Vector3> PositionFn => () => position - model.center; 
        
        public Vector3 speed; //private Vector3 _speed = Vector3.Origin; public Vector3 speed { get => _speed; set { _speed = value; if (double.IsNaN(value.x)) Debugger.Break(); } }
        // <ground>
        public double speedY = 0;
        // </ground>

        // </dynamic>
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
                    mass = k.x * ni.mass + k.y * nj.mass,
                    speed = k.x * ni.speed + k.y * nj.speed,
                };
            };

            applyCollideForce = (pack, collideMass, collideForce) =>
            {
                ni.collideForce += (collideMass * pack.k.x / ni.mass) * collideForce;
                ni.collideCount++;
                nj.collideForce += (collideMass * pack.k.y / nj.mass) * collideForce;
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
        public Node ni => nodes[i];
        public Node nj => nodes[j];
        public Vector3 positionI => ni.position;
        public Vector3 positionJ => nj.position;
        public Vector3 positionCenter => 0.5 * (positionI + positionJ);

        public Vector3 GetPointK(Vector3 p)
        {
            var a = (positionI - p).Length;
            var b = (positionJ - p).Length;
            var s = a + b;

            return new Vector3(b / s, a / s, 0);
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
                    k  = k,
                    mass = k.x * ni.mass + k.y * nj.mass,
                    speed = k.x * ni.speed + k.y * nj.speed + k.z * nk.speed
                };
            };

            applyCollideForce = (pack, collideMass, collideForce) =>
            {
                ni.collideForce += (collideMass * pack.k.x / ni.mass) * collideForce;
                ni.collideCount++;
                nj.collideForce += (collideMass * pack.k.y / nj.mass) * collideForce;
                nj.collideCount++;
                nk.collideForce += (collideMass * pack.k.z / nk.mass) * collideForce;
                nk.collideCount++;
            };
        }

        public Model model;         // dynamic values
        public Node[] nodes;        // dynamic values
        public int[] c;
        public int i;
        public int j;
        public int k;

        public Node ni => nodes[i];
        public Node nj => nodes[j];
        public Node nk => nodes[k];
        public Vector3 positionI => ni.position;
        public Vector3 positionJ => nj.position;
        public Vector3 positionK => nk.position;

        public Vector3 GetPointK(Vector3 p)
        {
            var a = (positionI - p).Length;
            var b = (positionJ - p).Length;
            var c = (positionK - p).Length;
            var s = a * b + b * c + a * c;

            return new Vector3(b * c / s, a * c / s, a * b / s);
        }

        public Plane3 collidePlane => new Plane3(ni.collidePosition, nj.collidePosition, nk.collidePosition);
    }

}
