using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Model3D.AsposeModel;
using MessagePack;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;
using ModelShapes = Model.Libraries.Shapes;

namespace Model3D.Actives;
public partial class ActiveWorld
{
    private readonly ActiveWorldOptions options;
    private readonly ActiveShapeOptions activeShapeOptions;
    private List<ActiveShape> activeShapes = new();
    private List<Shape> shapes = new();
    private Net3<ActiveShape> worldNet;
    private WorldExportState exportState;

    public ActiveWorldOptions Options => options;
    public List<ActiveShape> ActiveShapes => activeShapes;
    public List<Shape> Shapes => shapes;

    public ActiveWorld(ActiveWorldOptions options = null, ActiveShapeOptions activeShapeOptions = null)
    {
        this.options = options ?? ActiveWorldValues.DefaultActiveWorldOptions;
        this.activeShapeOptions = activeShapeOptions;
    }

    public void AddActiveShape(ActiveShape shape) { shape.Options.With(o => o.WorldOptions = options); activeShapes.Add(shape); }
    public void AddActiveShapes(IEnumerable<ActiveShape> shapes) => shapes.ForEach(AddActiveShape);
    public void AddActiveShape(Shape shape) => AddActiveShape(shape.ToActiveShape(activeShapeOptions));
    public void AddActiveShapes(IEnumerable<Shape> shapes) => shapes.ForEach(AddActiveShape);
    public void AddShape(Shape shape) => shapes.Add(shape);
    public void AddShapes(IEnumerable<Shape> ss) => shapes.AddRange(ss);

    
    private void Activate()
    {
        if (options.Ground != null && options.UseGround && options.Ground.ShowGround)
        {
            var o = options.Ground;
            var ground = Surfaces.Plane(o.Size, o.Size).Normalize().ToOy().Perfecto(o.Mult).MoveY(options.Ground.Y).ToLines(o.LineMult, o.Color ?? Color.Black);
            AddShape(ground);

            if (options.Ground.UseWaves)
            {
                options.AllowModifyStatics = true;

                options.OnStep += w =>
                {
                    var t = w.Options.StepNumber * 0.0001;
                    w.Shapes[^1] = ground.ToOyM().Mult(1/options.Ground.WavesSize).ApplyZ(Funcs3Z.ActiveWaves(t)).Mult(options.Ground.WavesSize).ToOy();
                };
            }
        }

        if (options.InteractionType.HasAnyFlag(InteractionType.Any))
        {
            if (options.Interaction.EdgeSize == null)
            {
                options.Interaction.EdgeSize = options.Interaction.EdgeSizeMult * activeShapes.Select(a => a.Shape.AverageEdgeLength).Average();
            }

            options.ForceInteractionRadius = GetForceInteractionRadius(options.Interaction.EdgeSize.Value);
        }

        activeShapes.ForEach(a =>
        {
            a.Activate();            
            a.Model.volume0 = GetActiveShapeVolume(a);
        });

        if (options.InteractionType.HasAnyFlag(InteractionType.Any))
        {
            var clusterSize = activeShapes.Select(a => (a.Model.borders0.max - a.Model.borders0.min).Length).Max();
            worldNet = new Net3<ActiveShape>(activeShapes.ToArray(), clusterSize, true, options.Interaction.InteractionAreaScale);

            if (options.Interaction.UseVolumeMass)
            {
                var d3ActiveShapes = activeShapes.Where(a => a.Options.Type == ActiveShapeOptions.ShapeType.D3).ToArray();

                if (d3ActiveShapes.Length > 0)
                {
                    var worldVolume = d3ActiveShapes.Select(a => a.Model.volume0).Sum();
                    var averageVolume = worldVolume / d3ActiveShapes.Length;

                    d3ActiveShapes.ForEach(a =>
                    {
                        var massCoef = a.Model.volume0 / averageVolume;
                        a.Nodes.ForEach(n => n.mass *= massCoef);
                    });
                }
            }
        }

        if (options.UseExport)
        {
            var metaSize = ModelShapes.Icosahedron.PointsCount;
            var offset = 0;

            exportState = new WorldExportState
            { 
                actives = activeShapes.Select(s =>
                {
                    var count = s.Shape.PointsCount;
                    var size = s.Options.ShowMeta ? metaSize : 1;
                    var active = new WorldExportState.Active { offset = offset, count = count, size = size, moves = [] };
                    offset += count * size;

                    return active;
                }).ToArray()
            };

            WriteExportState();
        }
    }

    int nStep = 0;
    private void Step()
    {
        if (options.UseExport)
        {
            if (options.Export.FrameFn(nStep))
            {
                if (exportState.actives.Length != activeShapes.Count)
                    throw new NotImplementedException("Dynamic actives export");

                activeShapes.ForEach((a, i) =>
                    exportState.actives[i].moves
                        .Add(a.Nodes.Select(n => n.position - n.position0).Select(p => new[] { (float)p.x, (float)p.y, (float)p.z }).ToArray()));
            }

            if (options.Export.FrameSaveFn(nStep))
                WriteExportState();
        }

        options.StepNumber = nStep;

        activeShapes.ForEach(a =>
        {
            a.Options.StepNumber = nStep;
            a.Step();
        });

        options.Step(this);

        activeShapes.ForEach(a => a.Model.center = a.NoSkeletonNodes.Select(n => n.position).Center());

        if (options.InteractionType.HasAnyFlag(InteractionType.Any))
        {
            worldNet.Update();

            foreach (var a in activeShapes)
            {                
                if (a.Options.UseInteractions)
                    a.Model.net.Update();
            }

            if (options.InteractionType.HasFlag(InteractionType.ParticleWithParticle))
                ParticleWithParticleInteraction();

            if (options.InteractionType.HasFlag(InteractionType.ParticleWithPlane))
                ParticleWithPlaneInteraction();

            if (options.InteractionType.HasFlag(InteractionType.EdgeWithPlane))
                EdgeWithPlaneInteraction();
        }

        foreach (var a in activeShapes)
        {
            a.Nodes.Where(NoLock).ForEach(n => n.speed += CalcMaterialForce(n, a.Options));

            if (a.Options.UseBlow)
            {
                a.Model.volume = GetActiveShapeVolume(a);
                a.Nodes.Where(NoLock).ForEach(n => n.speed += CalcBlowForce(a, n));
            }

            if (a.Options.UseMaterialDamping)
            {                
                a.Model.speed = a.NoSkeletonNodes.Select(n => n.speed).Center();
                a.Model.angleSpeed = GetActiveShapeAngleSpeed(a);
                a.NoSkeletonNodes.Where(NoLock).ForEach(n => n.speed += CalcMaterialDampingForce(a, n));
            }

            if (options.UseMassCenter)
            {
                a.Nodes.Where(NoLock).ForEach(n => n.speed += CalcSpaceForce(n));
            }

            if (options.UseGround)
            {
                a.Nodes.Where(NoLock).ForEach(n => n.speed += CalcGroundForce());
                a.Nodes.Where(NoLock).ForEach(n => n.speed = CalcGroundSpeed(n, a.Options));
                a.Nodes.Where(NoLock).Where(n => !IsBottom(n)).ForEach(n => n.speed += CalcBounceForce(n));
            }

            a.Nodes.Where(NoLock).ForEach(n => n.position += n.speed);

            if (options.UseGround)
            {
                a.Nodes.Where(NoLock).ForEach(n => n.position = FixY(n.position));
            }
        }

        nStep++;
    }

    public void WriteExportState()
    {
        var bytes = MessagePackSerializer.Serialize(exportState);
        File.WriteAllBytes(options.Export.FileName, bytes);
    }

    public IEnumerable<Shape> Animate()
    {
        Activate();

        var statics = options.AllowModifyStatics ? null : shapes.ToSingleShape();

        (options.OverCalculationMult * options.SkipSteps).ForEach(_ => Step());

        for (var i = 0; i < options.SceneCount; i++)
        {
            yield return activeShapes.Select(s => s.GetStepShape()).ToSingleShape()
                       + (statics ?? shapes.ToSingleShape());

            (options.OverCalculationMult * options.StepsPerScene).ForEach(_ => Step());
        }

        if (options.UseExport)
            WriteExportState();
    }
}
