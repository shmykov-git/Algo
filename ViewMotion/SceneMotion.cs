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
using System.Threading.Tasks.Sources;
using Model.Graphs;

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

    class Node /*: INet3Item*/
    {
        public int i;
        public List<Edge> edges;
        public Vector3 position;
        public Vector3 speed = Vector3.Origin;
        public double speedY = 0;
        //public double mass = 1;
        //public bool locked;
        //public Func<Vector3> PositionFn => () => position;
    }

    class Edge
    {
        public int i;
        public int j;
        public double fA;
        public double fC;
    }

    public Task<Motion> Scene()
    {
        var sceneCount = 2000;
        var dampingCoef = 0.8;
        var gravity = new Vector3(0, -0.00001, 0);
        var stepsPerScene = 10;
        var rotationAngleX = 0;// Math.PI / 6;
        var rotationSpeed = 0;// 0.001;
        var move = new Vector3(0, 30, 0);
        var fixBottom = false;
        var useDeformation = false;
        var thickness = 3;
        double blowPower = 10;
        double materialPower = 10;

        var blockLine = (thickness).SelectRange(z => Shapes.PerfectCubeWithCenter.MoveZ(z)).ToSingleShape().NormalizeWith2D();
        //var block = vectorizer.GetPixelShape("hh3").Points3.Select(p => blockLine.Move(p)).ToSingleShape().NormalizeWith2D().Centered();
        //block = block.Where(v => v.Length <= 4).NormalizeWith2D();

        var block = Surfaces.Shell2(30, 10, -0.5, 0.5, 1.8, 3.2).WithCenterPoint().Perfecto(100).AlignY(0);



        //var block = Solids.Sphere(10, 10, 5).Mult(20);

        //return block.ToMetaShape3().ToMotion();

        if (useDeformation)
            block = block.Mult(0.03).PullOnSurface(SurfaceFuncs.Hyperboloid).Mult(1/0.03);

        var bY = block.BorderY;

        block=block.RotateOx(rotationAngleX).Move(move);

        var ps = block.Points3;

        var nodes = block.PointIndices.Select(i => new Node() 
        { 
            i = i,
            position = ps[i]
        }).ToArray();

        var nLast = nodes.Length - 1;
        nodes.ForEach(n => n.edges = block.Links[n.i].Select(j => new Edge
        {
            i = n.i,
            j = j,
            fA = (n.position - nodes[j].position).Length,
            fC = j == nLast ? blowPower : materialPower
        }).ToList());

        //nodes.ForEach(n => n.ns = block.Links[n.i].ToList());
        //nodes.ForEach(n => n.fAs = n.ns.Select(j => (n.position - nodes[j].position).Length).ToList());
        nodes.ForEach(n => n.speed = rotationSpeed * n.position.ZeroY().MultV(Vector3.YAxis));

        Debug.WriteLine(nodes.SelectMany(n=>n.edges.Select(e=>e.fA)).Average());
        //var a = 0.933;
        //var b = 1;
        var forcePower = 0.001;
        var forceBorder = 0.75;
        double BlockForceFn(double c, double a, double y)
        {
            var x = y / a;

            if (x < forceBorder)
                x = forceBorder;

            return forcePower * c * a * (x - 1) * (x + 1) / x.Pow4();
        };

        //var bounceCoef = 0.2;
        Vector3 CalcSpeed(Node n)
        {
            var p0 = n.position;
            Vector3 offset = Vector3.Origin;

            foreach (var e in n.edges)
            {
                var sn = nodes[e.j];
                var p = sn.position;

                var d = (p - p0).Length;
                var ds = BlockForceFn(e.fC, e.fA, d);

                offset += ds * (p - p0) / d;
            }

            var speed = n.speed + offset * dampingCoef;

            if (IsBottom(n) && speed.y < 0)
            {
                n.speedY += - speed.y;
                speed = speed.SetY(0);
            }
            else
            {
                n.speedY = 0;
            }

            return speed;
        }

        Vector3 CalcBounceSpeedOffset(Node n)
        {
            Vector3 offset = Vector3.Origin;
            var sns = n.edges.Select(e => nodes[e.j]).Where(IsBottom).ToArray();

            if (sns.Length == 0)
                return offset;

            var sumY = sns.Select(sn => n.position.y - sn.position.y).Sum();

            foreach (var (sn, i) in sns.Select((v, i) => (v, i)))
            {
                offset += (n.position - sn.position).ToLenWithCheck(n.speedY * (n.position.y - sn.position.y) / sumY);
            }

            return offset;
        }

        bool IsBottom(Node n) => n.position.y <= bY.a;
        bool CanCalc(Node n) => !fixBottom || n.position.y > bY.a;
        Vector3 FixY(Vector3 a) => a.y > bY.a ? a : new Vector3(a.x, bY.a, a.z);


        void Step()
        {
            nodes.Where(CanCalc).ForEach(n => n.speed += gravity);
            nodes.Where(CanCalc).ForEach(n => n.speed = CalcSpeed(n));
            //Debug.WriteLine(speedY);
            //nodes.ForEach(n => n.speed += new Vector3(0, speedY/nodes.Length, 0));
            nodes.Where(CanCalc).Where(n => !IsBottom(n)).ForEach(n => n.speed += CalcBounceSpeedOffset(n));
            nodes.Where(CanCalc).ForEach(n => n.position += n.speed);
            nodes.Where(CanCalc).ForEach(n => n.position = FixY(n.position));
        }

        Shape GetBlock(int i) => new Shape
        {
            Points3 = nodes.Select(n => n.position).ToArray(),
            Convexes = block.Convexes
        };

        var platform = Surfaces.Plane(10, 10).ToOy().Perfecto(300).MoveY(bY.a).ToLines(50, Color.Black);
        //var platform = Shapes.CirclePlatform().ApplyColor(Color.FromArgb(64,0,0)).Mult(150).ScaleY(0.2);
        var coods = Shapes.Coods.Mult(25).MoveY(bY.a).ApplyColor(Color.Black);

        //(1000).ForEach(_ => Step());

        //return GetBlock(0).ToMetaShape3(1, 1, Color.Blue, Color.Red).ToMotion();

        IEnumerable<Shape> Animate()
        {
            for (var i = 0; i < sceneCount; i++)
            {
                yield return new[]
                {
                    GetBlock(i).Normalize()/*.ApplyColor(Color.Blue)*/.ToMetaShape3(50, 100, Color.Blue, Color.Red),
                    //coods,
                    platform
                }.ToSingleShape();


                (stepsPerScene).ForEach(_=>Step());
            }
        }

        return Animate().ToMotion(250);
    }
}