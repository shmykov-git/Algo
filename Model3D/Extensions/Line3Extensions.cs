using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Libraries;
using System.Collections.Generic;
using System.Linq;

namespace Model3D.Extensions
{
    public static class Line3Extensions
    {
        public static Shape ToShape(this IEnumerable<Line3> lines, double mult = 1, bool scaled = true)
        {
            var lineShape = Shapes.Cube.Move(0, 0, 0.5);
            var n = lineShape.PointsCount;

            var width = 0.003 * mult;

            Shape GetLineShape(Line3 line)
            {
                var q = Quaternion.FromRotation(Vector3.ZAxis, line.ab.Normalize());
                var w = scaled ? width * line.Len : width;
                var shape = lineShape.Scale(w, w, line.ab.Length).Transform(p => q * p).Move(line.a);

                return shape;
            }

            var shapes = lines.Select(GetLineShape).ToArray();

            return new Shape
            {
                Points = shapes.SelectMany(shape => shape.Points).ToArray(),
                Convexes = shapes.Index().SelectMany(i => shapes[i].Convexes.Transform(c => c + i * n)).ToArray()
            };
        }

        public static Line3[] Move(this Line3[] lines, Vector3 v)
        {
            return lines.Select(l => new Line3(l.a + v, l.b + v)).ToArray();
        }
    }
}
