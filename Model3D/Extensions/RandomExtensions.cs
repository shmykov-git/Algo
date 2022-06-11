using System;
using Aspose.ThreeD.Utilities;

namespace Model3D.Extensions
{
    public static class RandomExtensions
    {
        public static Vector3 NextV3(this Random r) => new(r.NextDouble(), r.NextDouble(), r.NextDouble());
    }
}