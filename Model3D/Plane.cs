using Aspose.ThreeD.Utilities;

namespace Model
{
    public class Plane
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 ab => b - a;
        public Vector3 bc => c - b;
        public Vector3 Normal => ab * bc;
        public Vector3 NOne => Normal.Normalize();

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
