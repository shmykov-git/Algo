using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System;
using System.Linq;

namespace Model3D.Libraries;

public static class Solids
{
    public static Shape Sphere(int un, int vn, int ln) => new Shape
    {
        Points3 = new SolidFuncInfo
        {
            Fn = SolidFuncs.Sphere,
            UFrom = 0,
            UTo = -2 * Math.PI,
            UN = un,
            VFrom = 0,
            VTo = Math.PI,
            VN = vn,
            LFrom = 0,
            LTo = 1,
            LN = ln,
        }.GetPoints(),
        Convexes = Cubes(un, vn, ln)
    }.NormalizeWith2D();

    private static int[][] Cubes(int un, int vn, int ln)
    {
        var convexes = Shapes.PerfectCube.Convexes;

        int GetNum(int u, int v, int l) => ln * (vn * u + v) + l;
        (int i, int j, int k) GetShift(int m) => (m & 1, (m >> 1) & 1, (m >> 2) & 1);

        return (un - 1, vn - 1, ln - 1).SelectRange((u, v, l) => convexes.Select(cs => cs.Select(GetShift).Select(s => GetNum(u + s.i, v + s.j, l + s.k)).ToArray()).ToArray()).SelectMany(v => v).ToArray();
    }
}
