using Model.Extensions;

namespace Model3D.Libraries.Functions;

public static class Funcs_3
{
    public static Func_3 Ellipsoid(double power = 2, double a = 1, double b = 1, double c = 1) =>
        v => (v.x / a).Abs().Pow(power) + (v.y / b).Abs().Pow(power) + (v.z / c).Abs().Pow(power) - 1;
}
