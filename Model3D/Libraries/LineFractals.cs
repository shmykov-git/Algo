using Model3D.AsposeModel;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;

namespace Model3D.Libraries
{
    public static class LineFractals
    {
        public static LineFractal Line1 = new LineFractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0.5,0.5,0.5)),
                new Line3(new Vector3(0.5,0.5,0.5), new Vector3(0,0,1)),
            }.Move(new Vector3(0, 0, -0.5))
        };

        public static LineFractal Line2 = new LineFractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0.1,0.2,0.3)),
                new Line3(new Vector3(0.1,0.2,0.3), new Vector3(-0.4,-0.5,-0.6)),
                new Line3(new Vector3(-0.4,-0.5,-0.6), new Vector3(0.7,-0.8,0.9)),
                new Line3(new Vector3(0.7,-0.8,0.9), new Vector3(0,0,1)),
            }.Move(new Vector3(0, 0, -0.5))
        };

        public static LineFractal Line3 = new LineFractal
        {
            lines = new Line3[]
            {
                ((0, 0, 0), (0, 0, 1.0/3)),
                ((0, 0, 1.0/3), (0, 3.0.Sqrt()/6, 0.5)),
                ((0, 3.0.Sqrt()/6, 0.5), (0, 0, 2.0/3)),
                ((0, 0, 2.0/3), (0, 0, 1))
            }.Move(new Vector3(0, 0, -0.5))
        };

        public static LineFractal Cube = new LineFractal
        {
            lines = Shapes.Cube.Mult(1.0 / 3).Lines3
        };

        public static LineFractal AnyShape(Shape shape) => new LineFractal
        {
            lines = shape.Mult(1.0 / 3).Lines3
        };

        public static LineTreeFractal Tree1 = new LineTreeFractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0,0.6,0.1)),
                new Line3(new Vector3(0,0,0), new Vector3(0.3,0,0.4)),
                new Line3(new Vector3(0,0,0), new Vector3(0.1,0.3,0.2))
            }
        };

        public static LineTreeFractal Tree2 = new LineTreeFractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0.5,0,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(-0.5,0,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(0,0.5,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(0,-0.5,0.5)),
            }
        };

        public static LineTreeFractal Tree3 = new LineTreeFractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0), new Vector3(0.4,0.1,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(-0.5,-0.2, 0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(-0.05, 0.5,0.5)),
                new Line3(new Vector3(0,0,0), new Vector3(0.3,-0.5,0.5)),
            }
        };

        public static LineTreeFractal Tree4 = new LineTreeFractal
        {
            lines = new[]
            {
                new Line3(new Vector3(0,0,0.1), new Vector3(0.4,0.1,0.5)),
                new Line3(new Vector3(0,0,0.1), new Vector3(-0.5,-0.2, 0.5)),
                new Line3(new Vector3(0,0,0.1), new Vector3(-0.05, 0.5,0.5)),
                new Line3(new Vector3(0,0,0.1), new Vector3(0.3,-0.5,0.5)),
            }
        };
    }


}
