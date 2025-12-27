using Model3D;

namespace Model
{
    public delegate Vector4 Transform(Vector4 p);

    public class Multiplication
    {
        public Transform[] Transformations;
    }
}
