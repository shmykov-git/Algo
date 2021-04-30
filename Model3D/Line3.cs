using Aspose.ThreeD.Utilities;

namespace Model
{
    public class Line3
    {
        public Vector3 a;
        public Vector3 b;

        public Vector3 ab => b - a;

        public Line3(Vector3 a, Vector3 b)
        {
            this.a = a;
            this.b = b;
        }

        public Line3((Vector3, Vector3) v)
        {
            this.a = v.Item1;
            this.b = v.Item2;
        }
    }
}
