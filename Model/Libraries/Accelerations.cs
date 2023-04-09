using System;

namespace Model.Libraries;

public delegate double Acceleration(double x);

public static class Accelerations
{
    public static double Line(double x) => x;
    public static double Sin(double x) => 0.5 * (1 + Math.Sin(Math.PI * (x - 0.5))); // по кругу
    public static double Poly2(double x) => x < 0.5 ? 2 * x * x : -2 * x * x + 4 * x - 1; // ускорение и торможение
}