using System;

namespace Model.Libraries;

public delegate double Acceleration(double x);

public static class Accelerations
{
    public static double Line(double x) => x;
    public static double Sin(double x) => 0.5 * (1 + Math.Sin(Math.PI * (x - 0.5))); // по кругу
    public static double Poly2(double x) => x < 0.5 ? 2 * x * x : -2 * x * x + 4 * x - 1; // ускорение и торможение

    public static Acceleration PolyA2(double a = 0.2) // a = [0, 0.5] // ускорение, линейный полет, торможение
    {
        var b = 0.5 / (a - a * a);
        var k = 2 * a * b;
        var k2 = -a * a * b;

        return x => x < a
            ? b * x * x
            : x < 1 - a
                ? k * x + k2
                : -b * (x - 1) * (x - 1) + 1;
    }
}

//return new[]
//{
//    (1000).SelectRange(i => i / 1000.0).Select(x => new Vector2(x, Accelerations.PolyA2(0.2)(x))).ToShape2().ToShape3().ToLines(1, Color.Blue),
//    (1000).SelectRange(i => i / 1000.0).Select(x => new Vector2(x, Accelerations.Line(x))).ToShape2().ToShape3().ToLines(1, Color.Red),
//    Shapes.Coods2WithText
//}.ToSingleShape().Centered().ToMotion();