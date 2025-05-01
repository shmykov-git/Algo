using Model3D.AsposeModel;
using Model;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model3D
{
    public class LineTreeFractal
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

        public Line3[] CreateFractal(int count)
        {
            return CreateFractal(new[] { Line3.ZLineOne }, count);
        }

        public Line3[] CreateFractal(Line3[] lines, int count)
        {
            List<Line3> fractal = new List<Line3>();
            fractal.AddRange(lines);

            var levelLines = lines;
            for(var i = 0; i<count; i++)
            {
                levelLines = levelLines.SelectMany(l => GetFractalLines(l)).ToArray();
                fractal.AddRange(levelLines);
            }

            return fractal.ToArray();
        }
    }


}
