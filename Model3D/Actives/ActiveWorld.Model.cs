using System;
using Aspose.ThreeD.Utilities;

namespace Model3D.Actives;

public partial class ActiveWorld
{
    public const double Epsilon = 0.000000001;
    public const double Epsilon2 = Epsilon * Epsilon;

    public class Node : INet3Item
    {
        public int i;
        public Node[] nodes;
        public Edge[] edges;
        public Plane[] planes;
        public Vector3 position0;
        public Vector3 position;
        public Vector3 speed = Vector3.Origin;
        public double speedY = 0;
        public double mass = 1;
        public bool locked;
        public Func<Vector3> PositionFn => () => position;
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
