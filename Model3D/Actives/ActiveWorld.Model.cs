using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model3D.Extensions;
using static Model3D.Actives.ActiveWorld;

namespace Model3D.Actives;

public partial class ActiveWorld
{
    public const double Epsilon = 0.000001;
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

        public Vector3 speed = Vector3.Origin;
        //public Vector3 materialSpeed = Vector3.Origin;
        public Vector3 rejectionSpeed = Vector3.Origin;
        public double speedY = 0;
        public double mass = 1;
        public bool locked;

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
