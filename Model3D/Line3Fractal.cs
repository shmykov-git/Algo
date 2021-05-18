using Aspose.ThreeD.Utilities;
using Model;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model3D
{
    public class Line3Fractal
    {
        public Line3[] lines;

        public Line3[] GetFractalLines(Line3 line)
        {
            var len = line.Len;
            var q = Quaternion.FromRotation(Vector3.ZAxis, line.ab.Normalize());

            Line3 GetLine(Line3 l)
            {
                var a = l.a;
                var b = l.a + l.ab * len;
                return new Line3(line.b + q * a, line.b + q * b);
            }

            return lines.Select(GetLine).ToArray();
        }

        public Line3[] CreateFractal(Line3 line, int count)
        {
            List<Line3> fractal = new List<Line3>();
            fractal.Add(line);

            var levelLines = new[] { line };
            for(var i = 0; i<count; i++)
            {
                levelLines = levelLines.SelectMany(l => GetFractalLines(l)).ToArray();
                fractal.AddRange(levelLines);
            }

            return fractal.ToArray();
        }
    }


}
