using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Aspose.ThreeD.Utilities;
using MathNet.Numerics;
using Meta.Libraries;
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
            if (n.hasnDir)
            {
                n.nDir = GetNodeNormal(n);
                n.collidePosition = n.position + n.nDir * options.JediMaterialThickness;
            }
            else
            {
                n.collidePosition = n.position;
            }
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
                        var ma = ParticleInteractionAcceleration(nb.mass * options.Interaction.ParticleForce, options.Interaction.EdgeSize.Value, (na.position - nb.position).Length);

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
                        var ma = ParticleInteractionAcceleration(nb.mass * options.Interaction.ParticleForce, options.Interaction.EdgeSize.Value, (na.position - nb.position).Length);
                        na.speed += (na.position - nb.position).ToLenWithCheck(ma);
                        interactionCounter++;
                    }
            }
        }

        //Debug.WriteLine(interactionCounter);
    }

    bool IsAtCollideDistance(double distance) => -(options.MaterialThickness + options.JediMaterialThickness) < distance && distance <= 0; // check side planes

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
                    var isPointInsideFn = pb.IsInsideFn(nOne);

                    foreach (var na in a.Model.net.SelectItemsByRadius(pCenter - a.Model.center, pSize))
                    {
                        var isStrikeDir = !na.hasnDir || na.nDir.MultS(nOne) < 0;

                        if (isStrikeDir)
                        {
                            var collidePoint = pProjFn(na.position);
                            var collideDistance = pDistanceFn(na.position);
                            var isAtCollideDistance = IsAtCollideDistance(collideDistance);
                            var isInsideTriangle = isPointInsideFn(collidePoint, collideDistance);
                            var isColliding = isAtCollideDistance && isInsideTriangle;

                            if (isColliding)
                            {
                                CollideWithPlaneB(na, pb, nOne, collidePoint, collideDistance);
                            }
                        }

                        interactionCounter++;
                    }
                }

            if (a.Options.UseSelfInteractions)
            {
                // todo
            }    
        }

        ApplyPointCollideValues();

        //Debug.WriteLine(interactionCounter);
    }

    private void ApplyPointCollideValues()
    {
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
                    }
                    else
                    {
                        n.nRejectionDir = Vector3.Origin;
                    }

                    if (n.collideCount > 0)
                    {
                        n.speed += n.collideForce / n.collideCount;
                        n.collideCount = 0;
                        n.collideForce = Vector3.Origin;
                    }

                    n.isInsideMaterial = n.isColliding;
                    n.isColliding = false;
                }));
    }

    //private static decimal maxPercent = 0;
    private void CollideWithPlaneB(Interactor rra, Interactor rrb, Vector3 bnOne, Vector3 collidePoint, double forceDistance)
    {
        rra.isColliding = true;

        if (!rra.isInsideMaterial)
            rra.rejectionDirSum += bnOne;

        var bnDir = rra.nRejectionDir == Vector3.Origin ? bnOne : rra.nRejectionDir;
        var elasticForce = GetPlaneElasticForce(bnDir, bnOne, forceDistance);

        var aPack = rra.pointPackFn(collidePoint);
        var planePack = rrb.pointPackFn(collidePoint);
        var collideMass = 0.5 * (aPack.mass + planePack.mass);
        var elasticSpeedUp = (collideMass / aPack.mass) * elasticForce;

        var interSpeed = aPack.speed - planePack.speed + elasticSpeedUp;
        var interSpeedN = interSpeed.MultS(bnDir);
        var slidingSpeed = interSpeed - bnDir * interSpeedN;

        var frictionForce = GetPlaneFrictionForce(slidingSpeed);
        var clingForce = GetPlaneClingForce(interSpeedN, bnDir);
        var collideForce = elasticForce + frictionForce + clingForce;

        if (options.Debug.DebugPlaneMaterialPenetration)
            Debugs.MaxPercent("Plane material penetration: {0}", forceDistance / (options.JediMaterialThickness + options.MaterialThickness));

        //Debug.WriteLine($"{cc++} {-forceDistance / (Options.JediMaterialThickness + Options.MaterialThickness):P0}: {collideForce.Length} ({elasticForce.Length}, {frictionForce.Length}, {clingForce.Length}) ");

        rra.applyCollideForce(aPack, collideMass, collideForce);
        rrb.applyCollideForce(planePack, collideMass, -collideForce);
    }
    private void EdgeWithPlaneInteraction()
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
                    var pIsPointInsideFn = pb.IsInsideFn(nOne);
                    var pLineCrossFn = plane.IntersectionFn;
                    var pEdgeNearPointsFn = pb.EdgeNearPointsFn;

                    var aNearNodes = a.Model.net.SelectItemsByRadius(pCenter - a.Model.center, pSize);
                    
                    foreach (var na in aNearNodes)
                    {
                        var isStrikeDir = !na.hasnDir || na.nDir.MultS(nOne) < 0;

                        if (isStrikeDir)
                        {
                            var collidePoint = pProjFn(na.position);
                            var collideDistance = pDistanceFn(na.position);
                            var isAtCollideDistance = IsAtCollideDistance(collideDistance);
                            var isInsideConvex = pIsPointInsideFn(collidePoint, collideDistance); // todo: collideDistance - катет
                            var isColliding = isAtCollideDistance && isInsideConvex;

                            if (isColliding)
                            {
                                CollideWithPlaneB(na, pb, nOne, collidePoint, collideDistance);
                            }
                        }

                        interactionCounter++;
                    }

                    var aNoCollideNodes = aNearNodes.Where(na => na.collideCount == 0).ToHashSet();

                    foreach(var ea in aNoCollideNodes.SelectMany(na=>na.edges.Where(ea=> aNoCollideNodes.Contains(ea.nj))))
                    {
                        var (hasCrossPoint, crossPoint) = pLineCrossFn(ea.positionI, ea.positionJ).SplitNullable();
                        
                        if (!hasCrossPoint)
                            continue;

                        var isInsideCollideConvex = pIsPointInsideFn(crossPoint, 0);
                        
                        if (isInsideCollideConvex)
                        {
                            var distanceI = pDistanceFn(ea.positionI);
                            var distanceJ = pDistanceFn(ea.positionJ);
                            var downPoint = distanceI < distanceJ ? ea.positionI : ea.positionJ;
                            var edgePoint = pEdgeNearPointsFn(crossPoint).MinBy(p => p.Length2);
                            
                            var (hasV, vCollidePoint) = (downPoint - crossPoint).ProjWithCheck(edgePoint - crossPoint);

                            if (hasV)
                            {
                                var collidePoint = crossPoint + vCollidePoint;
                                var vProjPoint = (edgePoint - crossPoint).Proj(vCollidePoint);
                                var collideDistance = -Math.Sqrt(vCollidePoint.Length2 - vProjPoint.Length2);

                                var isInsideConvex = pIsPointInsideFn(crossPoint, collideDistance);

                                if (isInsideConvex)
                                    CollideWithPlaneB(ea, pb, nOne, collidePoint, collideDistance);
                            }
                        }
                    }
                }

            if (a.Options.UseSelfInteractions)
            {
                // todo
            }
        }

        ApplyPointCollideValues();

        //Debug.WriteLine(interactionCounter);
    }

    //private void EdgeWithPlaneInteraction1()
    //{
    //    UpdateCollidePositions();

    //    var interactionCounter = 0;

    //    foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
    //    {
    //        var materialThickness = Options.MaterialThickness + Options.JediMaterialThickness;

    //        foreach (var b in worldNet.SelectNeighbors(a))
    //            foreach (var pb in b.Planes)
    //            {
    //                var plane = pb.collidePlane;
    //                var nOne = plane.NOne;
    //                var pCenter = plane.Center;
    //                var pSize = plane.Size;
    //                var pDistanceFn = plane.Fn;
    //                var pProjFn = plane.ProjectionFn;
    //                var pIsPointInsideFn = pb.IsInsideFn(nOne);
    //                var pLineCrossFn = plane.IntersectionFn;

    //                foreach (var ea in a.Model.net.SelectItemsByRadius(pCenter - a.Model.center, pSize).SelectMany(n => n.edges))
    //                {
    //                    var isStrikeDirI = /*!ea.ni.hasnDir || */ea.ni.acc.MultS(nOne) < 0;
    //                    var isStrikeDirJ = /*!ea.nj.hasnDir || */ea.nj.acc.MultS(nOne) < 0;

    //                    if (isStrikeDirI || isStrikeDirJ)
    //                    {
    //                        var distanceI = pDistanceFn(ea.positionI);
    //                        var distanceJ = pDistanceFn(ea.positionJ);
    //                        var isAtCollideDistanceI = IsAtCollideDistance(distanceI);
    //                        var isAtCollideDistanceJ = IsAtCollideDistance(distanceJ);
    //                        var isInsideTriangleI = pIsPointInsideFn(ea.positionI, distanceI);
    //                        var isInsideTriangleJ = pIsPointInsideFn(ea.positionJ, distanceJ);
    //                        var isCollidingI = isStrikeDirI && isAtCollideDistanceI && isInsideTriangleI;
    //                        var isCollidingJ = isStrikeDirJ && isAtCollideDistanceJ && isInsideTriangleJ;

    //                        if (isCollidingI || isCollidingJ)
    //                        {
    //                            var forceDistance = 0.25 * ((isCollidingI ? distanceI : 0) + (isCollidingJ ? distanceJ : 0));
    //                            var (hasCrossPoint, crossPoint) = pLineCrossFn(ea.positionI, ea.positionJ).SplitNullable();

    //                            if (hasCrossPoint)
    //                            {
    //                                var crossPointIsInsideTriangle = pIsPointInsideFn(crossPoint, forceDistance);

    //                                if (crossPointIsInsideTriangle)
    //                                {
    //                                    //Debug.WriteLine(1);
    //                                    CollideWithPlaneB(ea, pb, nOne, crossPoint, forceDistance);
    //                                }
    //                                else
    //                                {
    //                                    //Debug.WriteLine(2);
    //                                    CollideWithPlaneB(ea, pb, nOne, crossPoint, forceDistance);
    //                                    //var kI = distanceI / (distanceI + distanceJ);
    //                                    //var kJ = distanceJ / (distanceI + distanceJ);
    //                                    //CollideWithPlaneB(ea, pb, nOne, kI * ea.positionI + kJ * ea.positionJ, forceDistance);
    //                                }
    //                            }
    //                            else // edge is parallel to plane
    //                            {
    //                                Debug.WriteLine(3);
    //                                CollideWithPlaneB(ea, pb, nOne, ea.positionCenter, forceDistance);
    //                            }
    //                        }
    //                    }

    //                    interactionCounter++;
    //                }
    //            }

    //        if (a.Options.UseSelfInteractions)
    //        {
    //            // todo
    //        }
    //    }

    //    ApplyEdgeCollideValues();

    //    //Debug.WriteLine(interactionCounter);
    //}

    //private void ApplyEdgeCollideValues()
    //{
    //    activeShapes.Where(a => a.Options.UseInteractions)
    //        .ForEach(a => a.Nodes
    //            .ForEach(n =>
    //            {
    //                var isColliding = false;

    //                n.edges.ForEach(e =>
    //                {
    //                    isColliding = isColliding || e.isColliding;

    //                    if (e.isColliding)
    //                    {
    //                        if (!e.isInsideMaterial) // is crossing
    //                        {
    //                            e.nRejectionDir = e.rejectionDirSum.Normalize();
    //                            e.rejectionDirSum = Vector3.Origin;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        e.nRejectionDir = Vector3.Origin;
    //                    }

    //                    e.isInsideMaterial = n.isColliding;
    //                    e.isColliding = false;
    //                });

    //                if (isColliding)
    //                {
    //                    n.acc += n.collideForce / n.collideCount;
    //                }

    //                n.collideCount = 0;
    //                n.collideForce = Vector3.Origin;
    //            }));
    //}
}
