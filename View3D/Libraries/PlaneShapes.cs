using Aspose.ThreeD.Utilities;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View3D.Libraries
{
    static class PlaneShapes
    {
        public static PlaneShape Cube => new PlaneShape
        {
            Transformations = new Transform[][]{
                new Transform[]{ p => p + new Vector4(0, 0, 0.5, 0) },
                new Transform[]{ p => p - new Vector4(0, 0, 0.5, 0) },
                new Transform[]{ p => Quaternion.FromRotation(Vector3.ZAxis, Vector3.XAxis) * (p + new Vector4(0, 0, 0.5, 0)) },
                new Transform[]{ p => Quaternion.FromRotation(Vector3.ZAxis, Vector3.XAxis) * (p + new Vector4(0, 0, 0.5, 0)) - new Vector4(1, 0, 0, 0) },
                new Transform[]{ p => Quaternion.FromRotation(Vector3.ZAxis, Vector3.YAxis) * (p + new Vector4(0, 0, 0.5, 0)) },
                new Transform[]{ p => Quaternion.FromRotation(Vector3.ZAxis, Vector3.YAxis) * (p + new Vector4(0, 0, 0.5, 0)) - new Vector4(0, 1, 0, 0) },
            }
        };

        public static PlaneShape Plane => new PlaneShape
        {
            Transformations = new Transform[0][]
        };
    }
}
