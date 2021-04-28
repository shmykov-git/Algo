using Aspose.ThreeD.Utilities;

namespace View3D.Libraries
{
    public static class Rotates
    {
        public static Quaternion Z_X => Quaternion.FromRotation(Vector3.ZAxis, Vector3.XAxis);
        public static Quaternion Z_mZ => Quaternion.FromRotation(Vector3.ZAxis, -Vector3.ZAxis);
        public static Quaternion Z_mX => Quaternion.FromRotation(Vector3.ZAxis, -Vector3.XAxis);
        public static Quaternion Z_Y => Quaternion.FromRotation(Vector3.ZAxis, Vector3.YAxis);
    }
}
