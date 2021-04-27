using Aspose.ThreeD.Utilities;
using System;

namespace Model
{
    delegate Vector4 Transform(Vector4 p);

    class PlaneShape
    {
        public Transform[][] Transformations;
    }
}
