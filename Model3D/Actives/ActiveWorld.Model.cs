using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model.Libraries;
using Model3D.Extensions;
using static Model3D.Actives.ActiveWorld;

namespace Model3D.Actives;

public partial class ActiveWorld
{
    public const double Epsilon = Values.Epsilon6;
    public const double Epsilon2 = Epsilon * Epsilon;

    public class Model
    {
        public double forceInteractionRadius;
        public (Vector3 min, Vector3 max) borders0;
        public double volume0;
        public double volume;
        public Vector3 center;
        public Vector3 speed;
        public Vector3 angleSpeed;
        public Net3<Node> net;
        public Vector3 colliderScale;
        public int skeletonPointCount;
    }

    public class Node : INet3Item
    {
        public int i;
        public Model model;
        public Node[] nodes;
        public HashSet<int> selfInteractions;
        public Edge[] edges;
        public Plane[] planes;
        public Vector3 position0;
        public Vector3 position;
        public Vector3 nDir => planes.Select(p=>nodes[p.i].position.GetPlaneNormal(nodes[p.j].position, nodes[p.k].position)).Sum().Normalize();
        public double collideDistance;
        public Vector3 collidePosition => position + nDir * collideDistance;

        private Vector3 _speed = Vector3.Origin;
        public Vector3 speed { get => _speed; set { _speed = value; if (double.IsNaN(value.x)) Debugger.Break(); } }

        public double speedY = 0;
        public double mass = 1;
        public bool locked;

        // <plane>
        public bool isColliding;
        public int collideCount = 0;
        public Vector3 collideForce = Vector3.Origin;
        public Vector3 rejectionDirSum = Vector3.Origin;
        public Vector3 nRejectionDir = Vector3.Origin;
        public bool isInsideMaterial;
        // </plane>

        public Func<Vector3> PositionFn => () => position - model.center;// ().MultC(model.colliderScale);
    }

    public enum EdgeType
    {
        Material,
        Skeleton
    }

    public class Edge
    {
        public int i;
        public int j;
        public double fA;
        public EdgeType type = EdgeType.Material;
    }

    public class Plane
    {
        public int i;
        public int j;
        public int k;
    }

}
