using Model3D.AsposeModel;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Linq;
using MathNet.Numerics;
using Model.Fourier;
using Model.Libraries;
using Vector2 = Model.Vector2;
using System.Diagnostics;
using Shape = Model.Shape;

namespace Model3D.Libraries;

public static class Surfaces
{

    public static Shape Slide(int un, int vn, double height, double width, double gutterBend, double slope, double? hillHeight = null) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.Slide(slope, height, width, hillHeight),
            UFrom = 0,
            UTo = 1,
            UN = un,
            VFrom = -gutterBend * Math.PI/2,
            VTo = -Math.PI + gutterBend * Math.PI / 2,
            VN = vn,
        }.GetPoints(),
        Convexes = Squares(vn, un)
    };

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
        Convexes = Convexes.Squares(vn, un)// Squares(vn, un)
    };

    public static Shape Plane(Plane plane, int un, int vn, double mult = 1) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = plane.PointsFn,
            UFrom = (-un / 2 - 0.5 * ((un + 1) % 2)) * mult,
            UTo = (un / 2 + 0.5 * ((un + 1) % 2)) * mult,
            UN = un,
            VFrom = (-vn / 2 - 0.5 * ((vn + 1) % 2)) * mult,
            VTo = (vn / 2 + 0.5 * ((vn + 1) % 2)) * mult,
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

    public static Shape Shamrock(int un, int vn, bool triangulate = false, bool normalize = true) => new Shape
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
    }.ModifyIf(normalize, s => s.Normalize());

    public static Shape ShamrockDynamic(int un, int vn, int ui, bool triangulate = false) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.Shamrock,
            UFrom = 0,
            UTo = 4 * Math.PI * ui / (un-1),
            UN = un,
            VFrom = -Math.PI,
            VTo = Math.PI,
            VN = vn,
        }.GetPoints(),
        Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
    }.Normalize();

    public static Shape Shell(int un, int vn, int nToSpins = 2, bool triangulate = false, bool normalize = true) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.Shell,
            UFrom = 0,
            UTo = nToSpins * 2 * Math.PI,
            UN = un,
            VFrom = -Math.PI,
            VTo = Math.PI,
            VN = vn,
        }.GetPoints(),
        Convexes = triangulate ? Triangles(vn, un) : Squares(vn, un)
    }.ModifyIf(normalize, s=>s.Normalize());

    public static Shape ShellC(int m, int n, int nToSpins = 2, ConvexFunc? convexesFn = null, bool mClosed = false, bool nClosed = false, bool normalize = true) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.Shell,
            UFrom = 0,
            UTo = nToSpins * 2 * Math.PI,
            UN = n,
            VFrom = -Math.PI,
            VTo = Math.PI,
            VN = m,
        }.GetPoints(),
        Convexes = (convexesFn ?? Convexes.Squares).Invoke(m, n, mClosed, nClosed)
    }.Mult(1.0/m).ModifyIf(normalize, s => s.Normalize());
   
    public static Shape Shell2(int un, int vn, double vFromSpin = -0.5, double vToSpin = 0.5, double uFromSpins = 0, double uToSpins = 2, bool triangulate = false) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.Shell,
            UFrom = uFromSpins * 2 * Math.PI,
            UTo = uToSpins * 2 * Math.PI,
            UN = un,
            VFrom = vFromSpin * 2 * Math.PI,
            VTo = vToSpin * 2 * Math.PI,
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

    public static Shape DiniSurfaceC(int m, int n, double alfa = 2, ConvexFunc? convexesFn = null, bool mClosed = false, bool nClosed = false, bool bothFaces = false) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.DiniSurface,
            UFrom = 0,
            UTo = 4 * Math.PI,
            UN = n,
            VFrom = 0.005,
            VTo = alfa,
            VN = m,
        }.GetPoints(),
        Convexes = (convexesFn ?? Convexes.Squares).Invoke(m, n, mClosed, nClosed)
    }.Normalize().Centered().Mult(0.25);

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
    }.Normalize();

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
    }.Normalize();

    public static Shape PlaneHeart(int un, int vn) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.Heart,
            UFrom = 0,
            UTo = -2 * Math.PI,
            UN = un,
            VFrom = 0,
            VTo = vn - 1,
            VN = vn,
        }.GetPoints(),
        Convexes = Squares(vn, un)
    }.Normalize();

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
    }.Normalize();

    public static Shape CircleAngleM(int un, int vn, double from, double to) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.Circle,
            UFrom = 0,
            UTo = -2 * Math.PI,
            UN = un,
            VFrom = from,
            VTo = to,
            VN = vn,
        }.GetPoints(),
        Convexes = Squares(vn, un)
    }.Normalize();

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
    }.Normalize();

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

    public static Shape MagicWand(int un, int vn, int l = 3, double w = 0.3, double a = 1.3, double b = 1.3, double c = 2, ConvexFunc convexFunc = null) => new Shape
    {
        Points3 = new SurfaceFuncInfo
        {
            Fn = SurfaceFuncs.MagicWand(w, a, b, c),
            UFrom = 0,
            UTo = 2 * Math.PI,
            UN = un,
            VFrom = (Math.PI).Pow(1/a),
            VTo = ((2*l+1.5) * Math.PI).Pow(1/a),
            VN = vn,
        }.GetPoints(),
        Convexes = (convexFunc ?? Convexes.Squares).Invoke(vn, un, false, true)
    }.Centered();

    private static int[][] Squares(int un, int vn, bool bothFaces = false) => bothFaces ? Convexes.SquaresBoth(un, vn) : Convexes.Squares(un, vn);

    private static int[][] ChessSquares(int un, int vn) => Convexes.ChessSquares(un, vn);
    //{
    //    int GetNum(int u, int voxel) => vn * u + voxel;
    //    return (un - 1, vn - 1).SelectRange((u, voxel) => (u, voxel)).Where(x=>(x.u+x.voxel).IsEven()).Select(x => new int[]
    //    {
    //        GetNum(x.u, x.voxel), GetNum(x.u, x.voxel + 1), GetNum(x.u + 1, x.voxel + 1), GetNum(x.u + 1, x.voxel)
    //    }).ToArray();
    //}

    private static int[][] Triangles(int un, int vn) => Convexes.Triangles(un, vn);
    //{
    //    int GetNum(int u, int voxel) => vn * u + voxel;
    //    return (un - 1, vn - 1).SelectRange((u, voxel) => new int[][]
    //    {
    //        new int[] { GetNum(u, voxel), GetNum(u, voxel + 1), GetNum(u + 1, voxel) },
    //        new int[] { GetNum(u, voxel + 1), GetNum(u + 1, voxel + 1), GetNum(u + 1, voxel) }
    //    }).SelectMany(voxel => voxel).ToArray();
    //}

    private static int[][] Diagonals(int un, int vn) => Convexes.DiagonalSquares2D(un, vn);
    //{
    //    int GetNum(int u, int voxel) => vn * u + voxel;
    //    return (un - 1, vn - 1).SelectRange((u, voxel) => new int[][]
    //    {
    //        new int[] { GetNum(u, voxel), GetNum(u, voxel + 1), GetNum(u + 1, voxel + 1), GetNum(u + 1, voxel) },
    //        new int[] { GetNum(u, voxel), GetNum(u + 1, voxel + 1)},
    //        new int[] { GetNum(u, voxel+1), GetNum(u + 1, voxel)}
    //    }).SelectMany(voxel => voxel).ToArray();
    //}
}
