using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Linq;

namespace Model3D.Libraries
{
    public static class Surfaces
    {
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
            Convexes = Squeres(vn, un)
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
            Convexes = Squeres(vn, un)
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
            Convexes = triangulate ? Triangles(vn, un) : Squeres(vn, un)
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
            Convexes = triangulate ? Triangles(vn, un) : Squeres(vn, un)
        }.Normalize();

        public static Shape DiniSurface(int un, int vn, bool triangulate = false) => new Shape
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
            Convexes = triangulate ? Triangles(vn, un) : Squeres(vn, un)
        }.Normalize();

        public static Shape MobiusStrip(int un, int vn, bool triangulate = false) => new Shape
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
            Convexes = triangulate ? Triangles(vn, un) : Squeres(vn, un)
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
            Convexes = Squeres(vn, un)
        };

        private static int[][] Squeres(int un, int vn)
        {
            int GetNum(int u, int v) => vn * u + v;

            return (un - 1, vn - 1).SelectRange((u, v) => new int[] { GetNum(u, v), GetNum(u, v + 1), GetNum(u + 1, v + 1), GetNum(u + 1, v) }).ToArray();
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
    }
}
