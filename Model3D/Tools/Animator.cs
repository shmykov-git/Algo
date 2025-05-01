using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model3D.AsposeModel;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;

namespace Model3D.Tools
{
    public class AnimatorOptions
    {
        public bool UseParticleGravityAttraction;
        public double GravityAttractionPower;

        public bool UseGravity;
        public double GravityPower;
        public Vector3 GravityDirection = new Vector3(0, -1, 0);

        public bool UseParticleLiquidAcceleration;
        public double ParticleRadius;
        public double InteractionFactor = Math.E;
        public double FrictionFactor = 0.5;
        public double LiquidPower;
        public double ParticlePlaneThikness = 4;
        public double ParticleMaxMove = 2.5;

        public Vector3? NetFrom;
        public Vector3? NetTo;
        public double? NetSize;

        public int? StepDebugNotify = 50;
    }

    public interface IAnimatorItem
    {
        public Vector3 Position { get; set; }
        public Vector3 Speed { get; set; }
    }

    public interface IAnimatorParticleItem : IAnimatorItem
    {
    }

    public interface IAnimatorPlaneItem : IAnimatorItem
    {
        public Vector3[] Convex { get; set; }
        public double ForwardThickness { get; set; }
    }

    public class Animator
    {
        #region ctor

        private readonly AnimatorOptions options;
        private Particle[] particles = new Particle[0];
        private Plane[] planes;
        private Net3<NetItem> net;
        private double dParticleMin;
        private double dParticleMin2;
        private double zeroParticleDist2;
        private double planeBackwardThickness2;
        private Vector3 zeroV3 = new Vector3(0, 0, 0);
        private int stepNumber = 0;
        private Stopwatch? sw;

        public Animator(AnimatorOptions options)
        {
            this.options = options;
        }

        #endregion

        public Vector3[] NetField => net?.NetField;
        public Vector3[] NetParticles => net?.NetItems.OfType<Particle>().Select(p => p.Item.Position).ToArray();
        public Vector3[] NetPlanes => net?.NetItems.OfType<Plane>().Select(p => p.Position).ToArray();
        
        private bool HasNet => options.NetSize.HasValue && options.NetTo.HasValue && options.NetFrom.HasValue;

        private void InitNet()
        {
            if (net != null || !HasNet)
                return;

            net = new Net3<NetItem>(options.NetFrom.Value, options.NetTo.Value, options.NetSize.Value);
        }

        public void AddItems(IAnimatorParticleItem[] items)
        {
            InitNet();

            var offset = particles.Length;

            var newParticles = items.Select((p, i) => new Particle
            {
                i = i + offset,
                Item = p,
                ItemBase = p,
                PositionFn = () => p.Position,
            }).ToArray();

            particles = particles.Concat(newParticles).ToArray();

            if (net != null)
                net.AddItems(newParticles);
        }

        public void AddPlanes(IAnimatorPlaneItem[] planes)
        {
            if (this.planes != null)
                throw new ArgumentException("Cannot init planes twice");

            InitNet();

            var netCheckPoints = Shapes.Icosahedron.Perfecto(options.NetSize.Value).Planes.Select(p => p.Center())
                .ToArray();

            var fieldDistance = options.NetSize;
            var k = Math.Sqrt(3) / 4;

            var newPlanes =
                planes.SelectInParallel(pl =>
                {
                    var planeConvex = pl.Convex;
                    var convexCenter = planeConvex.Center();
                    var convexRadius = planeConvex.Max(v => (convexCenter - v).Length);
                    var farFieldDistance2 = (k * options.NetSize.Value + convexRadius).Pow2();
                    var plane = new Model3D.Plane(planeConvex[0], planeConvex[1], planeConvex[2]);
                    var distanceFn = plane.Fn;

                    bool IsNetIntersected(Vector3 p)
                    {
                        if (planeConvex.IsInside(p))
                            return true;

                        if (netCheckPoints.Any(v => planeConvex.IsInside(v + p)))
                            return true;

                        return false;
                    }

                    return net.NetField
                        .Where(p => (convexCenter - p).Length2 < farFieldDistance2)
                        .Where(p => distanceFn(p).Abs() < fieldDistance)
                        .Where(IsNetIntersected)
                        .Select(netPos => new Plane
                        {
                            Item = pl,
                            ItemBase = pl,
                            Normal = plane.NOne,
                            Coeffs = GetLiquidCoeffs(options.ParticleRadius, pl.ForwardThickness),
                            Position = netPos,
                            PositionFn = () => netPos,
                            ProjectionFn = plane.ProjectionFn
                        }).ToArray();
                }).SelectMany(v => v).Select((p, i) =>
                {
                    p.i = i;

                    return p;
                }).ToArray();

            this.planes = newPlanes;

            if (net != null)
                net.AddItems(newPlanes);
        }

        (double dMin, double dMin2, double zeroDist2) GetLiquidCoeffs(double aRadius, double bRadius)
        {
            var dist = aRadius + bRadius;
            var dist2 = dist.Pow2();
            var zeroDist2 = dist2 * options.InteractionFactor.Pow2();

            return (dist, dist2, zeroDist2);
        }

        private void Calculations()
        {
            (dParticleMin, dParticleMin2, zeroParticleDist2) =
                GetLiquidCoeffs(options.ParticleRadius, options.ParticleRadius);

            planeBackwardThickness2 = (options.ParticlePlaneThikness * options.ParticleRadius).Pow2();
        }

        IEnumerable<NetItem> GetNeighbors(Vector3 position) => net == null ? particles : net.SelectNeighbors(position);

        Vector3 GetAttractionAcceleration(Vector3 a, Vector3 b) =>
            (b - a).ToLen(options.GravityAttractionPower / (b - a).Length2);

        Vector3 GetGravityAcceleration() => options.GravityPower * options.GravityDirection;

        Vector3 GetLiquidPlaneAcceleration(IAnimatorParticleItem aP, Plane bP)
        {
            var (_, dPlaneMin2, zeroPlaneDist2) = GetLiquidCoeffs(options.ParticleRadius, bP.Item.ForwardThickness);
            var particleAcceleration = GetLiquidAcceleration(dPlaneMin2, zeroPlaneDist2, aP, bP.Item, () => bP.ProjectionFn(aP.Position));

            var zeroPr = bP.ProjectionFn(zeroV3);
            var frictionAcceleration =
                (zeroPr - bP.ProjectionFn(aP.Speed)).ToLenWithCheck(options.FrictionFactor * particleAcceleration.Length);

            // todo: gravity compensation

            return particleAcceleration + frictionAcceleration;
        }

        Vector3 GetLiquidAcceleration(double dMin2, double zeroDist2, IAnimatorParticleItem aP, IAnimatorItem bP, Func<Vector3> bPositionFn = null)
        {
            var a = aP.Position;
            var b = bPositionFn?.Invoke() ?? bP.Position;

            var d2 = (b - a).Length2;

            if (d2 > zeroDist2)
                return zeroV3;

            if (dMin2 < d2)
            {
                // attraction power

                var l1 = dMin2 / d2; // skip .Pow3()
                var l2 = l1.Pow2();

                var acc = (b - a).ToLen(options.LiquidPower * (l1 - l2));

                return acc;
            }
            
            // no repulsion power
            return zeroV3;
        }

        public void Animate(int count)
        {
            sw ??= Stopwatch.StartNew();

            Calculations();
            (count).ForEach(i => Step(stepNumber + i));
            stepNumber += count;
        }

        private void Step(int number)
        {
            particles.ForEach(p =>
                p.StepState = new ParticleStepState()
                {
                    Acceleration = zeroV3,
                });

            // calculate attraction accelerations
            if (options.UseParticleGravityAttraction)
            {
                var attractionAccelerations = particles.SelectInParallel(a =>
                        GetNeighbors(a.Item.Position)
                            .Where(b => a != b)
                            .Select(b => GetAttractionAcceleration(a.Item.Position, b.PositionFn())).Sum());
                
                attractionAccelerations.ForEach((a, i) => particles[i].StepState.Acceleration += a);
            }

            // calculate gravity accelerations
            if (options.UseGravity)
            {
                var a = GetGravityAcceleration();
                particles.ForEach(p => p.StepState.Acceleration += a);
            }

            // calculate liquid accelerations
            if (options.UseParticleLiquidAcceleration)
            {
                // particle attract to particle
                var particleLiquidAccelerations = particles.SelectInParallel(a =>
                        GetNeighbors(a.Item.Position)
                            .OfType<Particle>()
                            .Where(b => a != b)
                            .Select(b => GetLiquidAcceleration(dParticleMin2, zeroParticleDist2, a.Item, b.Item)).Sum());

                particleLiquidAccelerations.ForEach((a, i) => particles[i].StepState.Acceleration += a);

                // particle attract to plane
                var planeLiquidAccelerations = particles.SelectInParallel(a =>
                    GetNeighbors(a.Item.Position)
                        .OfType<Plane>()
                        .GroupBy(p => p.Item)
                        .Select(gp => gp.First())
                        .Select(b => GetLiquidPlaneAcceleration(a.Item, b))
                        .Sum());

                planeLiquidAccelerations.ForEach((a, i) => particles[i].StepState.Acceleration += a);

                //Debug.WriteLine($"{minDist}, {liquidAccelerations.Min(voxel=>voxel.Length)}, {liquidAccelerations.Max(voxel => voxel.Length)}");
            }

            foreach (var p in particles)
            {
                p.Item.Speed += p.StepState.Acceleration;
                p.Item.Position += p.Item.Speed;
            }

            if (options.UseParticleLiquidAcceleration)
            {
                // particle with particle collisions

                var particleCollisions = particles.SelectInParallel(a => (particle: a, infos:
                        GetNeighbors(a.Item.Position)
                            .OfType<Particle>()
                            .Where(b => a != b)
                            .Select(b => (b, ab: b.Item.Position - a.Item.Position, speed:b.ItemBase.Speed))
                            .Where(v => v.ab.Length2 < dParticleMin2)
                            .Select(v => (move: -v.ab.ToLen(dParticleMin - v.ab.Length), v.speed))
                            .ToArray()))
                    .Where(v => v.infos.Length > 0)
                    .ToArray();

                particleCollisions.ForEach(collision =>
                {
                    var (particle, infos) = collision;

                    var speed = infos.Select(v => v.speed).Concat(new[] { particle.Item.Speed }).Center();
                    particle.Item.Speed = speed;

                    var move = infos.Select(v => v.move).Sum();

                    if (move.Length2 > (options.ParticleMaxMove * options.ParticleRadius).Pow2())
                        move = move.ToLen(options.ParticleMaxMove * options.ParticleRadius);

                    // direction immunity compensation
                    // todo: safe particle impulse
                    foreach (var plane in particle.DirectionImmunities.Keys.ToArray())
                    {
                        var directionImmunity = particle.DirectionImmunities[plane];

                        var immunityLen = directionImmunity.MultS(move);

                        if (immunityLen < 0)
                        {
                            move -= immunityLen * directionImmunity;
                        }
                        else
                        {
                            particle.DirectionImmunities.Remove(plane);
                        }
                    }

                    particle.Item.Position += move;
                });


                // particle with plane collisions

                var planeCollisions = particles.SelectInParallel(a => (particle: a, infos:
                        GetNeighbors(a.Item.Position)
                            .OfType<Plane>()
                            .GroupBy(p=>p.Item)
                            .Select(gp=>gp.First())
                            .Select(b => (b, ab: b.ProjectionFn(a.Item.Position) - a.Item.Position))
                            .Select(v => (v.b, v.ab, closing: v.b.Normal.MultS(v.ab) < 0, plane: v.b))
                            .Where(v => v.closing ? v.ab.Length2 < v.b.Coeffs.dMin2 : v.ab.Length2 < planeBackwardThickness2)
                            .Where(v => v.b.Item.Convex.IsInside(a.Item.Position))
                            .Select(v => (move: v.closing ? -v.ab.ToLen(v.b.Coeffs.dMin - v.ab.Length) : v.ab.ToLen(v.ab.Length + v.b.Coeffs.dMin), v.plane))
                            .ToArray()))
                    .Where(v => v.infos.Length > 0)
                    .ToArray();

                planeCollisions.ForEach(collision =>
                {
                    var (particle, infos) = collision;

                    var move = infos.Select(info => info.move).Sum();

                    if (move.Length2 > (options.ParticleMaxMove * options.ParticleRadius).Pow2())
                        move = move.ToLen(options.ParticleMaxMove * options.ParticleRadius);

                    var nMove = move.Normalize();
                    particle.Item.Speed -= 2 * nMove * nMove.MultS(particle.Item.Speed);

                    particle.Item.Position += move;

                    foreach (var info in infos)
                    {
                        if (!particle.DirectionImmunities.ContainsKey(info.plane))
                            particle.DirectionImmunities[info.plane] = info.plane.Normal;
                    }

                    foreach (var plane in particle.DirectionImmunities.Keys.Except(infos.Select(info => info.plane)).ToArray())
                    {
                        particle.DirectionImmunities.Remove(plane);
                    }
                });
            }

            net?.Update();

            if (options.StepDebugNotify.HasValue && (number + 1) % options.StepDebugNotify.Value == 0 && sw != null)
            {
                Debug.WriteLine($"Step {number + 1}: {sw.Elapsed}");
                sw.Restart();
            }
        }

        class NetItem : INet3Item
        {
            public IAnimatorItem ItemBase;
            public Func<Vector3> PositionFn { get; set; }
        }

        class Plane : NetItem
        {
            public int i;
            public Vector3 Position;
            public Vector3 Normal;
            public (double dMin, double dMin2, double zeroDist2) Coeffs;
            public IAnimatorPlaneItem Item;
            public Func<Vector3, Vector3> ProjectionFn;
        }

        class Particle : NetItem
        {
            public int i;
            public IAnimatorParticleItem Item;
            public ParticleStepState StepState;
            public Dictionary<Plane, Vector3> DirectionImmunities = new Dictionary<Plane, Vector3>();
            public override string ToString() => $"{Item.Position}, {Item.Speed}, {StepState.Acceleration}";
        }

        struct ParticleStepState
        {
            public Vector3 Acceleration;
        }
    }
}
