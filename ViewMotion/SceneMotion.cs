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
        public Edge[] edges;
        public Pln[] planes;
        public Vector3 position0;
        public Vector3 position;
        public Vector3 speed = Vector3.Origin;
        public double speedY = 0;
        //public double mass = 1;
        //public bool locked;
        //public Func<Vector3> PositionFn => () => position;
    }

    enum EdgeType
    {
        Material,
        Skeleton
    }

    class Edge
    {
        public int i;
        public int j;
        public double fA;
        public EdgeType type = EdgeType.Material;
    }

    class Pln
    {
        public int i;
        public int j;
        public int k;
    }

    public Task<Motion> Scene()
    {
        var sceneCount = 2000;
        var showMeta = false;
        var color1 = Color.SaddleBrown;
        var color2 = Color.Red;
        var dampingCoef = 0.8;
        var gravity = new Vector3(0, -0.0001, 0); // no wind
        var stepsPerScene = 10;
        var rotationAngleX = 0;// Math.PI / 6;
        var rotationSpeed = 0; // 0.01;
        var move = new Vector3(0, 15, 0);
        var fixBottom = false;
        var useDeformation = false;
        var useSkeleton = true;
        var thickness = 3;

        double blowPower = 0.00005;
        var useBlowUp = false;
        double blowUpPowerStep = 0.1 * blowPower / stepsPerScene;
        int blowUpPowerStepNum = 100 * stepsPerScene;

        double skeletonPower = 1;
        double materialPower = 1;
        double frictionForce = 0.006;
        double clingForce = 0.0075;

        var blockLine = (thickness).SelectRange(z => Shapes.PerfectCubeWithCenter.MoveZ(z)).ToSingleShape().NormalizeWith2D();
        //var block = vectorizer.GetPixelShape("hh3").Points3.Select(p => blockLine.Move(p)).ToSingleShape().NormalizeWith2D().Centered();
        //block = block.Where(v => v.Length <= 4).NormalizeWith2D();

        var block = Shapes.Stone(4, 4, 1, 2).Normalize().Perfecto(50).AlignY(0);

        if (useSkeleton)
            block = block.WithCenterPoint();

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
            position0 = ps[i],
            position = ps[i]
        }).ToArray();

        var nLast = nodes.Length - 1;
        nodes.ForEach(n => n.edges = block.Links[n.i].Select(j => new Edge
        {
            i = n.i,
            j = j,
            fA = (n.position - nodes[j].position).Length,
            type = (useSkeleton && nLast == j) ? EdgeType.Skeleton : EdgeType.Material
        }).ToArray());

        nodes.ForEach(n => n.planes = block.Convexes.Where(c=>c.Length >= 3).Where(c => c.Any(j => n.i == j)).Select(c => new Pln() { i = c[0], j = c[1], k = c[2] }).ToArray());
        //nodes.ForEach(n => n.ns = block.Links[n.i].ToList());
        //nodes.ForEach(n => n.fAs = n.ns.Select(j => (n.position - nodes[j].position).Length).ToList());
        nodes.ForEach(n => n.speed = rotationSpeed * n.position.ZeroY().MultV(Vector3.YAxis));

        Debug.WriteLine(nodes.SelectMany(n=>n.edges.Select(e=>e.fA)).Average());
        //var a = 0.933;
        //var b = 1;
        var forcePower = 0.1;
        var forceBorder = 0.75;
        double BlockForceFn(double c, double a, double y)
        {
            var x = y / a;

            if (x < forceBorder)
                x = forceBorder;

            return forcePower * c * (x - 1) * (x + 1) / x.Pow4();
        };

        //Quaternion GetRotation(Node n, Vector3 center0, Vector3 center)
        //{
        //    var a = (nodes[n.i].position0 - center0).Normalize();
        //    var b = (nodes[n.i].position - center).Normalize();

        //    return Quaternion.FromRotation(a, b);
        //}

        Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) => (a - c).MultV(b - c);

        Vector3 BlowForce(Node n) => n.planes.Select(p => GetNormal(nodes[p.i].position, nodes[p.j].position, nodes[p.k].position)).Center()/*.Normalize()*/;

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

                var fc = e.type switch
                {
                    EdgeType.Skeleton => skeletonPower,
                    _ => materialPower,
                };

                var ds = BlockForceFn(fc, e.fA, d);

                offset += ds * (p - p0) / d;
            }

            var speed = n.speed + offset * dampingCoef;

            if (IsBottom(n))
            {
                if (speed.y < 0)
                {
                    n.speedY += -speed.y;
                    speed = speed.SetY(0);

                    var fForce = -speed.ToLenWithCheck(frictionForce);
                    speed = fForce.Length2 > speed.Length2
                        ? Vector3.Origin
                        : speed + fForce;
                }
                else
                {
                    n.speedY = 0;

                    var clForce = -Vector3.YAxis.ToLenWithCheck(clingForce);
                    speed = clForce.Length2 > speed.VectorY().Length2
                        ? Vector3.Origin
                        : speed + clForce;
                }
            }

            return speed;
        }

        Vector3 CalcBlowSpeedOffset(Node n)
        {
            if (blowPower > 0 && n.planes.Length > 0)
                return blowPower * BlowForce(n);
            else
                return Vector3.Origin;
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

        var nStep = 0;
        void Step()
        {
            if (useBlowUp)
            {
                if (nStep > blowUpPowerStepNum)
                    blowPower += blowUpPowerStep;
            }

            nodes.Where(CanCalc).ForEach(n => n.speed += gravity);
            nodes.Where(CanCalc).ForEach(n => n.speed += CalcBlowSpeedOffset(n));
            nodes.Where(CanCalc).ForEach(n => n.speed = CalcSpeed(n));
            //Debug.WriteLine(speedY);
            //nodes.ForEach(n => n.speed += new Vector3(0, speedY/nodes.Length, 0));
            nodes.Where(CanCalc).Where(n => !IsBottom(n)).ForEach(n => n.speed += CalcBounceSpeedOffset(n));
            nodes.Where(CanCalc).ForEach(n => n.position += n.speed);
            nodes.Where(CanCalc).ForEach(n => n.position = FixY(n.position));

            nStep++;
        }

        var normalizedBlock = block.Normalize();
        var nodesNoCenter = nodes.SkipLast(1).ToArray();
        var center0 = nodesNoCenter.Select(n => n.position0).Center();

        Shape GetBlock(int i)
        {
            //var center = nodesNoCenter.Select(n=>n.position).Center();
            //var p = nodesNoCenter[37].position - center;
            //var p0 = nodesNoCenter[37].position0 - center0;
            //var q = Quaternion.FromRotation(p0.Normalize(), p.Normalize());
            //var q = nodesNoCenter.Skip(37).Take(1).Select(n=>GetRotation(n,center0, center)).Aggregate((a, b) => a + b) / nodesNoCenter.Length;

            //var blockShape = new Shape
            //{
            //    Points3 = nodesNoCenter.Select(n => q * (n.position0 - center0) + center).ToArray(),
            //    Convexes = normalizedBlock.Convexes
            //};

            var modelShape = new Shape
            {
                Points3 = nodes.Select(n => n.position).ToArray(),
                Convexes = normalizedBlock.Convexes
            };

            return /*blockShape.ApplyColor(color1) + */modelShape.ApplyColor(color2);
        }

        var platform = Surfaces.Plane(20, 20).ToOy().Perfecto(100).MoveY(bY.a).ToLines(30, Color.Black);
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
                    showMeta 
                        ? GetBlock(i).ToMetaShape3(30, 60, color1, color2)
                        : GetBlock(i)/*.ApplyColor(color1)*/,
                    //coods,
                    platform
                }.ToSingleShape();


                (stepsPerScene).ForEach(_ => Step());
            }
        }

        return Animate().ToMotion(50);
    }
}