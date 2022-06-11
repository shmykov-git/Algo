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

            var ss = CubeGalaxiesIntersection(1, 20, 0.1, 1.5, 10).Perfecto(1.3).Rotate(1, -2, 3);

            var s = vectorizer.GetContentShape("s14").ToLines(0.5).ApplyColor(Color.Blue);
            


            var shape = new Shape[]
            {
                s,
                ss

            }.ToSingleShape();

            return shape;
        }
    }
}
