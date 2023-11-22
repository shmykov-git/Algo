using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;

namespace Model3D.Actives;

public partial class ActiveWorld // Interaction
{

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

    private void ParticleWithPlaneInteraction()
    {
        var interactionCounter = 0;

        foreach (var a in activeShapes.Where(a => a.Options.UseInteractions))
        {
            foreach (var b in worldNet.SelectNeighbors(a))
                foreach (var pb in b.Planes)
                {
                    var nbi = b.Nodes[pb.i];
                    var nbj = b.Nodes[pb.j];
                    var nbk = b.Nodes[pb.k];

                    var plane = new Model3D.Plane(nbi.collidePosition, nbj.collidePosition, nbk.collidePosition);
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
                        var isAtCollideDistance = -a.Options.MaterialThickness <= triangleDistance && triangleDistance <= 0;
                        var isInsideTriangle = isPointInsideFn(interPoint);
                        var isColliding = isAtCollideDistance && isInsideTriangle;

                        na.isColliding = na.isColliding || isColliding;

                        if (isColliding)
                        {
                            var forceDistance = -triangleDistance / a.Options.MaterialThickness;
                            var pK = GetPointK(b.Nodes, pb, interPoint);
                            var interPointSpeed = GetPointSpeed(b.Nodes, pb, pK);
                            var interSpeed = na.speed - interPointSpeed;
                            var interSpeedN = interSpeed.MultS(nOne);
                            var slidingSpeed = interSpeed - nOne * interSpeedN;
                            var rejectionSpeed = interSpeed - 2 * interSpeedN * nOne;

                            if (!na.isInsideMaterial)
                                na.rejectionDirSum += nOne;

                            var frictionForce = slidingSpeed.ToLenWithCheck(-Math.Min(slidingSpeed.Length, options.PlaneConst * options.Interaction.MaterialFrictionForce));
                            var clingForce = (-options.PlaneConst * options.Interaction.MaterialClingForce) * nOne;

                            var dirFactor = na.nRejectionDir.MultS(nOne);

                            var elasticForce = dirFactor > 0
                                ? (dirFactor * forceDistance * options.PlaneConst * options.Interaction.PlaneForce) * na.nRejectionDir
                                : Vector3.Origin;

                            var force = elasticForce + frictionForce + clingForce;

                            var mass = 0.25 * (na.mass + pK.x * nbi.mass + pK.y * nbj.mass + pK.z * nbk.mass); // 0.5 (both side calculation) * middle interaction mass

                            na.collideForce += (mass / na.mass) * force;
                            na.collideCount++;
                            nbi.collideForce -= (mass * pK.x / nbi.mass) * force;
                            nbi.collideCount++;
                            nbj.collideForce -= (mass * pK.y / nbj.mass) * force;
                            nbj.collideCount++;
                            nbk.collideForce -= (mass * pK.z / nbk.mass) * force;
                            nbk.collideCount++;
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

}
