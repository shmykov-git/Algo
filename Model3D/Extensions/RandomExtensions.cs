using System;

namespace Model3D.Extensions
{
    public static class RandomExtensions
    {
        public static Vector3 NextV3(this Random r, double a = 1) =>
            a * new Vector3(r.NextDouble(), r.NextDouble(), r.NextDouble());

        public static Vector3 NextCircleYV3(this Random r, double a = 1)
        {
            var alfa = 2 * Math.PI * r.NextDouble();

            return a * new Vector3(Math.Cos(alfa), 0, Math.Sin(alfa));
        }

        public static Vector3 NextCenteredV3(this Random r, double a = 1) =>
            a * new Vector3(r.NextDouble() - 0.5, r.NextDouble() - 0.5, r.NextDouble() - 0.5);

        public static Vector3 NextRotation(this Random r) => r.NextV3(Math.PI);
    }
}