using Aspose.ThreeD.Utilities;
using System;

namespace Model3D.Libraries
{
    public delegate Vector3 Func3(double t);

    public static class Funcs3
    {
        public static Func3 Spiral = t => new Vector3(Math.Sin(t), Math.Cos(t), t/(2*Math.PI));
    }
}
