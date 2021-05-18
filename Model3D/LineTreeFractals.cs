using Aspose.ThreeD.Utilities;
using Model;
using Model.Libraries;
using Model3D.Extensions;

namespace Model3D
{
    public static class LineTreeFractals
    {
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
