using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Tools;
using View3D.Libraries;
using Vector2 = Model.Vector2;

namespace Model3D.Systems
{
    public class WaterfallOptions
    {
        public Vector3 SceneSize = new Vector3(12, 15, 12);
        public (int m, int n) SceneSteps = (4, 4);
        public int ParticleCount = 500;
        public double ParticleRadius = 0.1;
        public double NetSize = 0.25;
        public double GutterCurvature = 1; // from 0 to 2
        public Vector3 GutterOffset = new Vector3(0, 0, 0);
        public Vector3 GutterRotation = new Vector3(0, 6, 1);
        public Vector3 SphereOffset = new Vector3(0, 0, 0);
        public double SphereRadius = 3;
        public Vector3 WatterOffset = new Vector3(0, 0, 0);
        public Vector3 Gravity = new Vector3(0, -1, 0);
        public double GravityPower = 0.001;
        public double LiquidPower = 0.0001;
        public int Seed = 0;
        public int SkipAnimations = 0;
        public int StepAnimations = 40;
        public int? StepDebugNotify = 50;
    }

    public static class WatterSystem
    {
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

        #region watterfall model

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
