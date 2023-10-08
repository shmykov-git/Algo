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
        //return Shapes.NativeCubeWithCenterPoint.Centered().ToMetaShape3(1, 1, Color.Red, Color.Blue).ToMotion(2);

        var rnd = new Random();

        var n = 7;
        var bulletRadius = 2;
        var brokenRadius = 3;
        var k = 0.01;
        var aCoef = k * 1;
        var gCoef = k * 1;
        var dampingCoef = 0.8;
        var forceBorder = 0.7;

        var bullet = new PointObject
        {
            position = new Vector3(-9, 0, 0),
            speed = new Vector3(0.5, 0, 0),
            mass = 10*4,
            radius = 0.5
        };

        var block = (n, n, n).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered();
        //return block.ToMetaShape3(5, 5, Color.Blue, Color.Green).ToMotion(n * 2);
        var ps = block.Points3;

        //var block = Shapes.Line.Centered();
        var nodes = block.PointIndices.Select(i => new Node() 
        { 
            i = i,
            position = ps[i]
        }).ToArray();
        nodes.ForEach(n => n.ns = block.Links[n.i].ToList());

        var net = new Net3<Node>(nodes, bulletRadius);

        //block = block.TransformPoints(p => p += 0.1 * new Vector3(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()));

        Func<double, double> blockForceFn = d =>
        {
            if (d < forceBorder)
                d = forceBorder;

            return -aCoef / d.Pow4() + gCoef / d.Pow2();
        };

        Func<double, double> bulletForceFn = d =>
        {
            var a = -bullet.radius;

            if (d + a < forceBorder)
                d = forceBorder - a;

            return -k / (d + a).Pow4();
        };

        Vector3 CalcSpeed(Vector3 p0, Vector3 s0, IEnumerable<Vector3> ps)
        {
            Vector3 offset = Vector3.Origin;

            foreach(var p in ps)
            {
                var d = (p - p0).Length;
                var ds = blockForceFn(d);

                offset += ds * (p - p0) / d;
            }

            //Debug.WriteLine($"{(p0-points.First()).Length}");

            return s0 + offset * dampingCoef;
        }

        bool IsBroken(Vector3 a, Vector3 b) => (b - a).Length2 > brokenRadius * brokenRadius;

        void Step()
        {
            net.SelectItemsByRadius(bullet.position, bulletRadius)
                .ForEach(n =>
                {
                    var d = (n.position - bullet.position).Length;
                    var ds = 0.5 * (bullet.mass + 1) * bulletForceFn(d);
                    n.speed -= ds * (n.position - bullet.position) / d;
                    bullet.speed += ds * (n.position - bullet.position) / (d * bullet.mass);
                });

            nodes.ForEach(n => n.speed = CalcSpeed(n.position, n.speed, n.ns.Select(j => nodes[j].position)));

            nodes.ForEach(n => n.position += n.speed);
            bullet.position += bullet.speed;

            nodes.ForEach(n => n.ns.ToArray().ForEach(j =>
            {
                if (IsBroken(n.position, nodes[j].position))
                {
                    n.ns.Remove(j);
                }
            }));
        }

        Shape GetBlock(int i) => new Shape
        {
            Points3 = nodes.Select(n => n.position).ToArray(),
            Convexes = nodes.SelectMany(n => n.ns.Select(j => (n.i, j).OrderedEdge())).Distinct().Select(v => v.EdgeToArray()).ToArray(),
        };

        IEnumerable<Shape> Animate()
        {
            for (var i = 0; i < 1000; i++)
            {
                yield return new[] 
                {
                    GetBlock(i).ToMetaShape3(5, 5, Color.Blue, Color.Green),
                    Shapes.IcosahedronSp3.Perfecto(bullet.radius).Move(bullet.position).ApplyColor(Color.Red),
                    Surfaces.Plane(2*n, 2*n).Centered().ToOx().ToLines(3, Color.White)
                    //Shapes.CoodsWithText.Mult(10)
                }.ToSingleShape();
                
                Step();
            }
            //return (101).Range(i => GetQ(i / 100.0)).Select(GetShape);
            //yield return vectorizer.GetContentShape("cat1").ApplyColor(Color.Black);
            //return (75).SelectRange(i => vectorizer.GetContentShape("t5", new ShapeOptions() { ZVolume = 0.02, ColorLevel = 50 + 2*i }).ApplyColor(Color.Red));
        }

        return Animate().ToMotion(n*2);
    }
}