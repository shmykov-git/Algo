using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using ViewMotion.Extensions;
using ViewMotion.Models;

namespace ViewMotion;

partial class SceneMotion
{

    public Task<Motion> Stone()
    {
        var platformSize = 3d;

        var rnd = new Random(0);

        var mass0 = 1;

        var stoneLogic = Shapes.Stone(4, 2, 1, 3, new Vector3(1.2, 3, 1.2)).Perfecto(Math.Pow(mass0, 1d / 3)).RotateToMassY(out Quaternion q);
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
            Items = ps.Select((p, i) => new StoneItem()
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

            touchPoint = stoneLogic.ProjectionBottomY;
            if (touchPoint.y < 0)
            {
                var plane = new Plane(Vector3.XAxis, Vector3.Origin, Vector3.ZAxis);

                var posY = touchPoint.y;
                var speedY = stone.PositionSpeed.y;

                var xz = plane.ProjectionFn(touchPoint - stone.Position);
                //var prz = plane.NOne.MultS(touchPoint - stone.Position);

                var kSpeed = 0.3;
                var kPos = 3;

                var q = new ExQuaternion(new Vector3(-kSpeed * xz.z * speedY, 0, -kSpeed * xz.x * speedY));
                stone.RotationSpeed *= q;

                stone.PositionSpeed += new Vector3(0, -speedY, 0);

                //stone.Position += new Vector3(0, -posY, 0);
                stone.Position += new Vector3(kPos * xz.z * posY, -posY, kPos * xz.x * posY);

                var w = 1 - (stone.RotationSpeed * Vector3.YAxis).MultS(Vector3.YAxis);
                var dY = (stone.Position - new Vector3(0, stone.Position.y, 0)).Length;
                Debug.WriteLine($"w:{w}, dy:{dY}");
            }


            //items.ForEach(v => v.Acceleration = Vector3.Origin);
            //stone.Items.ForEach(SetStoneAccelerations);

            //foreach (var Item in items)
            //{
            //    Item.Speed += Item.Acceleration;
            //    Item.Position += Item.Speed;
            //}

            stone.Rotation = stone.RotationSpeed * stone.Rotation;
        }

        Shape GetSceneShape() =>
            new[]
            {
                Shapes.IcosahedronSp3.Perfecto(0.1).Move(touchPoint).ApplyColor(Color.Red),
                stone.VisibleShape.Rotate(stone.Rotation).Move(stone.Position).ApplyColor(Color.DarkBlue),
                stone.LogicShape.Rotate(stone.Rotation).Move(stone.Position).ToLines(1).ApplyColor(Color.Green),
                Shapes.Coods.Rotate(stone.Rotation).Move(stone.Position),
                Shapes.CirclePlatform(platformSize, platformSize, 0.1).ApplyColor(Color.FromArgb(128, 0, 0)),
                Shapes.CoodsWithText.ApplyColor(Color.DarkBlue),
            }.ToSingleShape();

        IEnumerable<Shape> Animate()
        {
            while (true)
            {
                Step();
                yield return GetSceneShape();
            }
        }

        return Animate().ToMotion();
    }

    class Solid
    {
        public Shape VisibleShape;
        public Shape LogicShape;
        public StoneItem[] Items;
        public double Mass;
        public ExQuaternion Rotation;
        public Vector3 Position;
        public Vector3 PositionSpeed;
        public ExQuaternion RotationSpeed;

        public Vector3 PositionAcceleration;
        //public ExQuaternion RotationAcceleration;
    }

    class StoneItem
    {
        public Vector3 Position;
        public double Mass;
    }
}