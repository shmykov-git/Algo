using System;

namespace Model3D.Libraries
{
    public delegate double Func3(double x, double y);

    public class Funcs3
    {
        public static Func3 Hyperboloid = (x, y) => x * x - y * y;
        public static Func3 Paraboloid = (x, y) => x * x + y * y; 
    }
}
