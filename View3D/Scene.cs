using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Tools;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
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
            #region улучшить алгоритм триангуляции (без добавления)

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


            return BlackHole(new BlachHoleOptions()
            {
                Shape = Surfaces.Plane(100, 100).Perfecto(4),
                Colors = new Color?[]{ Color.Black, Color.Black, Color.White},
                NoRotation = false,
                TryBeauty = true,
                AngleSpeed = 0.17,
                GravityPower = 0.2,
                BlowFactor = 0,
                BlowRadius = 0,
                InterationsCount = 120
            });

            var shape = new Shape[]
            {
                //vectorizer.GetContentShape("b7").ApplyZ(Funcs3Z.Hyperboloid).Rotate(10,10,1).Move(0.7,1.0,0).ToLines().Mult(0.5).ApplyColor(Color.DeepPink),
               Surfaces.Heart(50, 25).Perfecto().ToLines(0.3).ApplyColor(Color.Blue),


                //Shapes.CoodsWithText
            }.ToSingleShape();

            return shape;
        }
    }
}
