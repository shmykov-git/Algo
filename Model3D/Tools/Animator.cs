using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
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
        public double LiquidPower;
        public double ParticlePlaneThikness = 2;
        public double MaxParticleMove = 2.5;

        public Vector3? NetFrom;
        public Vector3? NetTo;
        public double? NetSize;
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
        public Vector3 Normal { get; set; }
    }

    public class Animator
    {
        #region ctor

        private readonly AnimatorOptions options;
        private Particle[] particles;
        private Plane[] planes;
        private Net3<NetItem> net;
        private double dMin;
        private double dMin2;
        private double zeroDist2;
        private double planeThikness2;
        private Vector3 zeroV3 = new Vector3(0, 0, 0);

        public Animator(AnimatorOptions options)
        {
            this.options = options;
        }

        #endregion

        public Vector3[] NetField => net?.NetField;
        public Vector3[] NetParticles => net?.NetItems.OfType<Particle>().Select(p => p.Item.Position).ToArray();
        public Vector3[] NetPlanes => net?.NetItems.OfType<Plane>().Select(p => p.Position).ToArray();

        public void AddItems(IAnimatorParticleItem[] items)
        {
            var offset = particles?.Length ?? 0;

            var newParticles = items.Select((p, i) => new Particle
            {
                i = i + offset,
                Item = p,
                ItemBase = p,
                PositionFn = () => p.Position,
            }).ToArray();

            particles = particles == null ? newParticles : particles.Concat(newParticles).ToArray();

            if (options.NetSize.HasValue)
            {
                if (net == null)
                {
                    net = options.NetFrom.HasValue && options.NetTo.HasValue
                        ? new Net3<NetItem>(particles, options.NetFrom.Value, options.NetTo.Value,
                            options.NetSize.Value)
                        : new Net3<NetItem>(particles, options.NetSize.Value);
                }
                else
                {
                    net.AddItems(newParticles);
                }
            }
        }

        public void AddPlanes(IAnimatorPlaneItem[] planes)
        {
            if (this.planes != null)
                throw new ArgumentException("Cannot init planes twice");

            if (net != null)
            {
                var fieldDistance = options.NetSize;

                var newPlanes = planes.SelectMany(pl =>
                {
                    var plane = new Model3D.Plane(pl.Convex[0], pl.Convex[1], pl.Convex[2]);
                    var distanceFn = plane.Fn;

                    return net.NetField
                        .Where(netPos => distanceFn(netPos).Abs() < fieldDistance)
                        .Where(netPos => pl.Convex.IsInside(netPos))
                        .Select(netPos => new Plane
                        {
                            Item = pl,
                            ItemBase = pl,
                            Normal = plane.NOne,
                            Position = netPos,
                            PositionFn = () => netPos,
                            ProjectionFn = plane.ProjectionFn
                        });
                }).Select((p,i) =>
                {
                    p.i = i;

                    return p;
                }).ToArray();

                this.planes = newPlanes;

                net.AddItems(newPlanes);
            }
        }

        private void Calculations()
        {
            dMin = 2 * options.ParticleRadius;
            dMin2 = dMin.Pow2();
            zeroDist2 = dMin2 * options.InteractionFactor.Pow2();
            planeThikness2 = (options.ParticlePlaneThikness * 2 * options.ParticleRadius).Pow2();
        }

        IEnumerable<NetItem> GetNeighbors(Vector3 position) => net == null ? particles : net.SelectNeighbors(position);

        Vector3 GetAttractionAcceleration(Vector3 a, Vector3 b) =>
            (b - a).ToLen(options.GravityAttractionPower / (b - a).Length2);

        Vector3 GetGravityAcceleration() => options.GravityPower * options.GravityDirection;

        Vector3 GetLiquidAcceleration(IAnimatorParticleItem aP, IAnimatorItem bP, Func<Vector3> bPositionFn = null)
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
            else
            {
                // repulsion power

                return zeroV3;
            }
        }

        //Vector3 GetLiquidAcceleration(IAnimatorParticleItem a, NetItem b)
        //{
        //    switch (b)
        //    {
        //        case Particle bParticle:
        //            return GetLiquidAcceleration(a, bParticle.Item);
                
        //        case Plane bPlane:
        //            return GetLiquidAcceleration(a, bPlane.ItemBase, bPlane.PositionFn);

        //        default:
        //            throw new NotImplementedException();
        //    }
        //}

        public void Animate(int count)
        {
            Calculations();
            (count).ForEach(Step);
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
                var attractionAccelerations = particles.Select(a =>
                        GetNeighbors(a.Item.Position)
                            .Where(b => a != b)
                            .Select(b => GetAttractionAcceleration(a.Item.Position, b.PositionFn())).Sum())
                    .ToArray();
                
                attractionAccelerations.ForEach((a, i) => particles[i].StepState.Acceleration += a);
            }

            // calculate gravity accelerations
            if (options.UseGravity)
            {
                var gravityAccelerations = particles.Select(_ => GetGravityAcceleration()).ToArray();
                gravityAccelerations.ForEach((a, i) => particles[i].StepState.Acceleration += a);
            }

            // calculate liquid accelerations
            if (options.UseParticleLiquidAcceleration)
            {
                // particle attract to particle
                var particleLiquidAccelerations = particles.Select(a =>
                        GetNeighbors(a.Item.Position)
                        .OfType<Particle>()
                        .Where(b => a != b)
                        .Select(b => GetLiquidAcceleration(a.Item, b.Item)).Sum())
                    .ToArray();

                particleLiquidAccelerations.ForEach((a, i) => particles[i].StepState.Acceleration += a);

                // particle attract to plane
                var planeLiquidAccelerations = particles.Select(a =>
                    GetNeighbors(a.Item.Position)
                        .OfType<Plane>()
                        .GroupBy(p => p.Item)
                        .Select(gp => gp.First())
                        .Select(b => GetLiquidAcceleration(a.Item, b.Item, () => b.ProjectionFn(a.Item.Position)))
                        .Sum()).ToArray();

                planeLiquidAccelerations.ForEach((a, i) => particles[i].StepState.Acceleration += a);

                //Debug.WriteLine($"{minDist}, {liquidAccelerations.Min(v=>v.Length)}, {liquidAccelerations.Max(v => v.Length)}");
            }

            foreach (var p in particles)
            {
                p.Item.Speed += p.StepState.Acceleration;
                p.Item.Position += p.Item.Speed;
            }

            if (options.UseParticleLiquidAcceleration)
            {
                // particle with particle collisions

                var particleCollisions = particles.Select(a => (particle: a, infos:
                        GetNeighbors(a.Item.Position)
                            .OfType<Particle>()
                            .Where(b => a != b)
                            .Select(b => (b, ab: b.Item.Position - a.Item.Position, speed:b.ItemBase.Speed))
                            .Where(v => v.ab.Length2 < dMin2)
                            .Select(v => (move: -v.ab.ToLen(dMin - v.ab.Length), v.speed))
                            .ToArray()))
                    .Where(v => v.infos.Length > 0)
                    .ToArray();

                foreach (var (particle, infos) in particleCollisions)
                {
                    var speed = infos.Select(v => v.speed).Concat(new []{ particle.Item.Speed }).Center();
                    particle.Item.Speed = speed;

                    var move = infos.Select(v => v.move).Sum();

                    if (move.Length2 > (options.MaxParticleMove * options.ParticleRadius).Pow2())
                        move = move.ToLen(options.MaxParticleMove * options.ParticleRadius);

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
                }

                // particle with plane collisions

                var planeCollisions = particles.Select(a => (particle: a, infos:
                        GetNeighbors(a.Item.Position)
                            .OfType<Plane>()
                            .GroupBy(p=>p.Item)
                            .Select(gp=>gp.First())
                            .Select(b => (b, ab: b.ProjectionFn(a.Item.Position) - a.Item.Position))
                            .Select(v => (v.b, v.ab, closing: v.b.Normal.MultS(v.ab) < 0, plane: v.b))
                            .Where(v => v.closing ? v.ab.Length2 < dMin2 : v.ab.Length2 < planeThikness2)
                            .Where(v => v.b.Item.Convex.IsInside(a.Item.Position))
                            //.Select(v => (v.b, move: -v.ab.ToLen(dMin - v.ab.Length)))
                            .Select(v => (move: v.closing ? -v.ab.ToLen(dMin - v.ab.Length) : v.ab.ToLen(v.ab.Length + dMin), v.plane))
                            .ToArray()))
                    .Where(v => v.infos.Length > 0)
                    .ToArray();

                foreach (var (particle, infos) in planeCollisions)
                {
                    var move = infos.Select(info => info.move).Sum();
                    
                    if (move.Length2 > (options.MaxParticleMove * options.ParticleRadius).Pow2())
                        move = move.ToLen(options.MaxParticleMove * options.ParticleRadius);

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
                }
            }

            //Debug.WriteLine($"{particles[0].Item.Position}, {particles[0].Item.Speed}");

            net?.Update();
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
