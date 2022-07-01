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
using Model3D.Tools.Model;
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
            return Shapes.Butterfly2Platform() + Shapes.CoodsWithText;

            var platformSize = 3d;

            var rnd = new Random(0);

            var mass0 = 1;

            var stoneLogic = Shapes.Stone(4, 2, 1, 3, new Vector3(1.2, 3, 1.2)).Perfecto(Math.Pow(mass0, 1d/3)).RotateToMassY(out Quaternion q);
            var massCenter = stoneLogic.MassCenter;
            stoneLogic = stoneLogic.Move(massCenter);
            var stoneVisible = Shapes.Stone(4, 2, 1, 5, new Vector3(1.2, 3, 1.2)).Perfecto(Math.Pow(mass0, 1d / 3)).Rotate(q).Move(massCenter);

            var ps = stoneLogic.Points3;
            var masses = stoneLogic.Masses;

            var rotation0 = new ExQuaternion(0, 0, 1);
            var rotationSpeed0 = ExQuaternion.Identity;
            var projectionBottom0 = stoneLogic.Rotate(rotation0).ProjectionBottomY;
            var position0 = new Vector3(0, -projectionBottom0.y, 0);
            
            var touchPoint0 = projectionBottom0 + position0;

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
            };

            var gravity = 0.01 * new Vector3(0, -1, 0);
            //var planeForce = new Vector3(0, 1, 0);
            //var planePoint = ps.OrderByDescending(p => p.y).First();


            //void CalculateStoneAccelerationsSingleTouchPoint(Solid solid, Vector3 touchPoint)
            //{
            //    var touchDirection = touchPoint - solid.Position;

            //    solid.Items.Select(v =>
            //    {
            //        // todo: разложить v.position, gravity на ускорение вращения и ускорение перемещения центра
            //        // плоскость точки касания, центра, позиции
            //        // проекциия верктора гравитации на эту плоскость - это определяет ускорение перемещения без вращения (с учетом массы и расстояния)
            //        // а проекция на нормаль это плоскости определяет ускорение вращения (с учетом массы и расстояния)
                    
            //        // нужны только точки касания
            //        //  для обсчета достаточно центра, скоростей, ускорений, массы

            //        var a = touchDirection.Normalize();
            //        var b = v.Position.Normalize();
            //        var c = a.MultV(b);
            //        var d = a.MultV(c);

            //        var l = v.Position.MultV(touchDirection);

            //        return Vector3.Origin;
            //    }).Sum();

            //    //var planeForceDir = planePoint - v.Position;
            //    //var planeForce = planeForceDir.ToLen(planeForceDir.MultS(gravity).Abs());
            //    //var acc = v.Mass * (gravity + planeForce);

            //    // todo: 2 точки A, B. В точке A ускорение a, какое в точке B?
            //    //var accZ = 

            //    //v.Acceleration = acc;
            //}

            // todo: посчитать Zx, Zy, Yx

            var touchPoint = touchPoint0;
            void Step()
            {
                var a = stone.Mass * gravity;
                stone.PositionSpeed += a;
                stone.Position += stone.PositionSpeed;
                stone.Rotation *= stone.RotationSpeed;

                // todo: найти точку взаимодействия с плоскостью
                var stoneLogic = stone.LogicShape.Rotate(stone.Rotation).Move(stone.Position);

                var touchPoint = stoneLogic.ProjectionBottomY; // тут точка неправильная
                if (touchPoint.y < 0)
                {
                    var plane = new Plane(Vector3.XAxis, Vector3.Origin, Vector3.ZAxis);

                    var ln = touchPoint.y;

                    var prxy = plane.ProjectionFn(touchPoint - stone.Position);
                    //var prz = plane.NOne.MultS(touchPoint - stone.Position);

                    var q = new ExQuaternion(prxy.Length * new Vector3(-ln / 2, 0, -ln / 2));
                    stone.RotationSpeed *= q;

                    stone.Position += new Vector3(0, -ln, 0);
                }


                //items.ForEach(v => v.Acceleration = Vector3.Origin);
                //stone.Items.ForEach(SetStoneAccelerations);

                //foreach (var item in items)
                //{
                //    item.Speed += item.Acceleration;
                //    item.Position += item.Speed;
                //}

                stone.Rotation = stone.RotationSpeed * stone.Rotation;
            }

            void Animate() => (5).ForEach(_ => Step());

            //(10).ForEach(_ => Step());

            return Compounds.SnakeSlots((1, 1), (platformSize, platformSize), Animate, () =>
                new[]
                {
                    Shapes.IcosahedronSp3.Perfecto(0.05).Move(touchPoint).ApplyColor(Color.Red),
                    stone.VisibleShape.Rotate(stone.Rotation).Move(stone.Position).ApplyColor(Color.Black),
                    stone.LogicShape.Rotate(stone.Rotation).Move(stone.Position).ToLines(0.5).ApplyColor(Color.Green),
                    Shapes.Coods.Rotate(stone.Rotation).Move(stone.Position),
                    Shapes.Butterfly2Platform(platformSize, platformSize, 0.1),
                    Shapes.CoodsWithText.ApplyColor(Color.Black),
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
            //public ExQuaternion RotationAcceleration;
        }

        class Item
        {
            public Vector3 Position;
            public double Mass;
        }
    }
}
