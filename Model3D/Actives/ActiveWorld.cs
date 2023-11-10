using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;

namespace Model3D.Actives;
public partial class ActiveWorld
{
    private readonly ActiveWorldOptions options;
    private readonly ActiveShapeOptions activeShapeOptions;
    private List<ActiveShape> activeShapes = new();
    private List<Shape> shapes = new();
    private Net3<ActiveShape> worldNet;

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

        options.ForceInteractionRadius = GetForceInteractionRadius(options.Interaction.EdgeSize);

        activeShapes.ForEach(a =>
        {
            a.Activate();            
            a.Model.volume0 = GetActiveShapeVolume(a);
        });

        if (options.UseInteractions)
        {
            var clusterSize = activeShapes.Select(a => (a.Model.borders0.max - a.Model.borders0.min).Length).Max();
            worldNet = new Net3<ActiveShape>(activeShapes.ToArray(), clusterSize, true, options.Interaction.InteractionAreaScale);

            if (options.Interaction.UseMass)
            {
                var worldVolume = activeShapes.Select(a => a.Model.volume0).Sum();
                var averageVolume = worldVolume / activeShapes.Count;
                activeShapes.ForEach(a =>
                {
                    var mass = a.Model.volume0 / averageVolume;
                    a.Nodes.ForEach(n=>n.mass = mass);
                });
            }
        }
    }

    int nStep = 0;
    private void Step()
    {
        options.StepNumber = nStep;

        activeShapes.ForEach(a =>
        {
            a.Options.StepNumber = nStep;
            a.Step();
        });

        options.Step(this);

        activeShapes.ForEach(a => a.Model.center = a.NoSkeletonNodes.Select(n => n.position).Center());

        if (options.UseInteractions)
        {
            worldNet.Update();

            foreach (var a in activeShapes)
            {                
                if (a.Options.UseInteractions)
                    a.Model.net.Update();
            }

            var interactionCounter = 0;

            foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
                foreach (var b in worldNet.SelectNeighbors(a))
                    foreach (var nb in b.Nodes)
                        foreach (var na in a.Model.net.SelectItemsByRadius(nb.position - a.Model.center, options.ForceInteractionRadius))
                        {
                            var ma = MaterialInteractionAcceleration(nb.mass * options.Interaction.InteractionForce, options.Interaction.EdgeSize, (na.position - nb.position).Length);
                            na.speed += (na.position - nb.position).ToLenWithCheck(ma);
                            interactionCounter++;
                        }              

            //Debug.WriteLine(interactionCounter);
        }

        foreach (var a in activeShapes)
        {
            a.Nodes.Where(CanCalc).ForEach(n => n.speed += CalcMaterialForce(n, a.Options));

            if (a.Options.UseBlow)
            {
                a.Model.volume = GetActiveShapeVolume(a);
                a.Nodes.Where(CanCalc).ForEach(n => n.speed += CalcBlowForce(a, n));
            }

            if (a.Options.UseMaterialDamping)
            {                
                a.Model.speed = a.NoSkeletonNodes.Select(n => n.speed).Center();
                a.Model.angleSpeed = GetActiveShapeAngleSpeed(a);
                a.NoSkeletonNodes.Where(CanCalc).ForEach(n => n.speed += CalcMaterialDampingForce(a, n));
            }

            if (options.UseSpace)
            {
                a.Nodes.Where(CanCalc).ForEach(n => n.speed += CalcSpaceForce(n));
            }

            if (options.UseGround)
            {
                a.Nodes.Where(CanCalc).ForEach(n => n.speed += CalcGroundForce());
                a.Nodes.Where(CanCalc).ForEach(n => n.speed = CalcGroundSpeed(n, a.Options));
                a.Nodes.Where(CanCalc).Where(n => !IsBottom(n)).ForEach(n => n.speed += CalcBounceForce(n));
            }

            a.Nodes.Where(CanCalc).ForEach(n => n.position += n.speed);
            a.Nodes.Where(CanCalc).ForEach(n => n.position = FixY(n.position));
        }

        nStep++;
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
    }
}
