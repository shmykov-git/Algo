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
            var fShape = new Fr[]
                {(-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 2), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1)};
            var s = fShape.ToShape(3000, 0.02).ApplyColor(Color.Red);

            var shape = new Shape[]
            {
               s,
               fShape.ToLineShape(3000).MoveZ(0.01),
                //Shapes.ArrowCoods.Mult(5)
            }.ToSingleShape();

            return shape;//.Rotate(Rotates.Z_Y);
        }
    }
}
