using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Model3D.AsposeModel;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;

namespace Model3D.Actives;
public partial class ActiveWorld // Rules
{
    private double materialInteractionForceBorder = 0.2;
    private double interactionCoef4 = 0.000001;
    private double middleInteractionValue = 0.5;
    private double minInteractionMult = 3;

    double GetForceInteractionRadius(double edgeSize) => edgeSize * middleInteractionValue * minInteractionMult;


    double PlaneInteractionAcceleration(double power, double a, double y)
    {
        var x = y / a;

        if (x < materialInteractionForceBorder)
            x = materialInteractionForceBorder;

        return power / x.Pow2();
    }

    double ParticleInteractionAcceleration(double power, double a, double y)
    {
        var x = y / a;

        if (x < materialInteractionForceBorder)
            x = materialInteractionForceBorder;

        return power * interactionCoef4 / x.Pow4();
    }

    double MaterialForceFn(double power, double border, double a, double y)
    {
        var x = y / a;

        if (x < border)
            x = border;

        return power * (x - 1) * (x + 1) / x.Pow4();
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) => (a - c).MultV(b - c);
    Vector3 GetNodeNormal(Node n) => n.planes.Select(p => p.ni.position.GetPlaneNormal(p.nj.position, p.nk.position)).Sum().Normalize();
    double GetVolume(Vector3 a, Vector3 b, Vector3 c) => c.MultS(GetNormal(a, b, c));

    double GetActiveShapeVolume(ActiveShape a) => a.Options.Type == ActiveShapeOptions.ShapeType.D3
        ? a.Planes.Select(p => GetVolume(a.Nodes[p.i].position, a.Nodes[p.j].position, a.Nodes[p.k].position)).Sum()
        : 0;

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
        var forceBorder = options.MaterialForceBorder;

        foreach (var e in n.edges)
        {
            var sn = n.nodes[e.j];
            var p = sn.position;

            var d = (p - p0).Length;

            var fc = e.type switch
            {
                EdgeType.Skeleton => shapeOptions.Skeleton.Power * options.MaterialForceMult,
                _ => shapeOptions.MaterialPower * options.MaterialForceMult,
            };

            var ds = MaterialForceFn(fc, forceBorder, e.fA, d);

            materialForce += ds * (p - p0) / d;
        }

        if (options.UsePowerLimit)
            if (materialForce.Length2 > options.PowerLimit* options.PowerLimit)
                materialForce = materialForce.ToLen(options.PowerLimit);

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

                var fForce = -speed.ToLenWithCheck(options.ParticleConst * options.Ground.FrictionForce);
                speed = fForce.Length2 > speed.Length2
                    ? Vector3.Origin
                    : speed + fForce;
            }
            else
            {
                n.speedY = 0;

                var clForce = -Vector3.YAxis.ToLenWithCheck(options.ParticleConst * options.Ground.ClingForce);
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
        var angleV = a.Model.angleSpeed;

        var r = n.position - a.Model.center;
        var rCenterOffset = angleV.Normalize().MultS(r);
        var rotationCenter = a.Model.center + angleV.ToLenWithCheck(rCenterOffset);
        var rotationRadiusV = rotationCenter - n.position;
        
        if (rotationRadiusV.Length2 < Epsilon2)
            return Vector3.Origin;

        var rotationSpeed = angleV.MultV(rotationRadiusV);
        var materialSpeed = v - rotationSpeed;
        var rotationDamping = 0.02 * rotationSpeed.Length / rotationRadiusV.Length;
        var res = -a.Options.MaterialDamping * materialSpeed - rotationDamping * rotationSpeed;

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
    Vector3 CalcSpaceForce(Node n) => (options.MassCenter.MassCenter - n.position).ToLenWithCheck(options.MassCenter.GravityConst * options.MassCenter.GravityPower / options.OverCalculationMult / (n.position - options.MassCenter.MassCenter).Length2);

    bool IsBottom(Node n) => n.position.y <= options.Ground.Y;
    
    bool NoLock(Node n) => !n.locked;
    
    Vector3 FixY(Vector3 a) => a.y > options.Ground.Y ? a : new Vector3(a.x, options.Ground.Y, a.z);

    Vector3 GetPlanePointK(Plane plane, Vector3 p)
    {
        var a = (plane.positionI - p).Length;
        var b = (plane.positionJ - p).Length;
        var c = (plane.positionK - p).Length;

        var s = a * b + b * c + a * c;

        return new Vector3(b * c / s, a * c / s, a * b / s);
    }

    Vector3 GetPointSpeed(Plane plane, Vector3 pK)
    {
        return pK.x * plane.ni.speed + pK.y * plane.nj.speed + pK.z * plane.nk.speed;
    }


    #region Plane interaction
    
    double GetPlaneForceByDistance(double distance) => PlaneInteractionAcceleration(1, options.MaterialThickness + options.JediMaterialThickness, options.MaterialThickness + options.JediMaterialThickness + distance);

    Vector3 GetPlaneFrictionForce(Vector3 slidingSpeed)
    {
        var force = options.PlaneConst * options.Interaction.FrictionForce;

        if (slidingSpeed.Length2 < force* force)
            return -slidingSpeed;

        return slidingSpeed.ToLenWithCheck(-force, Values.Epsilon12);
    }

    Vector3 GetPlaneClingForce(double nSpeed, Vector3 nDir) => (-nSpeed.Sgn() * Math.Min(nSpeed.Abs(), options.PlaneConst * options.Interaction.ClingForce)) * nDir;

    Vector3 GetPlaneElasticForce(Vector3 nRejectionDir, Vector3 nOne, double distance)
    {
        var distanceForce = GetPlaneForceByDistance(distance);
        var rejectFactor = nRejectionDir.MultS(nOne);

        var elasticForce = rejectFactor > 0
            ? (rejectFactor * distanceForce * options.PlaneConst * options.Interaction.ElasticForce) * nRejectionDir
            : Vector3.Origin;

        return elasticForce;
    }
    #endregion
}