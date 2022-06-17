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
            #region триангуляция (не работает нормально)

            //var fShape = new Fr[]
            //    {(-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 2), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1)};

            //var s = fShape.ToShape(3000, 0.02, indices: new[] { 0 }).ApplyColor(Color.Red);



            //var mb = Polygons.Polygon5;

            //var mb = MandelbrotFractalSystem.GetPoints(2, 0.003, 1000, 0.99).ToPolygon();

            //return /*mb.ToShape().ToNumSpots3(0.3, Color.Blue) +*/ mb.ToShape().ToLines(0.5, Color.Red);


            //var s1 = mb.ToShape().ToLines().ApplyColor(Color.Red);// + Shapes.Ball.Mult(0.1).ApplyColor(Color.Red);

            //Shape s;

            //try
            //{
            //    //    var ts = Triangulator.Triangulate(mb, 0.01);
            //    //    s = new Shape() { Points2 = mb.Points, Convexes = ts }.ApplyColor(Color.Red);
            //    //s = ts.SelectWithIndex((t,i)=>new Shape() {Points2 = mb.Points, Convexes = new []{t}}.MoveZ(0)).ToSingleShape();
            //    s = mb.ToTriangulatedShape(40,incorrectFix:0)/*.Perfecto().ApplyZ(Funcs3Z.Hyperboloid)*/.ApplyColor(Color.Blue).ToLines(0.1, Color.Blue);//); 
            //}
            //catch (DebugException<(Shape polygon, Shape triangulation)> e)
            //{
            //    s = e.Value.triangulation.ToLines(0.2,
            //        Color.Blue); // + e.Value.polygon.ToMetaShape3(0.2, 0.2, Color.Green, Color.Red);
            //}
            //catch (DebugException<(Polygon p, int[][] t, Vector2[] ps)> e)
            //{
            //    s = new Shape() {Points2 = e.Value.p.Points, Convexes = e.Value.t}.ApplyColor(Color.Red)
            //        + e.Value.ps.ToPolygon().ToShape().ToNumSpots3(0.3, Color.Green)
            //        ;//+ mb.ToShape().ToSpots3(0.2, Color.Blue);
            //}

            //var s4 = net.Cut(mb.ToPolygon()).ToLines(0.5).ApplyColor(Color.Blue);

            #endregion

            var rnd = new Random(0);

            var particleRadius = 0.2;
            var cubeSize = new Vector3(8, 6, 8);

            // Visible Scene
            var particle = Shapes.IcosahedronSp1.Mult(particleRadius).ApplyColor(Color.Blue);

            var cube = Shapes.Cube.Scale(cubeSize);
            var ground = Surfaces.Plane(2, 2).Perfecto().Rotate(Rotates.Z_mY).Scale(cubeSize).AddVolumeY(0.5).MoveY(-cubeSize.y/2-0.25);
            var logicSphere = Shapes.IcosahedronSp2.Perfecto().Perfecto(3).Where(v => v.y > -0.1).MoveY(-cubeSize.y / 2);
            var insideSphere = Shapes.Ball.Perfecto(3).Where(v => v.y > -0.4).MoveY(-cubeSize.y / 2);
            var logicGutter = Surfaces.Plane(20, 2).Perfecto().Scale(2, 50, 1).MoveZ(-1).ApplyZ(Funcs3Z.CylinderXMR(1.1)).Rotate(0, 2, 1).Move(0, 3, -3);
            var gutter = Surfaces.Plane(20, 2).Perfecto().Scale(2, 50, 1).AddVolumeZ(0.2).MoveZ(-1).ApplyZ(Funcs3Z.CylinderXMR(1.1))
                .Rotate(0, 2, 1).Move(0, 3, -3);

            // ----------


            // Logic Scene (colliders)
            var logicCube = cube.AddBorder(particleRadius);
            var cubeCollider = logicCube.Planes.Select(c => new PlaneItem()
            {
                Convex = c.Reverse().ToArray(),
                Position = c.Center()
            });

            var sphereCollider = logicSphere.MovePlanes(-particleRadius).Planes.Select(c => new PlaneItem()
            {
                Convex = c,
                Position = c.Center()
            });

            var gutterCollider = logicGutter.MovePlanes(particleRadius).Planes.Select(c => new PlaneItem()
            {
                Convex = c,
                Position = c.Center()
            });

            var particles =
            (
                Shapes.Cube.SplitPlanes(0.5).Mult(1).MoveY(2).MoveZ(-3)/*.MoveX(1)*/
                //Shapes.Cube.SplitPlanes(0.5).Mult(1).MoveX(-1) +
                //Shapes.Cube.SplitPlanes(0.5).Mult(1).MoveZ(1) +
                //Shapes.Cube.SplitPlanes(0.5).Mult(1).MoveZ(-1)
            ).Points3.Select(p => p + rnd.NextV3(0.05)).ToArray();
            // ----------


            // Configuration

            var sceneCollider = cubeCollider
                .Concat(sphereCollider)
                .Concat(gutterCollider)
                .ToArray();
            var sceneSize = logicCube.GetBorders();
            var items = particles.Select(p=>new Item{Position = p, Speed = new Vector3(0,0,0)}).ToArray();
            var newItems = new List<Item>();

            var netSize = 0.25;

            var animator = new Animator(new AnimatorOptions()
            {
                UseGravity = true,
                GravityPower = 0.01,

                UseParticleLiquidAcceleration = true,
                LiquidPower = 0.001,
                InteractionFactor = 5,
                ParticleRadius = particleRadius,
                ParticlePlaneThikness = 3,
                MaxParticleMove = 2,

                NetSize = netSize,
                NetFrom = sceneSize.min - netSize * new Vector3(0.5, 0.5, 0.5),
                NetTo = sceneSize.max
            });

            animator.AddItems(items);
            animator.AddPlanes(sceneCollider);

            // ----------


            //(20).ForEach(k =>
            //{
            //    var items = Shapes.Cube.SplitPlanes(0.5).Mult(1).MoveY(1).Points3.Select(p =>
            //        new Item
            //        {
            //            Position = p,
            //            Speed = new Vector3(0, 0, 0)
            //        }).ToArray();

            //    animator.AddItems(items);
            //    newItems.AddRange(items);

            //    animator.Animate(15);
            //});

            Shape GetShape() => new Shape[]
            {
                //cube.ToShapedLines(Shapes.CylinderR(30, 1, 1), 10).ApplyColor(Color.Black),
                ground.ApplyColor(Color.Black),
                insideSphere/*.ToShapedLines(Shapes.CylinderR(7, 1, 1), 3)*/.ApplyColor(Color.Black),
                gutter.ApplyColor(Color.Black),

                //insideLogicSphere.ToLines(3, Color.Blue),
                //sceneCube.ToLines(3, Color.Blue),
                //ss.Select((s,i)=>s.MoveX(3*(i-5))).ToSingleShape(),

                items.Select(item => particle.Move(item.Position)).ToSingleShape(),
                newItems.Select(item => particle.Move(item.Position)).ToSingleShape(),

                //animator.NetPlanes.Select(p => Shapes.Tetrahedron.Mult(0.05).Move(p)).ToSingleShape().ApplyColor(Color.Green),

                //Shapes.CoodsWithText, Shapes.CoodsNet
            }.ToSingleShape();

            //animator.Animate(200);

            return (5, 5).SelectRange((i, j) =>
            {
                animator.Animate(5);

                return GetShape().Move(j * (cubeSize.x + 1), -i * (cubeSize.y + 1), 0);
            }).ToSingleShape();


            var shape = GetShape();

            return shape;
        }
    }
}
