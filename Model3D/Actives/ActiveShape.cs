using Model.Extensions;
using Model.Graphs;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using Material = Model.Material;
using Shape = Model.Shape;

namespace Model3D.Actives;

public class ActiveShape : INet3Item
{
    private readonly ActiveShapeOptions options;
    private ActiveWorld.Node[] nodes;
    private ActiveWorld.Plane[] planes;
    private ActiveWorld.Model model;

    private Shape shape0;
    private Shape staticModel;
    private Shape staticNormModel;
    private Material? material;
    //private 

    public ActiveShapeOptions Options => options;
    public ActiveWorld.Node[] Nodes => nodes;
    public IEnumerable<ActiveWorld.Node> NoSkeletonNodes => nodes.Where(n => options.UseSkeleton ? n.i < shape0.PointsCount : true);
    public ActiveWorld.Plane[] Planes => planes;
    public ActiveWorld.Model Model => model;
    public Shape Shape => shape0;

    Func<Vector3> INet3Item.PositionFn => () => model.center;

    public ActiveShape(Shape shape, ActiveShapeOptions options)
    {
        this.options = options;
        this.shape0 = options.AllowTriangulation0 ? shape.TriangulateByFour() : shape;
        if (options.Type == ActiveShapeOptions.ShapeType.Unknown)
            options.Type = shape0.IsD3 ? ActiveShapeOptions.ShapeType.D3 : ActiveShapeOptions.ShapeType.D2;
    }
    bool IsSkeletonPoint(int i) => model.skeletonPointStart <= i && i < model.skeletonPointStart + model.skeletonPointCount;
    bool IsSkeletonEdge(int i, int j) => IsSkeletonPoint(i) || IsSkeletonPoint(j);

    public void Activate()
    {
        staticModel = shape0;
        model = new ActiveWorld.Model
        {
        };

        Model.center = staticModel.PointCenter;
        Model.borders0 = staticModel.GetBorders();

        var rotationCenter = options.RotationCenter ?? Model.center;

        if (options.UseSkeleton)
        {
            model.skeletonPointStart = shape0.PointsCount;

            switch (options.Skeleton.Type)
            {
                case ActiveShapeOptions.SkeletonType.CenterPoint:
                    staticModel = staticModel.WithCenterPoint();
                    model.skeletonPointCount = 1;
                    break;

                case ActiveShapeOptions.SkeletonType.ShapeSizeRatioRadius:
                    var minXyz = staticModel.Centered().RotateToTopY().Size.MinXyz(); // todo: shape box ratio with size
                    (staticModel, var skeletonPointCountXyz) = new SupperShape(staticModel).GetSkeleton(minXyz * options.Skeleton.Radius);
                    model.skeletonPointCount = skeletonPointCountXyz;
                    break;

                case ActiveShapeOptions.SkeletonType.Radius:
                    var maxXyz = staticModel.Centered().RotateToTopY().Size.MaxXyz();
                    (staticModel, var skeletonPointCount) = new SupperShape(staticModel).GetSkeleton(maxXyz * options.Skeleton.Radius);
                    model.skeletonPointCount = skeletonPointCount;
                    break;
            }

            if (options.Docked != null)
                staticModel = staticModel.DockSingle(options.Docked);
        }

        if (options.RotationAngle.Abs() > 0)
            staticModel = staticModel.Rotate(options.RotationAngle, options.RotationAxis);

        var ps = staticModel.Points3;

        var nodes = ps.Select((p, i) => new ActiveWorld.Node()
        {
            i = i,
            model = model,
            position0 = p,
            position = p,
            mass = options.Mass,
            speed = Vector3.Origin,
        }).ToArray();

        nodes.ForEach(n => n.nodes = nodes);

        //var skeletonI = shape0.PointsCount;

        // todo: list of unique edges

        nodes.ForEach(n => n.edges = staticModel.Links.TryGetValue(n.i, out var js) ?
            js.Select(j => new ActiveWorld.Edge
            {
                model = model,
                nodes = nodes,
                i = n.i,
                j = j,
                fA = (n.position - nodes[j].position).Length,
                type = options.UseSkeleton && IsSkeletonEdge(n.i, j) ? ActiveWorld.EdgeType.Skeleton : ActiveWorld.EdgeType.Material
            }).ToArray()
        : new ActiveWorld.Edge[0]);

        planes = shape0.Convexes.Where(c => c.Length > 2)
            .Select(c => (c, p: new ActiveWorld.Plane()
            {
                model = model,
                nodes = nodes,
                c = c,
                i = c[0],
                j = c[1],
                k = c[2]
            }))
            .Select(v => v.p)
            .ToArray();

        nodes.ForEach(n => n.planes = planes.Where(p => p.c.Contains(n.i)).ToArray());
        nodes.ForEach(n => n.hasnDir = n.planes.Length > 0);

        if (options.Speed.Length > 0)
            nodes.ForEach(n => n.speed += options.Speed / options.WorldOptions.OverCalculationMult);

        if (options.RotationSpeedAngle.Abs() > 0)
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

                    n.speed += (options.RotationSpeedAngle / options.WorldOptions.OverCalculationMult) * pFn(position).MultV(options.RotationSpeedAxis);
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

            var b = Model.borders0;

            if (options.Fix.Dock.HasFlag(ActiveShapeOptions.FixDock.Point))
                Fix(options.Fix.Point, options.Fix.Direction, options.Fix.Distance);

            if (options.Fix.Dock.HasFlag(ActiveShapeOptions.FixDock.Left))
                Fix(b.min, Vector3.XAxis, options.Fix.Distance);

            if (options.Fix.Dock.HasFlag(ActiveShapeOptions.FixDock.Right))
                Fix(b.max, -Vector3.XAxis, options.Fix.Distance);

            if (options.Fix.Dock.HasFlag(ActiveShapeOptions.FixDock.Bottom))
                Fix(b.min, Vector3.YAxis, options.Fix.Distance);

            if (options.Fix.Dock.HasFlag(ActiveShapeOptions.FixDock.Top))
                Fix(b.max, -Vector3.YAxis, options.Fix.Distance);

            if (options.Fix.Dock.HasFlag(ActiveShapeOptions.FixDock.Back))
                Fix(b.min, Vector3.ZAxis, options.Fix.Distance);

            if (options.Fix.Dock.HasFlag(ActiveShapeOptions.FixDock.Front))
                Fix(b.max, -Vector3.ZAxis, options.Fix.Distance);
        }

        this.nodes = nodes;

        if (shape0.Materials != null)
        {
            var gm = shape0.Materials.GroupBy(v => v).ToArray();
            if (gm.Length == 1)
                material = gm[0].Key;
        }

        if (options.UseInteractions)
        {
            model.net = new Net3<ActiveWorld.Node>(nodes, options.WorldOptions.ForceInteractionRadius, true);

            if (options.UseSelfInteractions)
            {
                var g = new Graph(nodes.SelectMany(n => n.edges.Select(e => (e.i, e.j))).Distinct());
                var distance = options.WorldOptions.Interaction.SelfInteractionGraphDistance;
                nodes.ForEachInParallel(a =>
                {
                    var map = g.DistanceMap(a.i);
                    a.selfInteractions = nodes.Where(b => map[b.i] <= distance).Select(n => n.i).ToHashSet();
                });
            }
        }

        staticNormModel = staticModel.Normalize();
    }

    public void Step()
    {
        options.Step(this);
    }

    public Shape GetStepShape()
    {
        var shape = new Shape
        {
            Points3 = options.Skeleton.ShowPoints || !options.UseSkeleton
                ? nodes.Select(n => n.position).ToArray()
                : nodes.Where(n => !IsSkeletonPoint(n.i)).Select(n => n.position).ToArray(),
            Convexes = shape0.Convexes,
            Materials = staticNormModel.Convexes.Length == (shape0.Materials?.Length ?? 0) ? shape0.Materials : null,
        };

        if (options.ShowMeta)
        {
            shape = shape.ToCompositeMeta(options.Color2, options.Color1, options.MetaPointMult, options.MetaLineMult);
        }
        else
        {
            if (shape.Materials == null)
            {
                if (material != null)
                    shape = shape.ApplyMaterial(material);
                else if (options.Color1.HasValue)
                {
                    if (options.Color2.HasValue)
                        shape = shape.ApplyColorGradient(ExVector3.XyzAxis, options.Color1.Value, options.Color2.Value);
                    else
                        shape = shape.ApplyColor(options.Color1.Value);
                }
            }
        }

        shape = options.Show(shape);

        return shape;
    }
}
