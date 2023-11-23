using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using static Model3D.Actives.ActiveWorld;

namespace Model3D.Actives;

public partial class ActiveWorld // Interaction
{

    private void UpdateCollidePositions()
    {
        activeShapes.ForEach(a => a.Nodes.ForEach(n =>
        {
            n.collidePosition = n.position + GetNodeNormal(n) * options.JediMaterialThickness;
        }));
    }

    private void ParticleWithParticleInteraction()
    {
        var interactionCounter = 0;

        foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
        {
            foreach (var b in worldNet.SelectNeighbors(a))
                foreach (var nb in b.Nodes)
                    foreach (var na in a.Model.net.SelectItemsByRadius(nb.position - a.Model.center, options.ForceInteractionRadius))
                    {
                        var ma = MaterialInteractionAcceleration(nb.mass * options.Interaction.ParticleForce, options.Interaction.EdgeSize.Value, (na.position - nb.position).Length);

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
                        var ma = MaterialInteractionAcceleration(nb.mass * options.Interaction.ParticleForce, options.Interaction.EdgeSize.Value, (na.position - nb.position).Length);
                        na.speed += (na.position - nb.position).ToLenWithCheck(ma);
                        interactionCounter++;
                    }
            }
        }

        //Debug.WriteLine(interactionCounter);
    }

    bool IsAtCollideDistance(ActiveShapeOptions o, double distance) => -(options.MaterialThickness + options.JediMaterialThickness) < distance && distance <= 0; // check side planes

    private void ParticleWithPlaneInteraction()
    {
        UpdateCollidePositions();

        var interactionCounter = 0;

        foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
        {
            foreach (var b in worldNet.SelectNeighbors(a))
                foreach (var pb in b.Planes)
                {
                    var plane = pb.collidePlane;
                    var nOne = plane.NOne;
                    var pCenter = plane.Center;
                    var pSize = plane.Size;
                    var pDistanceFn = plane.Fn;
                    var pProjFn = plane.ProjectionFn;
                    var isPointInsideFn = plane.IsPointInsideFn;

                    foreach (var na in a.Model.net.SelectItemsByRadius(pCenter - a.Model.center, pSize))
                    {
                        var collidePoint = pProjFn(na.position);
                        var collideDistance = pDistanceFn(na.position);
                        var isAtCollideDistance = IsAtCollideDistance(a.Options, collideDistance);
                        var isInsideTriangle = isPointInsideFn(collidePoint);
                        var isColliding = isAtCollideDistance && isInsideTriangle;

                        na.isColliding = na.isColliding || isColliding;

                        if (isColliding)
                        {
                            CollideWithPlaneB(na, pb, nOne, collidePoint, collideDistance);
                        }

                        interactionCounter++;
                    }
                }

            if (a.Options.UseSelfInteractions)
            {
                // todo
            }    
        }

        activeShapes.Where(a => a.Options.UseInteractions)
            .ForEach(a => a.Nodes
                .ForEach(n =>
                {
                    if (n.isColliding)
                    {
                        if (!n.isInsideMaterial) // is crossing
                        {
                            n.nRejectionDir = n.rejectionDirSum.Normalize();
                            n.rejectionDirSum = Vector3.Origin;
                        }

                        n.speed += n.collideForce / n.collideCount;
                    }
                    else
                    {
                        n.nRejectionDir = Vector3.Origin;
                    }

                    n.collideCount = 0;
                    n.collideForce = Vector3.Origin;
                    n.isInsideMaterial = n.isColliding;
                    n.isColliding = false;
                }));

        //Debug.WriteLine(interactionCounter);
    }

    private void CollideWithPlaneB(Interactor rra, Interactor rrb, Vector3 bnOne, Vector3 collidePoint, double collideDistance)
    {
        var aPack = rra.pointPackFn(collidePoint);
        var bPack = rrb.pointPackFn(collidePoint);

        var interSpeed = aPack.speed - bPack.speed;
        var interSpeedN = interSpeed.MultS(bnOne);
        var slidingSpeed = interSpeed - bnOne * interSpeedN;

        if (!rra.isInsideMaterial)
            rra.rejectionDirSum += bnOne;

        var frictionForce = GetPlaneFrictionForce(slidingSpeed);
        var clingForce = GetPlaneClingForce(bnOne);
        var elasticForce = GetPlaneElasticForce(rra.nRejectionDir, bnOne, collideDistance);

        var collideForce = elasticForce + frictionForce + clingForce;
        var collideMass = 0.25 * (aPack.mass + bPack.mass); // half of collide collideMass (both side calculation) 

        rra.applyCollideForce(aPack, collideMass, collideForce);
        rrb.applyCollideForce(bPack, collideMass, -collideForce);
    }

    private void EdgeWithPlaneInteraction()
    {
        UpdateCollidePositions();

        var interactionCounter = 0;

        foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
        {
            var materialThickness = options.MaterialThickness + options.JediMaterialThickness;

            foreach (var b in worldNet.SelectNeighbors(a))
                foreach (var pb in b.Planes)
                {
                    var plane = pb.collidePlane;
                    var nOne = plane.NOne;
                    var pCenter = plane.Center;
                    var pSize = plane.Size;
                    var pDistanceFn = plane.Fn;
                    var pProjFn = plane.ProjectionFn;
                    var isPointInsideFn = plane.IsPointInsideFn;
                    var lineCrossFn = plane.IntersectionFn;

                    foreach (var ea in a.Model.net.SelectItemsByRadius(pCenter - a.Model.center, pSize).SelectMany(n => n.edges))
                    {
                        var (hasCrossPoint, crossPoint) = lineCrossFn(ea.positionI, ea.positionJ).SplitNullable();

                        if (hasCrossPoint)
                        {
                            var crossPointDistance = pDistanceFn(crossPoint);
                            var crossPointIsAtCollideDistance = IsAtCollideDistance(a.Options, crossPointDistance);
                            var crossPointIsInsideTriangle = isPointInsideFn(crossPoint);
                            var crossPointIsColliding = crossPointIsAtCollideDistance && crossPointIsInsideTriangle;

                            if (crossPointIsColliding)
                            {
                                CollideWithPlaneB(ea, pb, nOne, crossPoint, crossPointDistance);
                            }
                            else
                            {
                                // тут - алгоритм
                                var distanceI = pDistanceFn(ea.positionI);
                                var distanceJ = pDistanceFn(ea.positionJ);

                                if (distanceI < 0 && distanceJ < 0)
                                {

                                }
                                else
                                {

                                }
                            }
                        }
                        else // edge is parallel to plane
                        {
                            var ebPoint = ea.positionCenter;
                        }

                        //var crossPoint = pProjFn(ea.ni.position);
                        //var collideDistance = pDistanceFn(rra.position);
                        //var isAtCollideDistance = -a.Options.MaterialThickness <= collideDistance && collideDistance <= 0;
                        //var isInsideTriangle = isPointInsideFn(crossPoint);
                        //var isColliding = isAtCollideDistance && isInsideTriangle;

                        //rra.isColliding = rra.isColliding || isColliding;

                        //if (isColliding)
                        //{
                        //    var forceDistance = -collideDistance / a.Options.MaterialThickness;
                        //    var pK = GetPlanePointK(b.Nodes, rrb, crossPoint);
                        //    var interPointSpeed = GetPointSpeed(b.Nodes, rrb, pK);
                        //    var interSpeed = rra.speed - interPointSpeed;
                        //    var interSpeedN = interSpeed.MultS(bnOne);
                        //    var slidingSpeed = interSpeed - bnOne * interSpeedN;
                        //    var rejectionSpeed = interSpeed - 2 * interSpeedN * bnOne;

                        //    if (!rra.isInsideMaterial)
                        //        rra.rejectionDirSum += bnOne;

                        //    var frictionForce = slidingSpeed.ToLenWithCheck(-Math.Min(slidingSpeed.Length, options.PlaneConst * options.Interaction.MaterialFrictionForce));
                        //    var clingForce = (-options.PlaneConst * options.Interaction.MaterialClingForce) * bnOne;

                        //    var dirFactor = rra.nRejectionDir.MultS(bnOne);

                        //    var elasticForce = dirFactor > 0
                        //        ? (dirFactor * forceDistance * options.PlaneConst * options.Interaction.PlaneForce) * rra.nRejectionDir
                        //        : Vector3.Origin;

                        //    var collideForce = elasticForce + frictionForce + clingForce;

                        //    var collideMass = 0.25 * (rra.collideMass + pK.x * rrb.ni.collideMass + pK.y * rrb.nj.collideMass + pK.z * rrb.nk.collideMass); // 0.5 (both side calculation) * middle interaction collideMass

                        //    rra.collideForce += (collideMass / rra.collideMass) * collideForce;
                        //    rra.collideCount++;
                        //    rrb.ni.collideForce -= (collideMass * pK.x / rrb.ni.collideMass) * collideForce;
                        //    rrb.ni.collideCount++;
                        //    rrb.nj.collideForce -= (collideMass * pK.y / rrb.nj.collideMass) * collideForce;
                        //    rrb.nj.collideCount++;
                        //    rrb.nk.collideForce -= (collideMass * pK.z / rrb.nk.collideMass) * collideForce;
                        //    rrb.nk.collideCount++;
                        //}

                        interactionCounter++;
                    }
                }

            if (a.Options.UseSelfInteractions)
            {
                // todo
            }
        }

        activeShapes.Where(a => a.Options.UseInteractions)
            .ForEach(a => a.Nodes
                .ForEach(n =>
                {
                    if (n.isColliding)
                    {
                        if (!n.isInsideMaterial) // is crossing
                        {
                            n.nRejectionDir = n.rejectionDirSum.Normalize();
                            n.rejectionDirSum = Vector3.Origin;
                        }

                        n.speed += n.collideForce / n.collideCount;
                    }
                    else
                    {
                        n.nRejectionDir = Vector3.Origin;
                    }

                    n.collideCount = 0;
                    n.collideForce = Vector3.Origin;
                    n.isInsideMaterial = n.isColliding;
                    n.isColliding = false;
                }));

        //Debug.WriteLine(interactionCounter);
    }

}
