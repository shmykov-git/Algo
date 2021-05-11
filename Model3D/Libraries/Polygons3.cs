using Aspose.ThreeD.Utilities;
using Model;
using System;
using Model3D.Extensions;

namespace Model3D.Libraries
{
    public static class Polygons3
    {
        public static Polygon3 Test(Vector3 size, int count) => new Polygon3
        {
            Points = new Func3Info
            {
                Fn = Funcs3.Test,
                From = 0,
                To = 2 * Math.PI,
                N = count,
                Closed = true
            }.GetPoints()
        }.Scale(size);
    }
}
