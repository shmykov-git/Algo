using System;
using Model.Extensions;
using Model3D.Extensions;

namespace Model3D.Libraries;

public delegate Vector3 Trajectory(double x);

public static class Trajectories
{
    public static Trajectory LineTrajectory(Vector3 a, Vector3 b) => x => a + x * (b - a);

    public static Trajectory CircleTrajectory(Vector3 a, Vector3 center, Vector3 normal) =>
        x =>
        {
            var alfa = 2 * Math.PI * x;
            var q = Quaternion.FromAngleAxis(alfa, normal);

            return center + q * (a - center);
        };

    public static Trajectory EllipseTrajectory(Vector3 a, double ratio, Vector3 center, Vector3 normal) =>
        x =>
        {
            var alfa = 2 * Math.PI * x;
            var q = Quaternion.FromAngleAxis(alfa, normal);
            var v = q * (a - center);
            var k = (a - center).MultS(v).Abs() / v.Length2;

            return center + (k + ratio * (1 - k)) * v;
        };
}