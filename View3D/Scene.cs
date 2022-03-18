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
            //var s = vectorizer.GetText("汉字", 300, multX: 5).ToLines(0.2, Color.Red);

            var th = 0.1;
            var h = 2.56;
            var dz = 7.46;
            var dx = 6.14;
            var dc = 0.64;

            var c1 = Color.FromArgb(185, 122, 86);
            var c2 = Color.FromArgb(120, 79, 55);
            var c3 = Color.FromArgb(199, 141, 108);
            var c4 = Color.White;

            var walls = new (Vector2 size, Vector2 move, Color c)[]
            {
                ((th, dz), (dx / 2, 0), c1),
                ((th, dz), (-dx / 2, 0), c1),
                ((th, 2.09), (1, -1+dc), c2),
                ((th, 2.1), (1.1, dz/2-1.05), c1),

                ((dx, th), (0, -dz / 2), c1),
                ((dx/4, th), (-3*dx/8, dc), c2),
                ((dx/6, th), (1*dx/8, dc), c2),
                ((2, th), (dx/2-1, dz/2-2.1), c1),

            };

            var s = walls.Select(w =>
                    Shapes.Cube.Scale(w.size.x, h, w.size.y).Move(w.move.x, h / 2, w.move.y).ApplyColor(w.c))
                .ToSingleShape();


            //var w1 = Shapes.Cube.Scale(th, h, dz).Align(0.5, 0, 0.5).MoveX(dx / 2).ApplyColor(Color.DarkSalmon);
            //var w2 = Shapes.Cube.Scale(th, h, dz).Align(0.5, 0, 0.5).MoveX(-dx / 2).ApplyColor(Color.DarkSalmon);
            //var w3 = Shapes.Cube.Scale(dx, h, th).Align(0.5, 0, 0.5).MoveZ(-dz / 2).ApplyColor(Color.DarkSalmon);

            //var w4 = Shapes.Cube.Scale(dx, h, th).Align(0.5, 0, 0.5).MoveZ(-dz / 2).ApplyColor(Color.DarkSalmon);

            var shape = new Shape[]
            {
               s,

               Surfaces.Plane(2, 2).Perfecto().ToOy().Scale(dx, th / 10, dz).Move(0, 0, 0).ApplyColor(c3),
               Surfaces.Plane(2, 2).Perfecto().ToOy().FlipY().Scale(dx, th / 10, dz).Move(0, h, 0).ApplyColor(c4),

                //Shapes.ArrowCoods.Mult(5)
            }.ToSingleShape();

            return shape;//.Rotate(Rotates.Z_Y);
        }
    }
}
