using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

    double MaterialForceFn(double power, double border, double a, double y)
    {
        var x = y / a;

        if (x < border)
            x = border;

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
        var forceBorder = options.MaterialForceBorder;

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

                var fForce = -speed.ToLenWithCheck(options.CollideForceMult * options.GroundFrictionForce);
                speed = fForce.Length2 > speed.Length2
                    ? Vector3.Origin
                    : speed + fForce;
            }
            else
            {
                n.speedY = 0;

                var clForce = -Vector3.YAxis.ToLenWithCheck(options.CollideForceMult * options.GroundClingForce);
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
    
    bool CanCalc(Node n) => !n.locked;
    
    Vector3 FixY(Vector3 a) => a.y > options.Ground.Y ? a : new Vector3(a.x, options.Ground.Y, a.z);


    private void ParticleInteraction()
    {
        var interactionCounter = 0;

        foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
        {
            foreach (var b in worldNet.SelectNeighbors(a))
                foreach (var nb in b.Nodes)
                    foreach (var na in a.Model.net.SelectItemsByRadius(nb.position - a.Model.center, options.ForceInteractionRadius))
                    {
                        var ma = MaterialInteractionAcceleration(nb.mass * options.Interaction.InteractionForce, options.Interaction.EdgeSize.Value, (na.position - nb.position).Length);

                        if (options.UsePowerLimit)
                            if (ma * na.mass > options.PowerLimit)
                                ma = options.PowerLimit / na.mass;

                        na.speed += (na.position - nb.position).ToLenWithCheck(ma);
                        interactionCounter++;
                    }

            if (a.Options.UseSelfInteractions)
            {
                foreach (var na in a.Nodes)
                    foreach (var nb in a.Model.net.SelectItemsByRadius(na.position - a.Model.center, options.ForceInteractionRadius).Where(n => !na.selfInteractions.Contains(n.i)))
                    {
                        var ma = MaterialInteractionAcceleration(nb.mass * options.Interaction.InteractionForce, options.Interaction.EdgeSize.Value, (na.position - nb.position).Length);
                        na.speed += (na.position - nb.position).ToLenWithCheck(ma);
                        interactionCounter++;
                    }
            }
        }
        //Debug.WriteLine(interactionCounter);
    }

    Vector3 GetPointSpeed(Node[] ns, Plane plane, Vector3 p)
    {
        var a = (ns[plane.i].position - p).Length;
        var b = (ns[plane.j].position - p).Length;
        var c = (ns[plane.k].position - p).Length;

        var s = a * b + b * c + a * c;
        var speed = (b * c / s) * ns[plane.i].speed + (a * c / s) * ns[plane.j].speed + (a * b / s) * ns[plane.k].speed;

        return speed;
    }

    private void PlaneInteraction()
    {
        var interactionCounter = 0;

        foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
        {
            foreach (var b in worldNet.SelectNeighbors(a))
                foreach (var pb in b.Planes)
                {
                    var plane = new Model3D.Plane(b.Nodes[pb.i].position, b.Nodes[pb.j].position, b.Nodes[pb.k].position);
                    var nOne = plane.NOne;
                    var pCenter = plane.Center;
                    var pSize = plane.Size;
                    var pDistanceFn = plane.Fn;
                    var pProjFn = plane.ProjectionFn;
                    var isPointInsideFn = plane.IsPointInsideFn;

                    foreach (var na in a.Model.net.SelectItemsByRadius(pCenter - a.Model.center, pSize))
                    {
                        var interPoint = pProjFn(na.position);
                        var triangleDistance = pDistanceFn(na.position);
                        var isCrossingTriangle = triangleDistance.Sgn() != pDistanceFn(na.position + nOne * a.Options.MaterialThickness).Sgn();
                        var isUnderTriangle = -a.Options.MaterialThickness <= triangleDistance && triangleDistance <= 0;
                        var isInsideTriangle = isPointInsideFn(interPoint);
                        var isInsideMaterial = isUnderTriangle && isInsideTriangle;
                        var isCrossingMaterial = isCrossingTriangle && isInsideTriangle;

                        if (isCrossingMaterial)
                        {
                            var interPointSpeed = GetPointSpeed(b.Nodes, pb, interPoint);
                            var interSpeed = na.speed - interPointSpeed;
                            var interSpeedN = interSpeed.MultS(nOne);

                            if (interSpeedN < 0)
                            {
                                var slidingSpeed = interSpeed - nOne * interSpeedN;
                                var frictionForce = -slidingSpeed.ToLenWithCheck(Math.Max(slidingSpeed.Length, options.CollideForceMult * options.MaterialFrictionForce), Epsilon);
                                var clingForce = (-options.CollideForceMult * options.MaterialClingForce) * nOne;
                                var rSpeed = interSpeedN.Abs() * nOne + /*frictionForce + */clingForce;
                                na.rejectionSpeed += rSpeed;
                                b.Nodes[pb.i].rejectionSpeed += -rSpeed / 3;
                                b.Nodes[pb.j].rejectionSpeed += -rSpeed / 3;
                                b.Nodes[pb.k].rejectionSpeed += -rSpeed / 3;
                            }
                        }

                        if (isInsideMaterial)
                        {
                            var interPointSpeed = GetPointSpeed(b.Nodes, pb, interPoint);
                            var interSpeed = na.speed - interPointSpeed;
                            var interSpeedN = interSpeed.MultS(nOne);
                            var slidingSpeed = interSpeed - nOne * interSpeedN;
                            var frictionForce = -slidingSpeed.ToLenWithCheck(Math.Max(slidingSpeed.Length, options.CollideForceMult * options.MaterialFrictionForce), Epsilon);

                            var elasticForce = (triangleDistance.Abs() * options.CollideForceMult * options.MaterialElasticForce) * nOne;
                            var force = elasticForce + frictionForce;
                            na.speed+= force;
                            b.Nodes[pb.i].speed += -force / 3;
                            b.Nodes[pb.j].speed += -force / 3;
                            b.Nodes[pb.k].speed += -force / 3;
                        }

                        interactionCounter++;
                    }
                }
        }

        activeShapes.Where(a => a.Options.UseInteractions)
            .ForEach(a => a.Nodes
                .ForEach(n =>
                {
                    n.speed += n.rejectionSpeed;
                    n.rejectionSpeed = Vector3.Origin;
                }));

        Debug.WriteLine(interactionCounter);
    }

}