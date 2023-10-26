using System;
using System.Collections.Generic;
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


    public ActiveWorld(ActiveWorldOptions options = null, ActiveShapeOptions activeShapeOptions = null)
    {
        this.options = options ?? ActiveWorldValues.DefaultActiveWorldOptions;
        this.activeShapeOptions = activeShapeOptions;
    }

    public void AddActiveShape(ActiveShape shape) => activeShapes.Add(shape);
    public void AddActiveShapes(IEnumerable<ActiveShape> shapes) => activeShapes.AddRange(shapes);
    public void AddActiveShape(Shape shape) => activeShapes.Add(shape.ToActiveShape(activeShapeOptions));
    public void AddActiveShapes(IEnumerable<Shape> shapes) => activeShapes.AddRange(shapes.Select(shape => shape.ToActiveShape(activeShapeOptions)));
    public void AddShape(Shape shape) => shapes.Add(shape);
    public void AddShapes(IEnumerable<Shape> ss) => shapes.AddRange(ss);

    
    private double forceBorder = 0.75;
    double BlockForceFn(double power, double a, double y)
    {
        var x = y / a;

        if (x < forceBorder)
            x = forceBorder;

        return power * (x - 1) * (x + 1) / x.Pow4();
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) => (a - c).MultV(b - c);

    Vector3 BlowForce(Node n) => n.planes.Select(p => GetNormal(n.nodes[p.i].position, n.nodes[p.j].position, n.nodes[p.k].position)).Center();

    Vector3 CalcSpeed(Node n, ActiveShapeOptions shapeOptions)
    {
        var p0 = n.position;
        Vector3 offset = Vector3.Origin;

        foreach (var e in n.edges)
        {
            var sn = n.nodes[e.j];
            var p = sn.position;

            var d = (p - p0).Length;

            var fc = e.type switch
            {
                EdgeType.Skeleton => shapeOptions.SkeletonPower,
                _ => shapeOptions.MaterialPower,
            };

            var ds = BlockForceFn(fc, e.fA, d);

            offset += ds * (p - p0) / d;
        }

        var speed = n.speed + offset * options.MaterialDapming;

        if (IsBottom(n))
        {
            if (speed.y < 0)
            {
                n.speedY += -speed.y;
                speed = speed.SetY(0);

                var fForce = -speed.ToLenWithCheck(options.MaterialForceMult * options.FrictionForce);
                speed = fForce.Length2 > speed.Length2
                    ? Vector3.Origin
                    : speed + fForce;
            }
            else
            {
                n.speedY = 0;

                var clForce = -Vector3.YAxis.ToLenWithCheck(options.MaterialForceMult * options.ClingForce);
                speed = clForce.Length2 > speed.VectorY().Length2
                    ? Vector3.Origin
                    : speed + clForce;
            }
        }

        return speed;
    }

    Vector3 CalcBlowSpeedOffset(Node n, ActiveShapeOptions shapeOptions)
    {
        if (shapeOptions.BlowPower > 0 && n.planes.Length > 0)
            return shapeOptions.BlowPower * BlowForce(n);
        else
            return Vector3.Origin;
    }

    Vector3 CalcBounceSpeedOffset(Node n)
    {
        Vector3 offset = Vector3.Origin;
        var sns = n.edges.Select(e => n.nodes[e.j]).Where(IsBottom).ToArray();

        if (sns.Length == 0)
            return offset;

        var sumY = sns.Select(sn => n.position.y - sn.position.y).Sum();

        foreach (var (sn, i) in sns.Select((v, i) => (v, i)))
        {
            offset += (n.position - sn.position).ToLenWithCheck(n.speedY * (n.position.y - sn.position.y) / sumY);
        }

        return offset;
    }

    bool IsBottom(Node n) => n.position.y <= 0;
    //bool CanCalc(Node n) => !fixBottom || n.position.y > bY.a;
    Vector3 FixY(Vector3 a) => a.y > 0 ? a : new Vector3(a.x, 0, a.z);

    private void Activate()
    {
        if (options.UseDefaultGround)
            AddShape(Surfaces.Plane(11, 11).Normalize().ToOy().Perfecto(10).ToLines(1, Color.Black));

        activeShapes.ForEach(a => a.Activate());
    }

    int nStep = 0;
    private void Step()
    {
        foreach (var s in activeShapes)
        {
            if (s.Options.BlowUp != null)
            {
                if (nStep > s.Options.BlowUp.SinceStep)
                    s.Options.BlowPower += s.Options.BlowUp.BlowUpStepPower;
            }

            s.Nodes.ForEach(n => n.speed += (options.GravityPower * options.Gravity + options.WindPower * options.Wind) / options.OverCalculationMult);
            s.Nodes.ForEach(n => n.speed += CalcBlowSpeedOffset(n, s.Options));
            s.Nodes.ForEach(n => n.speed = CalcSpeed(n, s.Options));
            s.Nodes.Where(n => !IsBottom(n)).ForEach(n => n.speed += CalcBounceSpeedOffset(n));
            s.Nodes.ForEach(n => n.position += n.speed);
            s.Nodes.ForEach(n => n.position = FixY(n.position));
        }

        nStep++;
    }

    public IEnumerable<Shape> Animate()
    {
        Activate();
        var statics = shapes.ToSingleShape();

        for (var i = 0; i < options.SceneCount; i++)
        {
            yield return activeShapes.Select(s => s.GetStepShape()).ToSingleShape()
                       + statics;

            (options.OverCalculationMult * options.StepsPerScene).ForEach(_ => Step());
        }
    }
}
