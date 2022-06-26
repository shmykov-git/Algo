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

            var nStones = 8;
            var stones = (nStones).SelectRange(i => Shapes.Stone(4, i, 0.5, 4).AlignY(0).Move(10*(i, nStones).ToCircleYV3())).ToSingleShape();
            var stoneColliders = (nStones).SelectRange(i => Shapes.Stone(4, i, 0.5, 1).AlignY(0).Move(10 * (i, nStones).ToCircleYV3())).ToSingleShape();

            return new Shape[]
            {
                stones.ApplyColor(Color.Black),
                stoneColliders.ToLines(2).ApplyColor(Color.Green),
            }.ToSingleShape();
        }
    }
}
