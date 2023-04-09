using System;
using Aspose.ThreeD.Utilities;

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
}