using Aspose.ThreeD.Utilities;
using Model;

namespace View3D.Libraries
{
    public static class Transformations
    {
        public static Transformation Cube => new Transformation
        {
            Transformations = new Transform[]
            {
                p => p + new Vector4(0, 0, 0.5),
                p => p - new Vector4(0, 0, 0.5),
                p => Rotates.Z_X * (p + new Vector4(0, 0, 0.5)),
                p => Rotates.Z_X  * (p + new Vector4(0, 0, 0.5)) - new Vector4(1, 0, 0),
                p => Rotates.Z_Y  * (p + new Vector4(0, 0, 0.5)),
                p => Rotates.Z_Y * (p + new Vector4(0, 0, 0.5)) - new Vector4(0, 1, 0),
            }
        };

        public static Transformation None => new Transformation
        {
            Transformations = new Transform[] { p => p }
        };
    }
}
