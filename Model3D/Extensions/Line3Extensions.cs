using Model3D.AsposeModel;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Libraries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Model3D.Extensions
{
    public static class Line3Extensions
    {
        public static Shape ToShape(this IEnumerable<Line3> lines, double mult = 1, bool scaled = true, Color? fromColor = null, Color? toColor = null)
        {
            var hasMaterial = toColor.HasValue;
            fromColor ??= default;

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

            if (hasMaterial)
            {
                var sizes = shapes.Select(s => s.GetRadius()).ToArray();
                var min = sizes.Min();
                var max = sizes.Max();
                var len = Math.Round(max / min);
                foreach (var i in shapes.Index())
                {
                    var shape = shapes[i];
                    var size = sizes[i];
                    var k = 1 - (size - min) / (max - min);
                    var from = new Vector4(fromColor.Value);
                    var to = new Vector4(toColor.Value);
                    var v = from + (to - from) * k;
                    var material = Materials.GetByColor(v);
                    shape.ApplyMaterial(material);
                }
            }

            return new Shape
            {
                Points = shapes.SelectMany(shape => shape.Points).ToArray(),
                Convexes = shapes.Index().SelectMany(i => shapes[i].Convexes.Transform(c => c + i * n)).ToArray(),
                Materials = hasMaterial ? shapes.SelectMany(shape => shape.Materials).ToArray() : null
            };
        }

        public static Line3[] Move(this Line3[] lines, Vector3 v)
        {
            return lines.Select(l => new Line3(l.a + v, l.b + v)).ToArray();
        }

        // todo: check
        public static Vector3 ProjectionPoint(this Line3 l, Vector3 p)
        {
            return l.a + l.ab * ((p - l.a).MultS(l.ab) / l.Len2);
        }
    }
}
