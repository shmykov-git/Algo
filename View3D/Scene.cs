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
            //var fShape = new Fr[]
            //    {(-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 2), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1)};

            //var s = fShape.ToShape(3000, 0.02).ApplyColor(Color.Red);

            //var s = Polygons.Square.PutInside(Polygons.Spiral(15, 979).Mult(1.23)).MakeShape().Perfecto().(0.01);//.Transform(Multiplications.Cube);
            //var s = Surfaces.Cylinder(8, 61).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.RootPolinomY(1.0 / 20, new[] { -3, -2, -0.5, 0, 1.1, 2.2, 3 })) + Shapes.Cube;
            //var s = Surfaces.DiniSurface(100, 50).ToLines(2).Rotate(Rotates.Z_Y); // var shape = Surfaces.DiniSurface(120, 30).MassCentered().Normed().Move(0, 0, 1).ToLines(0.2, Color.Blue)
            //var s = Surfaces.MobiusStrip(512, 80).ToMaze(0, MazeType.SimpleRandom).ToLines().Rotate(Rotates.Z_Y).ApplyColor(Color.Black);

            var mb = MandelbrotFractalSystem.GetPoints(10, 0.0019, 1000);

            var s1 = mb.ToShape().ToLines(0.2).ScaleZ(15).ApplyColor(Color.Blue) + Shapes.Ball.Mult(0.1).ApplyColor(Color.Red);

            var s2 = mb.ToShape(0.01).ApplyColor(Color.Green);//.ToShape(0.02).ApplyColor(Color.Red);


            var shape = new Shape[]
            {
               s1, s2,
               //fShape.ToLineShape(3000).MoveZ(0.01),
                //Shapes.ArrowCoods.Mult(5)
            }.ToSingleShape();

            return shape;//.Rotate(Rotates.Z_Y);
        }
    }
}
