﻿using System;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;

namespace Model3D.Actives;

public partial class ActiveWorld // Rules
{
    private double materialInteractionForceBorder = 0.0001;
    private double interactionCoef = 0.000001;
    private double middleInteractionValue = 0.5;
    private double minInteractionMult = 3;

    double GetForceInteractionRadius(double edgeSize) => edgeSize * middleInteractionValue * minInteractionMult;

    double MaterialInteractionAcceleration(double power, double a, double y) 
    {
        var x = y / a;

        if (x < materialInteractionForceBorder) 
            x = materialInteractionForceBorder;

        return power * interactionCoef / x.Pow4();
    }

    private double materialForceBorder = 0.75;
    double MaterialForceFn(double power, double a, double y)
    {
        var x = y / a;

        if (x < materialForceBorder)
            x = materialForceBorder;

        return power * (x - 1) * (x + 1) / x.Pow4();
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) => (a - c).MultV(b - c);

    double GetVolume(Vector3 a, Vector3 b, Vector3 c) => c.MultS(GetNormal(a, b, c));

    double GetActiveShapeVolume(ActiveShape a) => a.Planes.Select(p => GetVolume(a.Nodes[p.i].position, a.Nodes[p.j].position, a.Nodes[p.k].position)).Sum();

    Vector3 GetAngleSpeed(Vector3 rotationSpeed, Vector3 rotationPosition) => rotationSpeed.MultV(rotationPosition) / rotationPosition.Length2;

    Vector3 GetActiveShapeAngleSpeed(ActiveShape a) =>
        1.5 * a.NoSkeletonNodes
        .Where(n => (n.position - a.Model.center).Length2 > Epsilon2)
        .Select(n => GetAngleSpeed(n.speed - a.Model.speed, n.position - a.Model.center))
        .Center();

    Vector3 BlowForce(Node n) => n.planes.Select(p => GetNormal(n.nodes[p.i].position, n.nodes[p.j].position, n.nodes[p.k].position)).Center();

    Vector3 CalcMaterialForce(Node n, ActiveShapeOptions shapeOptions)
    {
        var p0 = n.position;
        Vector3 materialForce = Vector3.Origin;

        foreach (var e in n.edges)
        {
            var sn = n.nodes[e.j];
            var p = sn.position;

            var d = (p - p0).Length;

            var fc = e.type switch
            {
                EdgeType.Skeleton => shapeOptions.SkeletonPower * options.MaterialForceMult,
                _ => shapeOptions.MaterialPower * options.MaterialForceMult,
            };

            var ds = MaterialForceFn(fc, e.fA, d);

            materialForce += ds * (p - p0) / d;
        }

        return materialForce;
    }

    Vector3 CalcGroundSpeed(Node n, ActiveShapeOptions shapeOptions)
    {
        var speed = n.speed;

        if (IsBottom(n))
        {
            if (speed.y < 0)
            {
                n.speedY += -speed.y;
                speed = speed.SetY(0);

                var fForce = -speed.ToLenWithCheck(options.CollideForceMult * options.FrictionForce);
                speed = fForce.Length2 > speed.Length2
                    ? Vector3.Origin
                    : speed + fForce;
            }
            else
            {
                n.speedY = 0;

                var clForce = -Vector3.YAxis.ToLenWithCheck(options.CollideForceMult * options.ClingForce);
                speed = clForce.Length2 > speed.VectorY().Length2
                    ? Vector3.Origin
                    : speed + clForce;
            }
        }

        return speed;
    }

    Vector3 CalcMaterialDampingForce(ActiveShape a, Node n)
    {
        var v = n.speed - a.Model.speed;

        var r = n.position - a.Model.center;
        var nl = a.Model.angleSpeed.Normalize();
        var anl = nl.MultS(r);
        var aCenter = a.Model.center + a.Model.angleSpeed.ToLenWithCheck(anl);
        var ar = n.position - aCenter;
        var aSpeed = a.Model.angleSpeed;

        var m = v + aSpeed.MultV(ar);
        var res = -a.Options.MaterialDamping * m;

        return res;
    }

    Vector3 CalcBlowForce(ActiveShape a, Node n)
    {
        var o = a.Options;
        var blowForce = o.BlowPower * a.Model.volume0 / a.Model.volume - options.PressurePower;

        if (o.BlowPower > 0 && n.planes.Length > 0)
            return blowForce * options.PressurePowerMult * BlowForce(n);
        else
            return Vector3.Origin;
    }

    Vector3 CalcBounceForce(Node n)
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

    Vector3 CalcGroundForce() => (options.Ground.GravityPower * options.Ground.Gravity + options.Ground.WindPower * options.Ground.Wind) / options.OverCalculationMult;
    Vector3 CalcSpaceForce(Node n) => (options.Space.MassCenter - n.position).ToLenWithCheck(options.Space.GravityConst * options.Space.GravityPower / options.OverCalculationMult / (n.position - options.Space.MassCenter).Length2);

    bool IsBottom(Node n) => n.position.y <= options.Ground.Y;
    
    bool CanCalc(Node n) => !n.locked;
    
    Vector3 FixY(Vector3 a) => a.y > options.Ground.Y ? a : new Vector3(a.x, options.Ground.Y, a.z);
}