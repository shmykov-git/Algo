using System.Threading.Tasks;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Models;
using Vector3 = Model3D.Vector3;
using Model.Tools;
using System.Linq;
using System.Collections.Generic;
using Shape = Model.Shape;
using Color = System.Drawing.Color;
using Model3D.Tools.Vectorization;
using Model3D;
using System;
using static ViewMotion.SceneMotion.MaterialSceneMotionExample;

namespace ViewMotion;

partial class SceneMotion // MaterialActiveWorld
{
    public Task<Motion> MaterialActiveWorld() => new MaterialSceneMotionExample(vectorizer).Scene();
    public Task<Motion> BulletCatchMotion() => new BulletCatchMotionExample().Scene();
    public Task<Motion> BulletThrowMotion() => new BulletThrowMotionExample().Scene();

    public class MaterialSceneMotionExample
    {
        private readonly Vectorizer vectorizer;

        public MaterialSceneMotionExample(Vectorizer vectorizer)
        {
            this.vectorizer = vectorizer;
        }

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
            var sceneCount = 500;
            var dampingCoef = 0.8;
            var frictionForce = 0.001;
            var gravity = new Vector3(0, -0.00001, 0);
            var stepsPerScene = 40;
            var rotationAngleX = 0; // Math.PI / 6;
            var rotationSpeed = 0; // 0.005;
            var moveY = 0;
            var move = new Vector3(0, moveY, 0);
            var fixBottom = false;
            var useDeformation = false;
            var color = Color.Green;

            var blockLine = (3).SelectRange(z => Shapes.PerfectCubeWithCenter.MoveZ(z)).ToSingleShape().NormalizeWith2D();
            var block = vectorizer.GetPixelShape("hh1").Points3.Select(p => blockLine.Move(p)).ToSingleShape().NormalizeWith2D().Centered();
            //return block.Perfecto(30).ToMeta().ToMotion(30);
            if (useDeformation)
                block = block.Mult(0.03).PullOnSurface(SurfaceFuncs.Hyperboloid).Mult(1 / 0.03);

            var bY = block.BorderY;

            block = block.RotateOx(rotationAngleX).Move(move);

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

                foreach (var (j, num) in n.ns.Select((j, num) => (j, num)))
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
                    n.speedY += -speed.y;
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

            Vector3 CalcBottomSpeedOffset(Node n)
            {
                var planeSpeed = n.speed.SetY(0);
                var frictionOffset = planeSpeed.Length < frictionForce ? -planeSpeed : -planeSpeed.ToLenWithCheck(frictionForce);

                return frictionOffset;
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
                nodes.Where(CanCalc).Where(n => IsBottom(n)).ForEach(n => n.speed += CalcBottomSpeedOffset(n));
                nodes.Where(CanCalc).ForEach(n => n.position += n.speed);
                nodes.Where(CanCalc).ForEach(n => n.position = FixY(n.position));
            }

            Shape GetBlock(int i)
            {
                var (bi, ns) = nodes.WhereBi(n => n.ns.Count <= 12);

                var s = new Shape
                {
                    Points3 = ns.Select(n => n.ns.Where(i => bi[i] != -1).Select(i => nodes[i].position).Center()).ToArray(),
                    Convexes = block.Convexes.ApplyConvexBi(bi).CleanBi(true)
                };

                return s;
            }

            var platform = Surfaces.Plane(10, 10).ToOy().Perfecto(100).MoveY(bY.a).ToLines(20, Color.DarkRed);// Shapes.CirclePlatformWithLines(platformColor:Color.FromArgb(64,0,0)).Mult(50);
                                                                                                              //var coods = Shapes.Coods().Mult(25).MoveY(bY.a).ApplyColor(Color.Black);

            //(1000).ForEach(_ => Step());

            //return GetBlock(0).ToMetaShape3(1, 1, Color.Blue, Color.Red).ToMotion();

            IEnumerable<Shape> Animate()
            {
                for (var i = 0; i < sceneCount; i++)
                {
                    yield return new[]
                    {
                    GetBlock(i).ApplyColor(color), //.ToMetaShape3(1, 1, Color.Blue, Color.Red),
                    //coods,
                    platform
                }.ToCompositeShape();


                    (stepsPerScene).ForEach(_ => Step());
                }
            }

            return Animate().ToMotion(/*block.Size.Length * 1.5*/);
        }
    }

    public class BulletThrowMotionExample
    {
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
            var rnd = new Random();

            var sceneCount = 1500;
            var showEach = 3;
            var n = 50;
            var activeRadius = 5;
            var brokenRadius = 50;
            var k = 0.05;
            var aCoef = k * 1;
            var gCoef = k * 1;
            var bulletForceCoef = k * 0.005;  // fix bullet interactive force
            var bulletBorderCoef = -0.4;      // depends on bullet.powerRadius
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

            //var aim = (5, 5, 5).SelectRange((iMp, j, k) => Shapes.NativeCubeWithCenterPoint.Move(iMp, j, k)).ToSingleShape().NormalizeWith2D().Centered().MoveZ(50);
            var block = (n, n, 1).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered()
                .Mult(0.05)
                .PullOnSurface(SurfaceFuncs.Paraboloid)
                .Mult(20)
                .Where(v => v.z < fixZPos + 1.5)
            //+ aim
            ;

            var ps = block.Points3;

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

                foreach (var p in ps)
                {
                    var d = (p - p0).Length;
                    var ds = blockForceFn(d);

                    offset += ds * (p - p0) / d;
                }

                return s0 + offset * dampingCoef;
            }

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

                nodes.Where(n => !n.locked).ForEach(n => n.speed = CalcSpeed(n.position, n.speed, n.ns.Select(j => nodes[j].position)));
                nodes.Where(n => !n.locked).ForEach(n => n.speed += gravityCoef * gravity);

                nodes.Where(n => !n.locked).ForEach(n => n.position += n.speed);
                bullet.position += bullet.speed;
            }

            Shape GetBlock(int i)
            {
                var (bi, ns) = nodes.WhereBi(n => n.ns.Count <= 12);

                var s = new Shape
                {
                    Points3 = ns.Select(n => n.ns.Where(i => bi[i] != -1).Select(i => nodes[i].position).Center()).ToArray(),
                    Convexes = block.Convexes.ApplyConvexBi(bi).CleanBi(true)
                };

                return s;
            }

            var border = Surfaces.Torus(60, 10, 11).Perfecto(37).MoveZ(fixZPos + 0.5).RotateOx(rotate).ApplyColor(Color.SaddleBrown);
            var bulletShape = Shapes.IcosahedronSp3.ApplyMasterPoint(Vector3.Origin).Perfecto(2 * bullet.radius).ApplyColor(Color.Red);

            IEnumerable<Shape> Animate()
            {
                for (var i = 0; i < sceneCount; i++)
                {
                    if (i % showEach == 0)
                        yield return new[]
                        {
                            GetBlock(i).ApplyColor(Color.Blue),//.ToMetaShape3(5, 5, Color.Blue, Color.Green),
                            bulletShape.Move(bullet.position),
                            border,
                        }.ToCompositeShape();

                    Step();
                }
            }

            return Animate().ToMotion(n * 2);
        }
    }

    public class BulletCatchMotionExample
    {
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
                mass = 10 * 4,
                radius = 0.5
            };

            var block = (n, n, n).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered();
            var ps = block.Points3;

            var nodes = block.PointIndices.Select(i => new Node()
            {
                i = i,
                position = ps[i]
            }).ToArray();
            nodes.ForEach(n => n.ns = block.Links[n.i].ToList());

            var net = new Net3<Node>(nodes, bulletRadius);

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

                foreach (var p in ps)
                {
                    var d = (p - p0).Length;
                    var ds = blockForceFn(d);

                    offset += ds * (p - p0) / d;
                }

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
                }.ToSingleShape();

                    Step();
                }
            }

            return Animate().ToMotion(n * 2);
        }
    }
}
