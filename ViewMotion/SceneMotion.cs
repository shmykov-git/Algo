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
        return new BulletMotionExample2().Scene();

        //return Shapes.NativeCubeWithCenterPoint.Centered().ToMetaShape3(1, 1, Color.Red, Color.Blue).ToMotion(2);

        var rnd = new Random();

        var sceneCount = 1000;
        var n = 50;
        var activeRadius = 5;
        var brokenRadius = 50;
        var k = 0.05;
        var aCoef = k * 1;
        var gCoef = k * 1;
        var bulletForceCoef = k * 0.005;  // fix bullet interactive force
        var bulletBorderCoef = -0.4;      // depends on bullet.radius
        var dampingCoef = 0.8;
        var forceBorder = 0.5;
        var gravityCoef = 0.0001;
        var gravity = new Vector3(0, -1, 0);
        var rotate = -0.2;

        var fixZPos = 14;
        var bullet = new PointObject
        {
            position = new Vector3(0, 0, 1.5),
            speed = new Vector3(0, 0, 0),
            mass = 30,
            radius = 1,
        };

        //var aim = (5, 5, 5).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered().MoveZ(50);
        var block = (n, n, 1).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered()
            .Mult(0.05)
            .PullOnSurface(SurfaceFuncs.Paraboloid)
            .Mult(20)
            .Where(v=>v.z < fixZPos + 1.5)

        //var block = (n, n, 1).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D()
        //    .AlignZ(0.5)
        //    .Adjust(2 * Math.PI)
        //    .Move(2,2,0)
        //    .ToOyM()
        //    .Transform(TransformFuncs3.CylinderWrapZR(Funcs.BackParabola(0.18))).NormalizeWith2D()
        //    .ToOy()
        //    .Mult(0.18)
        //    .PullOnSurface(SurfaceFuncs.Paraboloid)
        //    .Mult(2 / 0.18)
        //.ToLines(10).ApplyColor(Color.Blue)
        //+ Shapes.CoodsWithText
        //+ Surfaces.Plane(2 * n, 2 * n).Centered().Where(v => v.ToV2().Len > 10).MoveZ(fixZPos).ToLines(5, Color.Blue)
        ;

        //return block.ToMotion();
        //return block.ToMetaShape3(5, 5, Color.Blue, Color.Green).ToMotion(n * 2);

        //return block.ToMetaShape3(1,1,Color.Red,Color.Green).ToMotion(2);

        var ps = block.Points3;

        //var block = Shapes.Line.Centered();
        var nodes = block.PointIndices.Select(i => new Node() 
        { 
            i = i,
            position = ps[i]
        }).ToArray();
        nodes.ForEach(n => n.ns = block.Links[n.i].ToList());
        nodes.ForEach(n => n.locked = fixZPos < n.position.z && n.position.z < 40);

        block = block.RotateOx(rotate);
        ps = block.Points3;
        nodes.ForEach(n => n.position = ps[n.i]);

        block = block.Normalize();
        var net = new Net3<Node>(nodes, activeRadius);

        //block = block.TransformPoints(p => p += 0.1 * new Vector3(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()));

        Func<double, double> blockForceFn = d =>
        {
            if (d < forceBorder)
                d = forceBorder;

            return -aCoef / d.Pow4() + gCoef / d.Pow2();
        };

        Func<double, double> bulletForceFn = d =>
        {
            var a = bulletBorderCoef - bullet.radius;

            if (d + a < forceBorder)
                d = forceBorder - a;

            return -bulletForceCoef / (d + a).Pow4();
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
            net.SelectItemsByRadius(bullet.position, activeRadius)
                .ForEach(n =>
                {
                    var d = (n.position - bullet.position).Length;
                    var ds = 0.5 * (bullet.mass + n.mass) * bulletForceFn(d);
                    n.speed -= ds * (n.position - bullet.position) / (d * n.mass);
                    bullet.speed += ds * (n.position - bullet.position) / (d * bullet.mass);
                });
            bullet.speed += gravityCoef * gravity;

            nodes.Where(n=>!n.locked).ForEach(n => n.speed = CalcSpeed(n.position, n.speed, n.ns.Select(j => nodes[j].position)));
            nodes.Where(n => !n.locked).ForEach(n => n.speed += gravityCoef * gravity);

            nodes.Where(n => !n.locked).ForEach(n => n.position += n.speed);
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
            Convexes = block.Convexes
            //Convexes = nodes.SelectMany(n => n.ns.Select(j => (n.i, j).OrderedEdge())).Distinct().Select(v => v.EdgeToArray()).ToArray(),
        };

        var border = Surfaces.Torus(60, 10, 11).Perfecto(37).MoveZ(fixZPos + 0.5).RotateOx(rotate).ApplyColor(Color.SaddleBrown);
        var bulletShape = Shapes.IcosahedronSp3.Perfecto(2 * bullet.radius).ApplyColor(Color.Red);
        //var surfaceShape = Surfaces.Plane(2 * n, 2 * n).Centered().Where(v => v.ToV2().Len > 10).MoveZ(fixZPos).ToLines(5, Color.Blue);

        IEnumerable<Shape> Animate()
        {
            for (var i = 0; i < sceneCount; i++)
            {
                yield return new[]
                {
                    GetBlock(i).ApplyColor(Color.Blue),//.ToMetaShape3(5, 5, Color.Blue, Color.Green),
                    bulletShape.Move(bullet.position),
                    border,
                    //surfaceShape,
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