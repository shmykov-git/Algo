using System.Linq;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;

namespace Model3D.Actives;

public class ActiveShape
{
    private readonly ActiveShapeOptions options;
    private ActiveWorld.Node[] nodes;
    private Shape shape;
    private Shape staticModel;
    private Shape staticNormModel;

    public ActiveShapeOptions Options => options;
    public ActiveWorld.Node[] Nodes => nodes;

    public ActiveShape(Shape shape, ActiveShapeOptions options)
    {
        this.options = options;
        this.shape = shape;
    }

    public void Activate()
    {
        staticModel = shape;
        var rotationCenter = options.RotationCenter ?? staticModel.PointCenter;

        if (options.UseSkeleton)
            staticModel = staticModel.WithCenterPoint();

        if (options.RotationAngle > 0)
            staticModel = staticModel.Rotate(options.RotationAngle, options.RotationAxis);

        var ps = staticModel.Points3;

        var nodes = ps.Select((p, i) => new ActiveWorld.Node()
        {
            i = i,
            position0 = p,
            position = p
        }).ToArray();

        nodes.ForEach(n => n.nodes = nodes);

        var nLast = nodes.Length - 1;
        nodes.ForEach(n => n.edges = staticModel.Links[n.i].Select(j => new ActiveWorld.Edge
        {
            i = n.i,
            j = j,
            fA = (n.position - nodes[j].position).Length,
            type = options.UseSkeleton && nLast == j ? ActiveWorld.EdgeType.Skeleton : ActiveWorld.EdgeType.Material
        }).ToArray());

        nodes.ForEach(n => n.planes = staticModel.Convexes.Where(c => c.Length >= 3).Where(c => c.Any(j => n.i == j)).Select(c => new ActiveWorld.Plane() { i = c[0], j = c[1], k = c[2] }).ToArray());

        if (options.RotationSpeedAngle > 0)
        {
            var speedRotationCenter = options.RotationSpeedCenter ?? nodes.Select(n => n.position).Center();

            nodes.ForEach(n =>
            {
                var position = n.position - speedRotationCenter;
                var ort = position.MultV(options.RotationSpeedAxis);

                if (ort.Length2 < ActiveWorld.Epsilon2)
                {
                    n.speed = Vector3.Origin;
                }
                else
                {
                    var b = ort.Normalize();
                    var c = b.MultV(options.RotationSpeedAxis).Normalize();
                    var p = new Plane(Vector3.Origin, b, c);
                    var pFn = p.ProjectionFn;

                    n.speed = options.RotationSpeedAngle * pFn(position).MultV(options.RotationSpeedAxis);
                }
            });
        }

        staticNormModel = staticModel.Normalize();
        this.nodes = nodes;
    }

    public void Step()
    {
        options.Step(this);
    }

    public Shape GetStepShape()
    {
        var shape = new Shape
        {
            Points3 = options.ShowSkeletonPoint || !options.UseSkeleton
                ? nodes.Select(n => n.position).ToArray()
                : nodes.SkipLast(1).Select(n => n.position).ToArray(),
            Convexes = staticNormModel.Convexes
        };

        if (options.ShowMeta)
        {
            shape = shape.ToMetaShape3(options.MetaPointMult, options.MetaLineMult, options.Color2, options.Color1);
        }
        else
        {
            if (options.Color1.HasValue)
                shape = shape.ApplyColor(options.Color1.Value);
        }

        return shape;
    }
}
