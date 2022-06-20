using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Mapster.Utils;
using MathNet.Numerics;
using Model.Fourier;
using Model.Graphs;
using Model.Tools;
using Model3D.Systems;
using View3D.Libraries;
using Triangulator = Model.Tools.Triangulator;
using Vector2 = Model.Vector2;

namespace View3D
{
    partial class Scene
    {
        #region ctor

            private readonly Settings settings;
        private readonly Vectorizer vectorizer;

        public Scene(Settings settings, Vectorizer vectorizer)
        {
            this.settings = settings;
            this.vectorizer = vectorizer;
        }    

        #endregion

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

        public Shape GetShape()
        {
            return WatterSystem.Waterfall(new WaterfallOptions()
            {
                SceneSize = new Vector3(15, 18, 15),
                SphereOffset = new Vector3(0, 0, 1),
                GutterRotation = new Vector3(0, 2, 1),
                GutterCurvature = 0,
                ParticleCount = 1000,
                SkipAnimations = 0,
                StepAnimations = 40
            });



            var rnd = new Random(0);

            var particleRadius = 0.1;
            var particleCount = 5000;
            var netSize = 0.25;
            var cubeSize = new Vector3(12, 15, 12);

            // Visible Scene (models)
            var particle = Shapes.Icosahedron.Mult(1.2*particleRadius).ApplyColor(Color.Blue);

            var cube = Shapes.Cube.Scale(cubeSize);
            var ground = Surfaces.Plane(2, 2).Perfecto().Rotate(Rotates.Z_mY).Scale(cubeSize).AddVolumeY(0.5).MoveY(-cubeSize.y / 2 - 0.25);
            var logicCube = cube.AddBorder(particleRadius);

            var sphere = Shapes.Ball.Perfecto(3).Where(v => v.y > -0.4).MoveY(-cubeSize.y / 2).MoveZ(4);
            var logicSphere = Shapes.IcosahedronSp2.Perfecto().Perfecto(3).Where(v => v.y > -0.1).MoveY(-cubeSize.y / 2).MoveZ(4).MovePlanes(-particleRadius);

            var gutter = Surfaces.Plane(20, 2).Perfecto().FlipY().Scale(4, 50, 1).AddPerimeterVolume(.6)
                //.MoveZ(-2.5)
                .MoveZ(-2).ApplyZ(Funcs3Z.CylinderXMR(4)).MoveZ(3.5)
                .Rotate(0, 6, 1).Move(0, cubeSize.y / 2 - 2, -2);
            var logicGutter = gutter.AddBorder(-particleRadius);

            var particles = (particleCount).SelectRange(_ => rnd.NextCenteredV3(1.5) + new Vector3(0, cubeSize.y / 2 - 1, -3))
                .ToArray();
                //Shapes.Cube.SplitPlanes(0.15).Mult(1.5).MoveY(cubeSize.y/2-1).MoveZ(-3)
                //.Points3.Select(p => p + rnd.NextV3(0.05)).ToArray();
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
            var items = particles.Select(p=>new Item{Position = p, Speed = new Vector3(0,0,0)}).ToArray();
            var newItems = new List<Item>();

            var animator = new Animator(new AnimatorOptions()
            {
                UseGravity = true,
                GravityPower = 0.001,

                UseParticleLiquidAcceleration = true,
                LiquidPower = 0.0001,
                InteractionFactor = 5,
                ParticleRadius = particleRadius,
                ParticlePlaneThikness = 2,
                MaxParticleMove = 2,

                NetSize = netSize,
                NetFrom = sceneSize.min - netSize * new Vector3(0.5, 0.5, 0.5),
                NetTo = sceneSize.max
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
                //cube.ToShapedLines(Shapes.CylinderR(30, 1, 1), 10).ApplyColor(Color.Black),
                ground.ApplyColor(Color.Black),
                sphere.ApplyColor(Color.Black),
                gutter.ApplyColor(Color.Black),

                //logicGutter.ToLines().ApplyColor(Color.Green),
                //logicSphere.ToLines(3, Color.BlueViolet),

                items.Select(item => particle.Rotate(rnd.NextRotation()).Move(item.Position)).ToSingleShape(),
                newItems.Select(item => particle.Rotate(rnd.NextRotation()).Move(item.Position)).ToSingleShape(),

                //animator.NetPlanes.Select(p => Shapes.Tetrahedron.Mult(0.05).Move(p)).ToSingleShape().ApplyColor(Color.Green),

                //Shapes.CoodsWithText, Shapes.CoodsNet
            }.ToSingleShape();

            //animator.Animate(25 * 9);

            var shape = (4, 4).SelectSnakeRange((i, j) =>
            {
                animator.Animate(50);

                //var items = Shapes.Cube.SplitPlanes(0.5).Mult(1.5).MoveY(2).MoveZ(-3).Points3.Select(p =>
                //    new Item
                //    {
                //        Position = p,
                //        Speed = new Vector3(0, 0, 0)
                //    }).ToArray();

                //animator.AddItems(items);
                //newItems.AddRange(items);

                return GetStepShape().Move(j * (cubeSize.x + 1), -i * (cubeSize.y + 1), 0);
            }).ToSingleShape();

            Debug.WriteLine($"Scene: {sw.Elapsed}");
            sw.Stop();

            return shape;

            animator.Animate(25);

            return GetStepShape();
        }
    }
}
