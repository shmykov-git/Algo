using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model.Libraries;
using Model3D.Extensions;

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
        public double jediMaterialThickness;
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

    public class Node : INet3Item
    {
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
        public Vector3 position;
        public Func<Vector3> PositionFn => () => position - model.center; 
        
        public Vector3 speed; //private Vector3 _speed = Vector3.Origin; public Vector3 speed { get => _speed; set { _speed = value; if (double.IsNaN(value.x)) Debugger.Break(); } }
        // <ground>
        public double speedY = 0;
        // </ground>

        // <interaction>
        private Vector3 nDir => planes.Select(p => nodes[p.i].position.GetPlaneNormal(nodes[p.j].position, nodes[p.k].position)).Sum().Normalize();

        // <self>
        public HashSet<int> selfInteractions;
        // </self>

        // <plane>
        public bool isColliding;
        public int collideCount = 0;
        public Vector3 collideForce = Vector3.Origin;
        public Vector3 rejectionDirSum = Vector3.Origin;
        public Vector3 nRejectionDir = Vector3.Origin;
        public Vector3 collidePosition => position + nDir * model.jediMaterialThickness;
        public bool isInsideMaterial;
        // </plane>
        // </interaction>
        // </dynamic>
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
        public Model model;         // dynamic values
        public Node[] nodes;        // dynamic values
        public int[] c;
        public int i;
        public int j;
        public int k;
    }

}
