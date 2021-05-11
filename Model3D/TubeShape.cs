using Model3D.Extensions;
using Model3D.Libraries;

namespace Model
{
    public class TubeShape
    {
        public Shape Shape;
        public double ConnectAngle;

        public Shape Cylinder => Shape.Transform(TransformFuncs3.CylinderWrapZ);
    }
}
