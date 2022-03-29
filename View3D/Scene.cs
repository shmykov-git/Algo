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

            //var s = fShape.ToShape(3000, 0.02, indices: new[] { 0 }).ApplyColor(Color.Red);

            var mb = MandelbrotFractalSystem.GetPoints(2, 0.002, 1000);

            //var s1 = mb.ToShape().ToLines().ApplyColor(Color.Red);// + Shapes.Ball.Mult(0.1).ApplyColor(Color.Red);


            var s = mb.ToPolygon().ToTriangulatedShape(40, 0.05).Perfecto().ApplyZ(Funcs3Z.Hyperboloid).ToLines(0.1, Color.Blue);//.ApplyColor(Color.Blue); 
           
                

            //var s4 = net.Cut(mb.ToPolygon()).ToLines(0.5).ApplyColor(Color.Blue);

            var shape = new Shape[]
            {
               s, 
               
                //Shapes.CoodsWithText
            }.ToSingleShape();

            return shape;
        }
    }
}
