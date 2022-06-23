using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Linq;
using MathNet.Numerics;

namespace Model3D.Libraries
{
    public static class Surfaces
    {
        public static Shape APowerB(int un, int vn, double from, double to) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.APowerB,
                UFrom = from,
                UTo =  to,
                UN = un,
                VFrom = from,
                VTo = to,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape NormalDistribution(int un, int vn, double mult, double mu, double sigma) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.NormalDistribution(mu, sigma, new Model.Vector2(-un * mult/2, -vn * mult/2)),
                UFrom = 0,
                UTo = un * mult,
                UN = un,
                VFrom = 0,
                VTo = vn * mult,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape Plane(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = (u, v) => new Vector3(u, v, 0),
                UFrom = 0,
                UTo = un,
                UN = un,
                VFrom = 0,
                VTo = vn,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape PlaneWithDiagonals(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = (u, v) => new Vector3(u, v, 0),
                UFrom = 0,
                UTo = un,
                UN = un,
                VFrom = 0,
                VTo = vn,
                VN = vn,
            }.GetPoints(),
            Convexes = Diagonals(vn, un)
        };

        public static Shape Sphere(int un, int vn, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Sphere,
                UFrom = 0,
                UTo = -2 * Math.PI,
                UN = un,
                VFrom = 0,
                VTo = Math.PI,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape SphereAngle(int un, int vn, double from, double to, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Sphere,
                UFrom = from,
                UTo = to,
                UN = un,
                VFrom = 0,
                VTo = Math.PI,
                VN = vn,
                UClosed = false,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape SphereAngle2(int un, int vn, double from, double to, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Sphere,
                UFrom = 0,
                UTo = 2*Math.PI,
                UN = un,
                VFrom = from,
                VTo = to,
                VN = vn,
                UClosed = false,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape SectionShapeY(Shape shape, int vn, double from, double to, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.CylinderShapeFuncY(shape),
                UFrom = 0,
                UTo = shape.PointsCount - 1,
                UN = shape.PointsCount,
                VFrom = from,
                VTo = to,
                VN = vn,
                VClosed = false,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, shape.PointsCount) : Squares(vn, shape.PointsCount)
        }.Normalize();

        public static Shape Heart(int un, int vn, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Heart,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = 0,
                VTo = Math.PI,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape Torus(int un, int vn, double a, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Torus(a),
                UFrom = 0,
                UTo = -2 * Math.PI,
                UN = un,
                VFrom = 0,
                VTo = 2 * Math.PI,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape HalfSphere2(int un, int vn, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Sphere,
                UFrom = - Math.PI * 0.2,
                UTo = - Math.PI*0.8,
                UN = un,
                VFrom = Math.PI * 0.2,
                VTo = Math.PI*0.8,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape HalfSphere(int un, int vn, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Sphere,
                UFrom = 0,
                UTo = - Math.PI,
                UN = un,
                VFrom = 0,
                VTo = Math.PI,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape Shamrock(int un, int vn, bool triangulate = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Shamrock,
                UFrom = 0,
                UTo = 4 * Math.PI,
                UN = un,
                VFrom = -Math.PI,
                VTo = Math.PI,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
        }.Normalize();

        public static Shape DiniSurface(int un, int vn, bool triangulate = false, bool bothFaces = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.DiniSurface,
                UFrom = 0,
                UTo = 4 * Math.PI,
                UN = un,
                VFrom = 0.005,
                VTo = 2,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un, bothFaces)
        }.Normalize();

        public static Shape MobiusStrip(int un, int vn, bool triangulate = false, bool bothFaces = false) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.MobiusStrip,
                UFrom = 0,
                UTo = 2*Math.PI,
                UN = un,
                VFrom = -1,
                VTo = 1,
                VN = vn,
            }.GetPoints(),
            Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un, bothFaces)
        }.Normalize();

        public static Shape Cylinder(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Cylinder,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = 0,
                VTo = vn - 1,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape ChessCylinder(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Cylinder,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = 0,
                VTo = vn - 1,
                VN = vn,
            }.GetPoints(),
            Convexes = ChessSquares(vn, un)
        };

        public static Shape Circle(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Circle,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = 0,
                VTo = vn - 1,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape CircleAngle(int un, int vn, double from, double to) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Circle,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = from,
                VTo = to,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape CircleM(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Circle,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = vn - 1,
                VTo = 0,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape Cone(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.Cone,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = 0,
                VTo = vn - 1,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };
        public static Shape ConeM(int un, int vn) => new Shape
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.ConeM,
                UFrom = 0,
                UTo = 2 * Math.PI,
                UN = un,
                VFrom = vn - 1,
                VTo = 0,
                VN = vn,
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        public static Shape MathFlower(int un, int vn, double r = 15) => new Shape()
        {
            Points3 = new SurfaceFuncInfo
            {
                Fn = SurfaceFuncs.MathFlower,
                UFrom = -r,
                UTo = r,
                UN = un,
                VFrom = -r,
                VTo = r,
                VN = vn
            }.GetPoints(),
            Convexes = Squares(vn, un)
        };

        private static int[][] Squares(int un, int vn, bool bothFaces = false)
        {
            int GetNum(int u, int v) => vn * u + v;
            if (bothFaces)
                return (un - 1, vn - 1).SelectRange((u, v) => new[]
                {
                    new[] {GetNum(u, v), GetNum(u, v + 1), GetNum(u + 1, v + 1), GetNum(u + 1, v)},
                    new[] {GetNum(u + 1, v), GetNum(u + 1, v + 1), GetNum(u, v + 1), GetNum(u, v)}
                }).SelectMany(v => v).ToArray();
            else
                return (un - 1, vn - 1).SelectRange((u, v) => new int[]
                {
                    GetNum(u, v), GetNum(u, v + 1), GetNum(u + 1, v + 1), GetNum(u + 1, v)
                }).ToArray();
        }

        private static int[][] ChessSquares(int un, int vn)
        {
            int GetNum(int u, int v) => vn * u + v;

            return (un - 1, vn - 1).SelectRange((u, v) => (u, v)).Where(x=>(x.u+x.v).IsEven()).Select(x => new int[]
            {
                GetNum(x.u, x.v), GetNum(x.u, x.v + 1), GetNum(x.u + 1, x.v + 1), GetNum(x.u + 1, x.v)
            }).ToArray();
        }

        private static int[][] Triangles(int un, int vn)
        {
            int GetNum(int u, int v) => vn * u + v;

            return (un - 1, vn - 1).SelectRange((u, v) => new int[][]
            {
                new int[] { GetNum(u, v), GetNum(u, v + 1), GetNum(u + 1, v) },
                new int[] { GetNum(u, v + 1), GetNum(u + 1, v + 1), GetNum(u + 1, v) }
            }).SelectMany(v => v).ToArray();
        }

        private static int[][] Diagonals(int un, int vn)
        {
            int GetNum(int u, int v) => vn * u + v;

            return (un - 1, vn - 1).SelectRange((u, v) => new int[][]
            {
                new int[] { GetNum(u, v), GetNum(u, v + 1), GetNum(u + 1, v + 1), GetNum(u + 1, v) },
                new int[] { GetNum(u, v), GetNum(u + 1, v + 1)},
                new int[] { GetNum(u, v+1), GetNum(u + 1, v)}
            }).SelectMany(v => v).ToArray();
        }
    }
}
