using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Meta;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems.Model;
using Model3D.Tools;
using View3D.Libraries;
using Vector2 = Model.Vector2;

namespace Model3D.Systems
{

    public static class WaterSystem
    {
        public static Shape Fountain(FountainOptions options = null)
        {
            var vectorizer = DI.Get<Vectorizer>();
            options ??= new FountainOptions();

            var rnd = new Random(options.Seed);
            var cubeSize = options.SceneSize;
            var particleRadius = options.ParticleRadius;

            var level1 = Surfaces.CircleAngle(40, 10, 0, Math.PI / 2)
                .Perfecto(8).AddPerimeterVolume(0.6).MoveZ(-2).ApplyZ(Funcs3Z.SphereMR(10)).MoveZ(12).ToOy()
                .MoveY(-cubeSize.y / 2 + 0.5);

            var level2 = Surfaces.CircleAngle(40, 10, 0, Math.PI / 2)
                .Perfecto(5).AddPerimeterVolume(0.6).MoveZ(-1.3).ApplyZ(Funcs3Z.SphereMR(7)).MoveZ(8.3).ToOy()
                .MoveY(-cubeSize.y / 2 + 3.5);

            var level3 = Surfaces.CircleAngle(20, 10, 0, Math.PI / 2)
                .Perfecto(3).AddPerimeterVolume(0.6).MoveZ(-1).ApplyZ(Funcs3Z.SphereMR(4)).MoveZ(5).ToOy()
                .MoveY(-cubeSize.y / 2 + 5.5);

            var models = new List<WaterCubePlaneModel>
            {
                new() {VisibleShape = level1},
                new() {VisibleShape = level2},
                new() {VisibleShape = level3}
            };

            if (options.JustAddShamrock)
            {
                var shamrock = Surfaces.Shamrock(480, 40).Perfecto(20).ToOy().MoveY(-cubeSize.y / 2 + 2);
                var logicShamrock = Surfaces.Shamrock(96, 8).Perfecto(20).ToOy().MoveY(-cubeSize.y / 2 + 2)
                    .MovePlanes(-particleRadius);

                var fire = vectorizer.GetContentShape("f1").Perfecto().ApplyZ(Funcs3Z.Waves).Mult(5)
                    .MoveY(-cubeSize.y / 2 + 3).ToLines(10);
                fire = fire.Rotate(1, 0, 1).Move(cubeSize.x / 2, 0, cubeSize.z / 2) +
                       fire.Rotate(-1, 0, 1).Move(-cubeSize.x / 2, 0, cubeSize.z / 2) +
                       fire.Rotate(1, 0, -1).Move(cubeSize.x / 2, 0, -cubeSize.z / 2) +
                       fire.Rotate(-1, 0, -1).Move(-cubeSize.x / 2, 0, -cubeSize.z / 2);

                models.Add(new WaterCubePlaneModel() { VisibleShape = shamrock, LogicShape = logicShamrock});
                models.Add(new WaterCubePlaneModel() { VisibleShape = fire, SkipLogic = true});
            }


            Item[] GetStepItems(int n) => (n).SelectRange(_ => new Item
            {
                Position = rnd.NextCenteredV3(0.5) + new Vector3(0, -cubeSize.y / 2 + 6.5, 0),
                Speed = options.ParticleSpeed
            }).ToArray();

            return CubePlatform(new WaterCubeModel()
            {
                GetStepItemsFn = GetStepItems,
                PlaneModels = models
            }, options);
        }

        public static Shape CubePlatform(WaterCubeModel model, WaterCubeOptions options = null)
        {
            options ??= new WaterCubeOptions();
            var rnd = new Random(options.Seed);

            var particleRadius = options.ParticleRadius;
            var particleCount = options.ParticleCount;
            var netSize = options.NetSize;
            var cubeSize = options.SceneSize;

            // Visible Scene with logic scene
            var particle = Shapes.Icosahedron.Mult(1.2 * particleRadius).ApplyColor(Color.Blue);

            var cube = Shapes.Cube.Scale(cubeSize);
            var ground = Surfaces.Plane(2, 2).Perfecto().ToOyM().Scale(cubeSize).AddVolumeY(0.5).MoveY(-cubeSize.y / 2 - 0.25);
            var logicCube = cube.AddBorder(particleRadius);
            // ----------


            // Scene Colliders
            IEnumerable<PlaneItem> GetCollider(Shape logicShape, bool reverseNormals = false) =>
                logicShape.Planes.Select(c => new PlaneItem()
                {
                    Convex = reverseNormals ? c.Reverse().ToArray() : c,
                    Position = c.Center()
                });

            var cubeCollider = GetCollider(logicCube, true);

            Shape GetLogicShape(WaterCubePlaneModel model)
            {
                if (model.SkipLogic)
                    return Shape.Empty;

                var logicShape = model.LogicShape ?? model.VisibleShape;

                return model.ColliderStrategy switch
                {
                    WaterCubeColliderStrategy.MovePlanes => logicShape.MovePlanes(-particleRadius),
                    WaterCubeColliderStrategy.AddBorder => logicShape.AddBorder(particleRadius),
                    _ => throw new NotImplementedException(model.ColliderStrategy.ToString())
                };
            }

            var modelColliders = model.PlaneModels.Select(GetLogicShape).Select(logicShape => GetCollider(logicShape));

            // ----------


            // Configuration

            var sceneCollider = new[]
                {
                    cubeCollider,
                }
                .Concat(modelColliders)
                .ManyToArray();

            var sceneSize = logicCube.GetBorders();

            var animator = new Animator(new AnimatorOptions()
            {
                UseGravity = true,
                GravityDirection = options.Gravity,
                GravityPower = options.GravityPower,

                UseParticleLiquidAcceleration = true,
                LiquidPower = options.LiquidPower,
                InteractionFactor = 5,
                ParticleRadius = particleRadius,
                ParticlePlaneThikness = 2,
                MaxParticleMove = 2,

                NetSize = netSize,
                NetFrom = sceneSize.min - netSize * new Vector3(0.5, 0.5, 0.5),
                NetTo = sceneSize.max,

                StepDebugNotify = options.StepDebugNotify
            });

            var sw = Stopwatch.StartNew();
            animator.AddPlanes(sceneCollider);
            Debug.WriteLine($"Planes: {sw.Elapsed}");
            sw.Restart();

            // ----------


            var items = new List<Item>();

            if (model.GetInitItemsFn != null)
                items.AddRange(model.GetInitItemsFn(options.ParticleInitCount).Select(item => new Item()
                {
                    Position = item.Position,
                    Speed = item.Speed
                }));

            Shape GetStepShape() => new Shape[]
            {
                ground.ApplyColor(Color.Black),

                items.Select(item => particle.Rotate(rnd.NextRotation()).Move(item.Position)).ToSingleShape(),
                model.PlaneModels.Where(m => !m.SkipVisible).Select(m => m.VisibleShape).ToSingleShape(),

                //animator.NetPlanes.Select(p => Shapes.Tetrahedron.Mult(0.05).Move(p)).ToSingleShape().ApplyColor(Color.Green),
            }.ToSingleShape();

            void EmissionStep(int k)
            {
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

            var firstShape = GetStepShape();

            var shapes = options.SceneSteps.SelectSnakeRange((i, j) => (i, j)).Skip(1).Select(v =>
            {
                var (i, j) = v;

                (options.StepAnimations / options.EmissionAnimations).ForEach(EmissionStep);

                return GetStepShape().Move(j * (cubeSize.x + 1), -i * (cubeSize.y + 1), 0);
            });

            var shape = new[] { firstShape }.Concat(shapes).ToSingleShape();

            Debug.WriteLine($"Scene: {sw.Elapsed}");
            sw.Stop();

            return shape;
        }

        public static Shape Waterfall(WaterfallOptions options = null)
        {
            options ??= new WaterfallOptions();
            var rnd = new Random(options.Seed);

            var particleRadius = options.ParticleRadius;
            var particleCount = options.ParticleCount;
            var netSize = options.NetSize;
            var cubeSize = options.SceneSize;

            // Visible Scene (models)
            var particle = Shapes.Icosahedron.Mult(1.2 * particleRadius).ApplyColor(Color.Blue);

            var cube = Shapes.Cube.Scale(cubeSize);
            var ground = Surfaces.Plane(2, 2).Perfecto().Rotate(Rotates.Z_mY).Scale(cubeSize).AddVolumeY(0.5).MoveY(-cubeSize.y / 2 - 0.25);
            var logicCube = cube.AddBorder(particleRadius);

            var sphere = Shapes.Ball.Perfecto(options.SphereRadius).Where(v => v.y > -0.4).MoveY(-cubeSize.y / 2).MoveZ(4).Move(options.SphereOffset);
            var logicSphere = Shapes.IcosahedronSp2.Perfecto().Perfecto(options.SphereRadius).Where(v => v.y > -0.1).MoveY(-cubeSize.y / 2).MoveZ(4).Move(options.SphereOffset).MovePlanes(-particleRadius);

            var gutterTmp = Surfaces.Plane(20, 2).Perfecto().FlipY().Scale(4, 50, 1).AddPerimeterVolume(.6);
            gutterTmp = options.GutterCurvature.Abs() < 0.001
                ? gutterTmp.MoveZ(-2.5)
                : gutterTmp.MoveZ(-2 / options.GutterCurvature).ApplyZ(Funcs3Z.CylinderXMR(4 / options.GutterCurvature))
                    .MoveZ(6 / options.GutterCurvature - 2.5);
            var gutter = gutterTmp.Rotate(options.GutterRotation, Vector3.YAxis).Move(0, cubeSize.y / 2 - 2, -2).Move(options.GutterOffset);
            var logicGutter = gutter.AddBorder(-particleRadius);

            var particles = (particleCount).SelectRange(_ => rnd.NextCenteredV3(1.5) + new Vector3(0, cubeSize.y / 2 - 1, -3) + options.WatterOffset)
                .ToArray();
            // ----------


            // Logic Scene (colliders)
            var cubeCollider = logicCube.Planes.Select(c => new PlaneItem()
            {
                Convex = c.Reverse().ToArray(),
                Position = c.Center()
            });

            var sphereCollider = logicSphere.Planes.Select(c => new PlaneItem()
            {
                Convex = c,
                Position = c.Center()
            });

            var gutterCollider = logicGutter.Planes.Select(c => new PlaneItem()
            {
                Convex = c,
                Position = c.Center()
            });

            // ----------


            // Configuration

            var sceneCollider = cubeCollider
                .Concat(sphereCollider)
                .Concat(gutterCollider)
                .ToArray();

            var sceneSize = logicCube.GetBorders();
            var items = particles.Select(p => new Item { Position = p, Speed = new Vector3(0, 0, 0) }).ToArray();

            var animator = new Animator(new AnimatorOptions()
            {
                UseGravity = true,
                GravityDirection = options.Gravity,
                GravityPower = options.GravityPower,

                UseParticleLiquidAcceleration = true,
                LiquidPower = options.LiquidPower,
                InteractionFactor = 5,
                ParticleRadius = particleRadius,
                ParticlePlaneThikness = 2,
                MaxParticleMove = 2,

                NetSize = netSize,
                NetFrom = sceneSize.min - netSize * new Vector3(0.5, 0.5, 0.5),
                NetTo = sceneSize.max,

                StepDebugNotify = options.StepDebugNotify
            });

            var sw = Stopwatch.StartNew();
            animator.AddItems(items);
            Debug.WriteLine($"Items: {sw.Elapsed}");
            sw.Restart();

            animator.AddPlanes(sceneCollider);
            Debug.WriteLine($"Planes: {sw.Elapsed}");
            sw.Restart();

            // ----------


            Shape GetStepShape() => new Shape[]
            {
                ground.ApplyColor(Color.Black),
                sphere.ApplyColor(Color.Black),
                gutter.ApplyColor(Color.Black),

                items.Select(item => particle.Rotate(rnd.NextRotation()).Move(item.Position)).ToSingleShape(),
            }.ToSingleShape();

            animator.Animate(options.SkipAnimations);

            var shape = options.SceneSteps.SelectSnakeRange((i, j) =>
            {
                animator.Animate(options.StepAnimations);

                return GetStepShape().Move(j * (cubeSize.x + 1), -i * (cubeSize.y + 1), 0);
            }).ToSingleShape();

            Debug.WriteLine($"Scene: {sw.Elapsed}");
            sw.Stop();

            return shape;
        }

        #region watter model

        class Item : IAnimatorParticleItem
        {
            public Vector3 Position { get; set; }
            public Vector3 Speed { get; set; }
        }

        class PlaneItem : IAnimatorPlaneItem
        {
            public Vector3 Position { get; set; }
            public Vector3 Speed { get; set; }
            public Vector3[] Convex { get; set; }
            public Vector3 Normal { get; set; }
        }

        #endregion
    }
}
