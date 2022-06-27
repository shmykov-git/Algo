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
using Model3D.Systems.Model;
using View3D.Libraries;
using Shape = Model.Shape;
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

        public Shape GetShape()
        {
            //return Aqueduct();
            //Test.Do();

            var platformSize = 3d;

            var rnd = new Random(0);

            var mass0 = 1;

            var stoneLogic = Shapes.Stone(4, 2, 1, 3, new Vector3(1.2, 3, 1.2)).Perfecto(Math.Pow(mass0, 1d/3)).RotateToMassY(out Quaternion q);
            var massCenter = stoneLogic.MassCenter;
            stoneLogic = stoneLogic.Move(massCenter);
            var stoneVisible = Shapes.Stone(4, 2, 1, 5, new Vector3(1.2, 3, 1.2)).Perfecto(Math.Pow(mass0, 1d / 3)).Rotate(q).Move(massCenter);

            var ps = stoneLogic.Points3;
            var masses = stoneLogic.Masses;

            var position0 = -stoneLogic.BottomY + new Vector3(0, 1, 0);
            var rotation0 = new ExQuaternion(0, 0, 0.2);
            var rotationSpeed0 = new ExQuaternion(0.1, 0.1, 0.1);

            var stone = new Solid()
            {
                VisibleShape = stoneVisible,
                LogicShape = stoneLogic,
                Items = ps.Select((p, i) => new Item()
                {
                    Position = p,
                    Mass = masses[i]
                }).ToArray(),

                Position = position0,
                Rotation = rotation0,
                Mass = mass0,
                PositionSpeed = Vector3.Origin,
                RotationSpeed = rotationSpeed0,

                PositionAcceleration = Vector3.Origin,
                RotationAcceleration = ExQuaternion.Identity,
            };


            //var q1 = Quaternion.FromRotation(Vector3.XAxis, new Vector3(1,2,3).Normalize());
            //var q2 = Quaternion.FromRotation(Vector3.YAxis, new Vector3(4, 5, 6).Normalize());

            //Quaternion Outer(Quaternion q, Quaternion p) => (q.Conjugate()*p + p.Conjugate() * q * -1) * 0.5;

            //var p = new Vector3(3, 5, 2);
            //var p1 = q2 * (q1 * p);
            //var q12 = Outer(q2, q1);
            //var p2 = q12 * p;


            var gravity = 0.01 * new Vector3(0, -1, 0);
            //var planeForce = new Vector3(0, 1, 0);
            var planePoint = ps.OrderByDescending(p => p.y).First();

            void CalculateStoneAccelerationsNoTouchPoints(Solid solid)
            {
                solid.RotationAcceleration = ExQuaternion.Identity;
                solid.PositionAcceleration = solid.Mass * gravity;
            }

            void CalculateStoneAccelerationsSingleTouchPoint(Solid solid, Vector3 touchPoint)
            {
                var touchDirection = touchPoint - solid.Position;


                //var planeForceDir = planePoint - v.Position;
                //var planeForce = planeForceDir.ToLen(planeForceDir.MultS(gravity).Abs());
                //var acc = v.Mass * (gravity + planeForce);

                // todo: 2 точки A, B. В точке A ускорение a, какое в точке B?
                //var accZ = 

                //v.Acceleration = acc;
            }

            // todo: посчитать Zx, Zy, Yx


            void Step()
            {


                //items.ForEach(v => v.Acceleration = Vector3.Origin);
                //stone.Items.ForEach(SetStoneAccelerations);

                //foreach (var item in items)
                //{
                //    item.Speed += item.Acceleration;
                //    item.Position += item.Speed;
                //}

                stone.Rotation = stone.RotationSpeed * stone.Rotation;
            }

            (10).ForEach(_ => Step());

            return Compounds.SnakeSlots((1, 1), (platformSize, platformSize), Step, () =>
                new[]
                {
                    stone.VisibleShape.Rotate(stone.Rotation).Move(stone.Position).ApplyColor(Color.Black),
                    stone.LogicShape.Rotate(stone.Rotation).Move(stone.Position).ToLines(0.5).ApplyColor(Color.Green),
                    Shapes.Coods.Rotate(stone.Rotation).Move(stone.Position),
                    Shapes.MandelbrotPlatform(platformSize, platformSize, 0.1),
                    Shapes.CoodsWithText.ApplyColor(Color.Black),
                    Shapes.SquarePlatform(platformSize, platformSize, 0.1).MoveY(-0.3),
                    Shapes.CirclePlatform(platformSize, platformSize, 0.1).MoveY(-0.6),
                    Shapes.HeartPlatform(platformSize, platformSize, 0.1).MoveY(-0.9),
                }.ToSingleShape());
        }

        class Solid
        {
            public Shape VisibleShape;
            public Shape LogicShape;
            public Item[] Items;
            public double Mass;
            public ExQuaternion Rotation;
            public Vector3 Position;
            public Vector3 PositionSpeed;
            public ExQuaternion RotationSpeed;

            public Vector3 PositionAcceleration;
            public ExQuaternion RotationAcceleration;
        }

        class Item
        {
            public Vector3 Position;
            public double Mass;
        }
    }
}
