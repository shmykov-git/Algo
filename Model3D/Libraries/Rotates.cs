﻿using Model3D;

namespace View3D.Libraries
{
    public static class Rotates
    {
        public static Quaternion Z_X => Quaternion.FromRotation(Vector3.ZAxis, Vector3.XAxis);
        public static Quaternion Z_mZ => Quaternion.FromRotation(Vector3.ZAxis, -Vector3.ZAxis);
        public static Quaternion Z_mX => Quaternion.FromRotation(Vector3.ZAxis, -Vector3.XAxis);
        public static Quaternion Z_Y => Quaternion.FromRotation(Vector3.ZAxis, Vector3.YAxis);
        public static Quaternion Z_mY => Quaternion.FromRotation(Vector3.ZAxis, -Vector3.YAxis);
        public static Quaternion Y_Z => Quaternion.FromRotation(Vector3.YAxis, Vector3.ZAxis);
        public static Quaternion Y_mZ => Quaternion.FromRotation(Vector3.YAxis, -Vector3.ZAxis);
        public static Quaternion Y_X => Quaternion.FromRotation(Vector3.YAxis, Vector3.XAxis);
        public static Quaternion X_Y => Quaternion.FromRotation(Vector3.XAxis, Vector3.YAxis);
    }
}
