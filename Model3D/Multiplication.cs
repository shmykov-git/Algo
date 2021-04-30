using Aspose.ThreeD.Utilities;
using System;

namespace Model
{
    public delegate Vector4 Transform(Vector4 p);

    public class Multiplication
    {
        public Transform[] Transformations;
    }
}
