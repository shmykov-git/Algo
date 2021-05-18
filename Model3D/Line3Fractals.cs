using Aspose.ThreeD.Utilities;
using Model;

namespace Model3D
{
    public static class Line3Fractals
    {
        public static Line3Fractal FractalTree1 = new Line3Fractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0,0.6,0.1)),
                new Line3(new Vector3(0,0,0), new Vector3(0.3,0,0.4)),
                new Line3(new Vector3(0,0,0), new Vector3(0.1,0.3,0.2))
            }
        };

        public static Line3Fractal FractalTree2 = new Line3Fractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0.5,0,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(-0.5,0,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(0,0.5,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(0,-0.5,0.5)),
            }
        };

        public static Line3Fractal FractalTree3 = new Line3Fractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0.4,0.1,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(-0.5,-0.2, 0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(-0.05, 0.5,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(0.3,-0.5,0.5)),
            }
        };
    }


}
