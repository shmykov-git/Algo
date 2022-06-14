using System;
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
        private Vector3 zeroV3 = new Vector3(0, 0, 0);

        public Animator(AnimatorOptions options)
        {
            this.options = options;
        }

        #endregion

        public Vector3[] NetField => net?.NetField;

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
                var fieldDistance = options.NetSize / 2;

                var newPlanes = planes.SelectMany(pl =>
                {
                    var plane = new Model3D.Plane(pl.Convex[0], pl.Convex[1], pl.Convex[2]);
                    var distanceFn = plane.Fn;
                    var projectionFn = plane.ProjectionFn;

                    return net.NetField
                        .Where(pos => distanceFn(pos).Abs() < fieldDistance)
                        .Where(pos => IsInside(pl.Convex, projectionFn(pos))) // todo: check
                        .Select(pos => new Plane
                        {
                            Item = pl, 
                            ItemBase = pl,
                            Position = pos,
                            ProjectionFn = plane.ProjectionFn
                        });
                }).Select((p,i) =>
                {
                    p.i = i;
                    return p;
                }).ToArray();

                this.planes = newPlanes;
            }
        }

        private void Calculations()
        {
            dMin = 2 * options.ParticleRadius;
            dMin2 = dMin.Pow2();
            zeroDist2 = dMin2 * options.InteractionFactor.Pow2();
        }

        IEnumerable<NetItem> GetNeighbors(Vector3 position) => net == null ? particles : net.SelectNeighbors(position);

        bool IsInside(Vector3[] convex, Vector3 pos)
        {
            return convex.SelectCirclePair((a, b) => new Line3(a, b)).All(line => line.IsLeft(pos));
        }

        Vector3 GetAttractionAcceleration(Vector3 a, Vector3 b) =>
            (b - a).ToLen(options.GravityAttractionPower / (b - a).Length2);

        Vector3 GetGravityAcceleration() => options.GravityPower * options.GravityDirection;

        Vector3 GetLiquidAcceleration(IAnimatorParticleItem aP, Vector3 b)
        {
            var a = aP.Position;

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

            return zeroV3;
        }

        Vector3 GetLiquidAcceleration(IAnimatorParticleItem a, INet3Item b)
        {
            switch (b)
            {
                case Particle bParticle:
                    return GetLiquidAcceleration(a, bParticle.Item.Position);
                
                case Plane bPlane:
                    return GetLiquidAcceleration(a, bPlane.ProjectionFn(b.PositionFn()));

                default:
                    throw new NotImplementedException();
            }
        }

        public void Animate(int count)
        {
            Calculations();
            (count).ForEach(Step);
        }
        
        private void Step(int number)
        {
            particles.ForEach(p =>
            {
                p.Acceleration = zeroV3;
            });

            // calculate attraction accelerations
            if (options.UseParticleGravityAttraction)
            {
                var attractionAccelerations = particles.Select(a =>
                        GetNeighbors(a.Item.Position)
                            .Where(b => a != b)
                            .Select(b => GetAttractionAcceleration(a.Item.Position, b.PositionFn())).Sum())
                    .ToArray();
                
                attractionAccelerations.ForEach((a, i) => particles[i].Acceleration += a);
            }

            // calculate gravity accelerations
            if (options.UseGravity)
            {
                var gravityAccelerations = particles.Select(_ => GetGravityAcceleration()).ToArray();
                gravityAccelerations.ForEach((a, i) => particles[i].Acceleration += a);
            }

            // calculate liquid accelerations
            if (options.UseParticleLiquidAcceleration)
            {
                var liquidAccelerations = particles.Select(a =>
                        GetNeighbors(a.Item.Position)
                        .Where(b => a != b)
                        .Select(b => GetLiquidAcceleration(a.Item, b)).Sum())
                    .ToArray();

                liquidAccelerations.ForEach((a, i) => particles[i].Acceleration += a);

                //Debug.WriteLine($"{minDist}, {liquidAccelerations.Min(v=>v.Length)}, {liquidAccelerations.Max(v => v.Length)}");
            }

            foreach (var p in particles)
            {
                p.Item.Speed += p.Acceleration;
                p.Item.Position += p.Item.Speed;
            }

            if (options.UseParticleLiquidAcceleration)
            {
                // particle collisions

                var offsets = particles.Select(a => (particle: a, infos:
                        GetNeighbors(a.Item.Position)
                            .Where(b => a != b)
                            .Select(b => (b, ab: b.PositionFn() - a.PositionFn(), speed:b.ItemBase.Speed))
                            .Where(v => v.ab.Length2 < dMin2)
                            .Select(v => (move: -v.ab.ToLen(dMin - v.ab.Length), v.speed))
                            .ToArray()))
                    .Where(v => v.infos.Length > 0)
                    .ToArray();

                foreach (var (particle, infos) in offsets)
                {
                    var move = infos.Select(v => v.move).Sum();
                    var speed = infos.Select(v => v.speed).Concat(new []{ particle.Item.Speed }).Center();
                    //var nMove = move.Normalize();
                    particle.Item.Speed = speed;
                    particle.Item.Position += move;
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
            public IAnimatorPlaneItem Item;
            public Func<Vector3, Vector3> ProjectionFn;
        }

        class Particle : NetItem
        {
            public int i;
            public Vector3 Acceleration;
            public IAnimatorParticleItem Item;
            public override string ToString() => $"{Item.Position}, {Item.Speed}, {Acceleration}";
        }
    }
}
