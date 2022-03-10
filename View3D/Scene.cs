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

            var fShape = new Fr[]
            {
                (10, -1),
                (9, 3),
                (4, -7),
                (-3, -8),
                (3, 5),
                (-9, 3),
                (2, 5),
                (-18, 3),
                (1, 3),
                (-6, -1),
                (200, -1),
                (-16, 1),
                (11, -2),
                (13, 1),
                (13, -2),
                (-8, 5),
                (-37, 2),
                (1,2),
                (3,4),
                (-2,3),
                (-1,5),
                (5,1),
                (16, -3),

                //(9, -3), (0, 4),
                //(-19, -3), (16, 4),
                (5, -3), (5, 4),

                //(-1, 10),
                //(17, 1), (20, -2),
                //(5, -3), (-6, 4),
                //(200, 0.2)
            };

            //return FourierShapes.SearchSeriesOffset(fShape, 2, 3);
            //return FourierShapes.SearchSeries(fShape, -3, 4, -20, 20, -20, 20, 500);
            //return FourierShapes.SearchSeries(fShape, 6, -2, -10, 10, -10, 10, 100);

            var sps = (10, 10).SelectMiddleRange((i, j) => fShape.ModifyTwoLasts((a, b) =>
            {
                a.n = i;
                b.n = j;
            }).ToShape(5000, 0.01).MoveX(2 * j).MoveY(2 * i));

            //sps = fShape.ToShapes(5000, 0.01);

            //Shape debugShape = Shape.Empty;
            //try
            //{
            //    var sps = fShape.ToShapes(100, 0.01, 0.001);
            //}
            //catch (DebugException<Vector2[]> e)
            //{
            //    debugShape = new Polygon()
            //    {
            //        Points = e.Value
            //    }.ToShape().Rotate(Math.PI / 2).Adjust(0.8).ToNumSpots3(0.2);
            //}

            //var sps = fShape.ToShapes(3000, null).Select(s => s.ToLines(0.3));

            var shape = new Shape[]
            {
                //debugShape,
                //fShape.ToNumShape(100, 0.1).ApplyColor(Color.Blue),

                sps.ToSingleShape().ApplyColor(Color.Red),
                //fShape.ToLineShape(5000, 0.3).MoveZ(0.005).ApplyColor(Color.Blue),

                //sps.ToBlowedShape(1.05).ApplyColor(Color.Red),
                //fShape.ToNumShape(100, 0.1).MoveZ(0.01).ApplyColor(Color.Blue),
                //fShape.ToLineShape(4567, 0.3).MoveZ(-0.01).ApplyColor(Color.Blue),
                //sps.ToSingleShape().ApplyColor(Color.Blue),
                //sps.SelectWithIndex((s, i) => s.MoveZ(-i * 0.02)).ToSingleShape().ApplyColor(Color.DarkGreen),
                //fShape.ToFormulaShape().Perfecto(2).ScaleX(0.6).MoveY(-0.5).ApplyColor(Color.Blue),

                //Shapes.CoodsNet
            }.ToSingleShape();

            return shape;//.Rotate(Rotates.Z_Y);
        }
    }
}
