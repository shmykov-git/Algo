using System;
using System.Drawing;
using System.Linq;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using Material = Model.Material;

namespace Model3D.Actives;

public class ActiveShape
{
    private readonly ActiveShapeOptions options;
    private ActiveWorld.Node[] nodes;
    private ActiveWorld.Plane[] planes;
    private ActiveWorld.Model model;

    private Shape shape;
    private Shape staticModel;
    private Shape staticNormModel;
    private Material? material;

    public ActiveShapeOptions Options => options;
    public ActiveWorld.Node[] Nodes => nodes;
    public ActiveWorld.Plane[] Planes => planes;
    public ActiveWorld.Model Model => model;

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

        var plns = staticModel.Convexes.Where(c => c.Length >= 3).Select(c => (c:c.ToList(), p:new ActiveWorld.Plane() { i = c[0], j = c[1], k = c[2] })).ToArray();
        planes = plns.Select(v => v.p).ToArray();
        nodes.ForEach(n => n.planes = plns.Where(v => v.c.Contains(n.i)).Select(v => v.p).ToArray());

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

        if (options.Fix != null && options.Fix.Dock != ActiveShapeOptions.FixDock.None)
        {
            Func<Vector3, bool> FixFn(Vector3 point, Vector3 direction, double distance) => x => direction.MultS(x - point).Abs() < distance;

            void Fix(Vector3 point, Vector3 direction, double distance)
            {
                var lockFn = FixFn(point, direction, distance);
                nodes.Where(n => lockFn(n.position)).ForEach(n => n.locked = true);
            }

            var b = staticModel.GetBorders();

            switch (options.Fix.Dock)
            {
                case ActiveShapeOptions.FixDock.Point:
                    Fix(options.Fix.Point, options.Fix.Direction, options.Fix.Distance);
                    break;
                case ActiveShapeOptions.FixDock.Left:
                    Fix(b.min, Vector3.XAxis, options.Fix.Distance);
                    break;
                case ActiveShapeOptions.FixDock.Right:
                    Fix(b.max, -Vector3.XAxis, options.Fix.Distance);
                    break;
                case ActiveShapeOptions.FixDock.Bottom:
                    Fix(b.min, Vector3.YAxis, options.Fix.Distance);
                    break;
                case ActiveShapeOptions.FixDock.Top:
                    Fix(b.max, -Vector3.YAxis, options.Fix.Distance);
                    break;
                case ActiveShapeOptions.FixDock.Back:
                    Fix(b.min, Vector3.ZAxis, options.Fix.Distance);
                    break;
                case ActiveShapeOptions.FixDock.Front:
                    Fix(b.max, -Vector3.ZAxis, options.Fix.Distance);
                    break;
            }
        }

        this.nodes = nodes;

        var gm = shape.Materials.GroupBy(v => v).ToArray();
        if (gm.Length == 1)
            material = gm[0].Key;

        staticNormModel = staticModel.Normalize();
        model = new();
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

        if (material != null)
            shape = shape.ApplyMaterial(material);

        shape = options.Show(shape);

        return shape;
    }
}
