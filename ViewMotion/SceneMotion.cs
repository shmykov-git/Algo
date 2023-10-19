using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;
using Mapster.Utils;
using MathNet.Numerics;
using Meta;
using Model;
using Model.Extensions;
using Model.Fourier;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using Model3D.Tools.Model;
using Model4D;
using View3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Libraries;
using ViewMotion.Models;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Vector2 = Model.Vector2;
using Model.Tools;
using System.Drawing.Text;

namespace ViewMotion;


partial class SceneMotion
{
    #region ctor

    private readonly Vectorizer vectorizer;

    public SceneMotion(Vectorizer vectorizer)
    {
        this.vectorizer = vectorizer;
    }

    #endregion

    class Node : INet3Item
    {
        public int i;
        public List<int> ns;
        public Vector3 position;
        public Vector3 speed = Vector3.Origin;
        public double mass = 1;
        public bool locked;
        public Func<Vector3> PositionFn => () => position;
    }

    class PointObject
    {
        public Vector3 position;
        public Vector3 speed;
        public double mass;
        public double radius;
    }

    public Task<Motion> Scene()
    {
        var sceneCount = 2000;
        var dampingCoef = 0.8;
        var forceBorder = 0.75;
        var gravity = new Vector3(0, -0.00005, 0);
        var stepsPerScene = 10;
        var rotationSpeed = 0.001;
        var fixBottom = false;

        var blockLine = (3).SelectRange(z => Shapes.PerfectCubeWithCenter.MoveZ(z)).ToSingleShape().NormalizeWith2D();
        var block = vectorizer.GetPixelShape("hh2").Points3.Select(p => blockLine.Move(p)).ToSingleShape().NormalizeWith2D().Centered();
        var bY = block.BorderY;
        var ps = block.Points3;

        var nodes = block.PointIndices.Select(i => new Node() 
        { 
            i = i,
            position = ps[i]
        }).ToArray();
        nodes.ForEach(n => n.ns = block.Links[n.i].ToList());
        nodes.ForEach(n => n.speed = rotationSpeed * n.position.ZeroY().MultV(Vector3.YAxis));

        var a = 0.933;
        var b = 1;
        var c = 0.1;
        double BlockForceFn(double x)
        {
            if (x < forceBorder)
                x = forceBorder;

            return c * (x - a) * (x + b) / x.Pow4();
        };

        Vector3 CalcSpeed(Vector3 p0, Vector3 s0, IEnumerable<Vector3> ps)
        {
            Vector3 offset = Vector3.Origin;

            foreach(var p in ps)
            {
                var d = (p - p0).Length;
                var ds = BlockForceFn(d);

                offset += ds * (p - p0) / d;
            }

            return s0 + offset * dampingCoef;
        }

        bool CanCalc(Node n) => !fixBottom || n.position.y > bY.a;
        Vector3 FixY(Vector3 a) => a.y > bY.a ? a : new Vector3(a.x, bY.a, a.z);

        void Step()
        {
            nodes.Where(CanCalc).ForEach(n => n.speed = CalcSpeed(n.position, n.speed, n.ns.Select(j => nodes[j].position)));
            nodes.Where(CanCalc).ForEach(n => n.speed += gravity);
            nodes.Where(CanCalc).ForEach(n => n.position += n.speed);
            nodes.Where(CanCalc).ForEach(n => n.position = FixY(n.position));
        }

        Shape GetBlock(int i) => new Shape
        {
            Points3 = nodes.Select(n => n.position).ToArray(),
            Convexes = block.Convexes
        };

        IEnumerable<Shape> Animate()
        {
            for (var i = 0; i < sceneCount; i++)
            {
                yield return new[]
                {
                    GetBlock(i).ApplyColor(Color.Blue),
                }.ToSingleShape();


                (stepsPerScene).ForEach(_=>Step());
            }
        }

        return Animate().ToMotion(block.Size.Length * 1.5);
    }
}