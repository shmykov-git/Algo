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
using Aspose.ThreeD.Utilities;
using MathNet.Numerics;
using Model.Fourier;
using Model.Graphs;
using Model.Tools;
using View3D.Libraries;
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
            // .ApplyZ(Funcs3Z.SphereR(1.2))
            // Shapes.IcosahedronSp2.Mult(0.02).ApplyColor(Color.Red)
            // Shapes.GolfBall.Move(0.7, 1.5, 2).ToLines(1, Color.Red)

            //var fShape = FShapes.Generate(0, 4, 8, (-1, 10), (17, 1), (20, -2), (5, -3), (-6, 4), (200, 0.2), (0, 5), (0, -6));

            var fShape = new Fr[]
            {
                (-1, 10), (17, 1), (20, -2),
                //(200, 0.2), 
                (-5, -4), (6, 5),
                (-8, 3), (-7, -4),
                (10, 1), (3, 0.5),

                //(0, -1), (0, -0.5),
                
            };

            var N = 5000;

            //return FourierShapes.SearchSeriesOffset(fShape, 2, 3);
            //return FourierShapes.SearchSeries(fShape, -3, 4, -20, 20, -20, 20, 500);
            //return FourierShapes.SearchSeries(fShape, 6, -2, -10, 10, -10, 10, 100);

            //var searchShape = ((-10, 21), (-10, 21)).SelectRange((i, j) =>
            //    (fShape.ModifyTwoLasts((a, b) =>
            //     {
            //         a.n = i;
            //         b.n = j;
            //     }).ToShape(N, 0.01).ApplyColor(Color.Blue)
            //     + vectorizer.GetText($"{i} {j}").Perfecto(0.3).MoveY(-0.7).MoveZ(0.005).ToLines(1, Color.Red)
            //    ).MoveX(2 * j).MoveY(2 * i)).ToSingleShape();

            var sps = fShape.ToShapes(N, 0.01);

            //Shape debugShape = Shape.Empty;
            //try
            //{
            //    var sps = fShape.ToShapes(5000, 0.01);
            //}
            //catch (DebugException<Vector2[]> e)
            //{
            //    debugShape = new Polygon()
            //    {
            //        Points = e.Value
            //    }.ToShape().ToLines(1, Color.Red)/*.Rotate(Math.PI / 2).Adjust(0.8).ToNumSpots3(0.2)*/;
            //}

            //var sps = fShape.ToShapes(3000, null).Select(s => s.ToLines(0.3));

            var shape = new Shape[]
            {
                //debugShape,
                //fShape.ToNumShape(100, 0.1).ApplyColor(Color.Blue),

                //searchShape,

                sps.ToSingleShape().ApplyColor(Color.Blue),
                fShape.ToLineShape(N, 0.3).MoveZ(-0.005).ApplyColor(Color.Red),
                fShape.ToFormulaShape().Perfecto(2).ScaleX(0.6).MoveY(-0.7).ApplyColor(Color.DarkGreen),

                //sps.ToBlowedShape(1.05).ApplyColor(Color.Red),
                //fShape.ToNumShape(100, 0.1).MoveZ(0.01).ApplyColor(Color.Blue),
                //fShape.ToLineShape(4567, 0.3).MoveZ(-0.01).ApplyColor(Color.Blue),
                //sps.ToSingleShape().ApplyColor(Color.Blue),
                //sps.SelectWithIndex((s, i) => s.MoveZ(-i * 0.02)).ToSingleShape().ApplyColor(Color.DarkGreen),

                //Shapes.CoodsNet
            }.ToSingleShape();

            return shape;//.Rotate(Rotates.Z_Y);
        }
    }
}
