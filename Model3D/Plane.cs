using Aspose.ThreeD.Utilities;
using Model3D.Extensions;

namespace Model
{
    public class Plane
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 ca => a - c;
        public Vector3 cb => b - c;
        public Vector3 Normal => ca.MultV(cb);
        public Vector3 NOne => Normal.Normalize();

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
