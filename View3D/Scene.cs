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
            return Aqueduct();

            var rnd = new Random(0);

            var i = 2;

            var s = Shapes.Stone(4, i, 1, 5, new Vector3(1.2, 3, 1.2)).Perfecto();
            var ms = s.RotateToMassY().AlignY(0).ApplyColor(Color.DarkGreen);
            var ts = s.RotateToTopY().AlignY(0).ApplyColor(Color.Black);

            var mc = ms.MoveX(-0.7).MassCenter;
            var (mt, mb) = ms.MoveX(-0.7).TopsY;

            var tc = ts.MoveX(0.7).MassCenter;
            var (tt, tb) = ts.MoveX(0.7).TopsY;


            //var c = s.AlignY(0).MassCenter;

            //var t = s.AlignY(0).TopY;
            //var b = s.AlignY(0).BottomY;

            return new Shape[]
            {
                Shapes.Coods.Mult(0.3).Move(mc).ApplyColor(Color.Red),
                Shapes.Coods.Mult(0.3).Move(mt).ApplyColor(Color.Red),
                Shapes.Coods.Mult(0.3).Move(mb).ApplyColor(Color.Red),
                Shapes.Coods.Mult(0.3).Move(tc).ApplyColor(Color.Red),
                Shapes.Coods.Mult(0.3).Move(tt).ApplyColor(Color.Red),
                Shapes.Coods.Mult(0.3).Move(tb).ApplyColor(Color.Red),
                s.AlignY(0).MoveZ(-1).ApplyColor(Color.Blue),
                ms,ts,
                ms.MoveX(-0.7), ts.MoveX(0.7),
                Shapes.SquarePlatform(),
                //Shapes.CoodsWithText.MoveY(0.5)
            }.ToSingleShape();

            var stone = Shapes.Stone(4, 2, 0.5, 2).Perfecto().RotateToMassY(out Quaternion q).AlignY(0);
            var stoneVisible = Shapes.Stone(4, 0, 0.5, 5).Perfecto().Rotate(q).AlignY(0);

            var ps = stone.Points3;

            //double GetConvexArea(int[] c)
            //{
            //    var a = ps[c[1]] - ps[c[0]];
            //    var b = ps[c[2]] - ps[c[1]];

            //    return 0.5 * a.MultV(b).Length;
            //}

            //var masses = stone.Convexes.SelectMany(c => c.Select(i => (i, c))).GroupBy(v => v.i)
            //    .Select(gv => (i: gv.Key, m: gv.Select(v => GetConvexArea(v.c)).Average())).OrderBy(v => v.i)
            //    .Select(v => v.m).ToArray();

            //var massAvg = masses.Average();
            //masses = masses.Select(m => m / massAvg).ToArray();

            var masses = stone.Masses;

            var min = masses.Min();
            var max = masses.Max();

            var k = max / min;

            var items = ps.Select((p, i) => new Item()
            {
                Acceleration = Vector3.Origin,
                Speed = Vector3.Origin,
                Position = p,
                Mass = masses[i]
            }).ToArray();

            var rot = Quaternion.Identity;
            var move = Vector3.Origin;
            var center = items.Select(v => v.Mass * v.Position).Center();
            var posZ = center + Vector3.ZAxis;
            var speedZ = Vector3.Origin;
            var posY = center + Vector3.YAxis;
            var speedY = Vector3.Origin;

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

            void SetStoneAccelerations(Item v)
            {
                var planeForceDir = planePoint - v.Position;
                var planeForce = planeForceDir.ToLen(planeForceDir.MultS(gravity).Abs());
                var acc = v.Mass * (gravity + planeForce);

                // todo: 2 точки A, B. В точке A ускорение a, какое в точке B?
                //var accZ = 

                v.Acceleration = acc;
            }

            // todo: посчитать Zx, Zy, Yx


            void Step()
            {


                //items.ForEach(v => v.Acceleration = Vector3.Origin);
                items.ForEach(SetStoneAccelerations);

                //foreach (var item in items)
                //{
                //    item.Speed += item.Acceleration;
                //    item.Position += item.Speed;
                //}
            }

            (1).ForEach(_=>Step());

            return new Shape[]
            {
                stoneVisible.Rotate(rot).Move(move).ApplyColor(Color.Black),
                stone.Rotate(rot).Move(move).ToLines(1).ApplyColor(Color.Green),
                Surfaces.Plane(2,2).Perfecto(3).AddNormalVolume(0.1).ToOyM().ApplyColor(Color.Black),
                Shapes.CoodsWithText.Rotate(rot).Move(move+center)
            }.ToSingleShape();
        }

        class Item
        {
            public Vector3 Acceleration;
            public Vector3 Speed;
            public Vector3 Position;
            public double Mass;
        }
    }
}
