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
        public List<int> ns;
        public List<double> fAs;
        public Vector3 position;
        public Vector3 speed = Vector3.Origin;
        public double speedY = 0;
        //public double mass = 1;
        //public bool locked;
        //public Func<Vector3> PositionFn => () => position;
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
        var gravity = new Vector3(0, -0.00005, 0);
        var stepsPerScene = 10;
        var rotationAngleX = Math.PI / 6;
        var rotationSpeed = 0;// 0.005;
        var move = new Vector3(0, 3, 0);
        var fixBottom = false;
        var useDeformation = false;

        var blockLine = (3).SelectRange(z => Shapes.PerfectCubeWithCenter.MoveZ(z)).ToSingleShape().NormalizeWith2D();
        var block = vectorizer.GetPixelShape("hh3").Points3.Select(p => blockLine.Move(p)).ToSingleShape().NormalizeWith2D().Centered();
        
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
        nodes.ForEach(n => n.ns = block.Links[n.i].ToList());
        nodes.ForEach(n => n.fAs = n.ns.Select(j => (n.position - nodes[j].position).Length).ToList());
        nodes.ForEach(n => n.speed = rotationSpeed * n.position.ZeroY().MultV(Vector3.YAxis));

        //var a = 0.933;
        var b = 1;
        var c = 0.1;
        var forceBorder = 0.65;
        double BlockForceFn(double a, double x)
        {
            if (x < forceBorder)
                x = forceBorder;

            return c * (x - a) * (x + b) / x.Pow4();
        };

        //var bounceCoef = 0.2;
        Vector3 CalcSpeed(Node n)
        {
            var p0 = n.position;
            Vector3 offset = Vector3.Origin;

            foreach (var (j, num) in n.ns.Select((j, num)=>(j, num)))
            {
                var sn = nodes[j];
                var p = sn.position;

                var d = (p - p0).Length;
                var ds = BlockForceFn(n.fAs[num], d);

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
            var sns = n.ns.Select(j => nodes[j]).Where(IsBottom).ToArray();

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

        var platform = Surfaces.Plane(10, 10).ToOy().Perfecto(100).MoveY(bY.a).ToLines(20, Color.DarkRed);// Shapes.CirclePlatformWithLines(platformColor:Color.FromArgb(64,0,0)).Mult(50);
        var coods = Shapes.Coods.Mult(25).MoveY(bY.a).ApplyColor(Color.Black);

        //(1000).ForEach(_ => Step());

        //return GetBlock(0).ToMetaShape3(1, 1, Color.Blue, Color.Red).ToMotion();

        IEnumerable<Shape> Animate()
        {
            for (var i = 0; i < sceneCount; i++)
            {
                yield return new[]
                {
                    GetBlock(i).ApplyColor(Color.Blue), //.ToMetaShape3(1, 1, Color.Blue, Color.Red),
                    //coods,
                    platform
                }.ToSingleShape();


                (stepsPerScene).ForEach(_=>Step());
            }
        }

        return Animate().ToMotion(block.Size.Length * 1.5);
    }
}