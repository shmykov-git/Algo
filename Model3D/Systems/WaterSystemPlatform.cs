using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Model3D.AsposeModel;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems.Model;
using Model3D.Tools;

namespace Model3D.Systems
{
    public static class WaterSystemPlatform
    {
        public static Shape Cube(WaterCubeModel model, WaterCubeOptions options = null) =>
            CubeMotion(model, options).WaterCubeMotionToStatic(options);

        public static Shape WaterCubeMotionToStatic(this IEnumerable<Shape> frameShapes, WaterCubeOptions options)
        {
            options ??= new WaterCubeOptions();
            var cubeSize = options.SceneSize;
            var motion = frameShapes.GetEnumerator();

            var jj = (options.SceneSteps.n - 1) / 2;
            var ii = (options.SceneSteps.m - 1) / 2;

            var shapes = options.SceneSteps.SelectSnakeRange((i, j) => (i - ii, j - jj)).Select(v =>
            {
                var (i, j) = v;

                motion.MoveNext();

                return motion.Current.Move(j * (cubeSize.x + 1), -i * (cubeSize.y + 1), 0);
            });

            return shapes.ToSingleShape();
        }

        public static IEnumerable<Shape> CubeMotion(WaterCubeModel model, WaterCubeOptions options = null)
        {
            options ??= new WaterCubeOptions();

            if (!model.RunCalculations)
                options.SceneSteps = (1, 1);

            var rnd = new Random(options.Seed);

            var particleRadius = options.ParticleRadius;
            var particleCount = options.ParticleCount;
            var netSize = options.NetSize;
            var cubeSize = options.SceneSize;

            // Visible Scene with logic scene
            var particle = Shapes.IcosahedronSp1.Mult(1.2 * particleRadius).ApplyColor(Color.Blue);

            var cube = Shapes.Cube.Scale(cubeSize);

            Shape GetCubeCollider(PlatformType type)
            {
                switch (type)
                {
                    case PlatformType.Circle:
                        return new[]
                        {
                            Surfaces.Circle(100, 2).Perfecto().Scale(cubeSize.x, cubeSize.z, 1).AddVolumeZ(cubeSize.y)
                                .ToOy().FilterPlanes(p => p.NOne.MultS(Vector3.YAxis).Abs() < 0.999),
                            cube.FilterPlanes(p => p.NOne.MultS(Vector3.YAxis).Abs() > 0.999).ReversePlanes()
                        }.ToSingleShape();

                    default:
                        return cube.ReversePlanes();
                }
            }

            Shape GetCubeGround(PlatformType type)
            {
                switch (type)
                {
                    case PlatformType.Square:
                        return Shapes.SquarePlatform(cubeSize.x, cubeSize.z, 0.5).MoveY(-cubeSize.y / 2);

                    case PlatformType.Circle:
                        return Shapes.CirclePlatform(cubeSize.x, cubeSize.z, 0.5).MoveY(-cubeSize.y / 2);

                    case PlatformType.Heart:
                        return Shapes.HeartPlatform(cubeSize.x, cubeSize.z, 0.5).MoveY(-cubeSize.y / 2);

                    case PlatformType.Mandelbrot:
                        return Shapes.MandelbrotPlatform(cubeSize.x, cubeSize.z, 0.5).MoveY(-cubeSize.y / 2);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            var cubeGround = GetCubeGround(options.PlatformType).ApplyColor(options.PlatformColor);
            var cubeCollider = GetCubeCollider(options.PlatformType);

            model.PlaneModels.Add(new WaterCubePlaneModel()
            {
                VisibleShape = cubeGround,
                ColliderShape = cubeCollider,
                DebugColliderSkip = model.DebugCollidersSkipCube,
                ColliderShift = particleRadius
            });

            if (model.GetStepItemsFn == null && options.WaterEnabled)
            {
                Item[] GetStepItems(int n) => (n).SelectRange(_ => new Item
                {
                    Position = rnd.NextCenteredV3(0.3) + options.WaterPosition,
                    Speed = options.WaterDir.ToLen(options.WaterSpeed)
                }).ToArray();

                model.GetStepItemsFn = GetStepItems;
            }

            // ----------


            // Scene Colliders

            Shape GetColliderShape(WaterCubePlaneModel model, bool withShift, bool allowSkip)
            {
                if ((allowSkip && model.DebugColliderSkip) || model.ColliderShape == null)
                    return Shape.Empty;

                return withShift ? model.ColliderShape.ResizeByNormals(-model.ColliderShift) : model.ColliderShape;
            }

            IEnumerable<PlaneItem> GetCollider(WaterCubePlaneModel model)
            {
                var logicShape = GetColliderShape(model, true, false);

                return logicShape.Planes.Select(c => new PlaneItem()
                {
                    Convex = c,
                    Position = c.Center(),
                    ForwardThickness = model.ColliderShift
                });
            }

            // ----------


            // Configuration

            var sceneSize = cubeCollider.GetBorders();
            var sceneColliders = model.PlaneModels.SelectMany(GetCollider).ToArray();

            var animator = new Animator(new AnimatorOptions()
            {
                UseGravity = true,
                GravityDirection = options.Gravity,
                GravityPower = options.GravityPower,

                UseParticleLiquidAcceleration = true,
                LiquidPower = options.LiquidPower,
                FrictionFactor = options.FrictionFactor,
                InteractionFactor = 5,
                ParticleRadius = particleRadius,
                ParticlePlaneThikness = options.ParticlePlaneBackwardThikness,
                ParticleMaxMove = options.ParticleMaxMove,

                NetSize = netSize,
                NetFrom = sceneSize.min - netSize * new Vector3(0.5, 0.5, 0.5),
                NetTo = sceneSize.max,

                StepDebugNotify = options.StepDebugNotify
            });

            var sw = Stopwatch.StartNew();

            if (model.RunCalculations)
                animator.AddPlanes(sceneColliders);

            Debug.WriteLine($"Planes: {sw.Elapsed}");
            sw.Restart();

            // ----------


            var items = new List<Item>();

            if (model.GetInitItemsFn != null)
            {
                var newItems = model.GetInitItemsFn(options.ParticleInitCount).Select(item => new Item()
                {
                    Position = item.Position,
                    Speed = item.Speed
                }).ToArray();

                animator.AddItems(newItems);
                items.AddRange(newItems);
                particleCount -= options.ParticleInitCount;
            }

            Shape GetStepShape() => new Shape[]
            {
                model.ModifyParticleFn == null
                ? items.Select(item => particle.Rotate(rnd.NextRotation()).Move(item.Position).ApplyMetaPoint(item.Position)).ToSingleShape()
                : items.Select(item =>
                {
                    var p = particle.Rotate(rnd.NextRotation()).Move(item.Position).ApplyMetaPoint(item.Position);
                    model.ModifyParticleFn(p);
                    return p;
                }).ToSingleShape(),
                
                model.DebugCollidersNoVisible 
                    ? Shape.Empty 
                    : model.PlaneModels.Where(m => !m.SkipVisible).Where(m=>!m.Debug || !model.RunCalculations).Select(m => m.VisibleShape).ToCompositeShape(),

                model.DebugColliders
                    ? model.DebugCollidersAsLines
                        ? model.PlaneModels.Select(m=>GetColliderShape(m, !model.DebugCollidersSkipShift, true)).ToSingleShape()
                            .ToLines(model.DebugCollidersAsLinesThikness).ApplyColor(Color.Green)
                        : model.PlaneModels.Select(m => GetColliderShape(m, !model.DebugCollidersSkipShift, true)).ToSingleShape().ApplyColor(Color.Green)
                    : Shape.Empty,

                model.DebugNetPlanes && animator.NetPlanes != null
                    ? animator.NetPlanes.Select(p => Shapes.Tetrahedron.Mult(0.05).Move(p)).ToSingleShape()
                        .ApplyColor(Color.Green)
                    : Shape.Empty,
            }.ToCompositeShape();

            void EmissionStep(int k)
            {
                if (!model.RunCalculations)
                    return;

                if (model.GetStepItemsFn != null && particleCount > 0)
                {
                    var newItems = model.GetStepItemsFn(options.ParticlePerEmissionCount)
                        .Select(item => new Item()
                        {
                            Position = item.Position,
                            Speed = item.Speed
                        }).ToArray();

                    animator.AddItems(newItems);
                    items.AddRange(newItems);

                    particleCount -= options.ParticlePerEmissionCount;
                }

                animator.Animate(options.EmissionAnimations);
            }

            if (options.SkipAnimations > 0)
                (options.SkipAnimations / options.EmissionAnimations).ForEach(EmissionStep);

            if (model.GetParticleFilterFn != null)
                items = items.Where(model.GetParticleFilterFn).Cast<Item>().ToList();

            var firstShape = GetStepShape();

            yield return firstShape;

            var shapes = (options.SceneMotionSteps - 1).Range().Select(_ =>
            {
                (options.StepAnimations / options.EmissionAnimations).ForEach(EmissionStep);
                var s = GetStepShape();
                
                return s;
            });

            foreach (var shape in shapes)
                yield return shape;

            Debug.WriteLine($"Scene: {sw.Elapsed}");
            sw.Stop();
        }

        #region watter model

        public class Item : IAnimatorParticleItem
        {
            public Vector3 Position { get; set; }
            public Vector3 Speed { get; set; }
        }

        public class PlaneItem : IAnimatorPlaneItem
        {
            public Vector3 Position { get; set; }
            public Vector3 Speed { get; set; }
            public Vector3[] Convex { get; set; }
            public double ForwardThickness { get; set; }
        }

        #endregion

    }
}