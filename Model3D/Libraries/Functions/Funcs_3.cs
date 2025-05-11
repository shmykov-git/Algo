using Model.Extensions;

namespace Model3D.Libraries.Functions;

public static class Funcs_3
{
    public static Func_3 PowerEllipsoid(double power = 2, double a = 1, double b = 1, double c = 1) =>
        v => (v.x / a).Abs().Pow(power) + (v.y / b).Abs().Pow(power) + (v.z / c).Abs().Pow(power) - 1;
    public static Func_3 PowerEllipsoid(double powerA = 2, double powerB = 2, double powerC = 2, double a = 1, double b = 1, double c = 1) =>
        v => (v.x / a).Abs().Pow(powerA) + (v.y / b).Abs().Pow(powerB) + (v.z / c).Abs().Pow(powerC) - 1;
}
