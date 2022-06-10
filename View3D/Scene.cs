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

            return CubeGalaxiesIntersection();

            var pShape = Shapes.Cube.Perfecto(0.1);


            Vector3 Rotate(Vector3 v, Vector3 r)
            {
                var q = Quaternion.FromRotation(Vector3.ZAxis, r.Normalize());

                return q * v;
            }

            var scene = new (Shape s, Vector3 shift, Func<Shape, Vector3> speed, Color color)[]
            {
                (Shapes.Cube.SplitPlanes(0.1).ScaleY(5), new Vector3(-2.5, 0, 0), s=>0.5 * s.MassCenter.MultV(Vector3.YAxis), Color.Black),
                (Shapes.Cube.SplitPlanes(0.1).ScaleY(5).Rotate(1, 1, 1), new Vector3(2.5, 0, 0), s=>0.5 * s.MassCenter.MultV(Rotate(Vector3.YAxis, new Vector3(1,1,1))), Color.Black),
            };

            var particles = scene
                .SelectMany((s,k) => s.s.SplitByConvexes()
                    .Select(ss => new Particle()
                    {
                        Mass = 0.01,
                        Pos = ss.MassCenter + s.shift,
                        Speed = s.speed(ss),
                        Color = s.color
                    }))
                .Select((p, i) =>
                {
                    p.i = i;
                    return p;
                }).ToArray();

            var gPower = 0.1;

            void Step()
            {
                var speedChanges = particles.Select(a => particles.Where(b=>a!=b).Select(b => (b.Pos-a.Pos).ToLen(a.Mass*b.Mass * gPower / (b.Pos - a.Pos).Length2)).Sum()).ToArray();
                foreach (var p in particles)
                {
                    p.Speed += speedChanges[p.i];
                    p.Pos += p.Speed;
                }
            }

            void Animate(int steps)
            {
                for(var i=0;i<steps;i++)
                    Step();
            }

            Animate(10);

            var s = particles.Where(p=>p.Pos.Length<10).Select(p => pShape.Move(p.Pos).ApplyColor(p.Color)).ToSingleShape()
                .ApplyColorSphereGradient(Color.White, Color.Black, Color.Black);

            var shape = new Shape[]
            {
                s
            }.ToSingleShape();

            return shape;
        }
    }
}
