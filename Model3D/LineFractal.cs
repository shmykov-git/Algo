using Model;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model3D
{
    public class LineFractal
    {
        public Line3[] lines;

        public Line3[] GetFractalLines(Line3 line)
        {
            var len = line.Len;
            var q = Quaternion.FromRotation(Vector3.ZAxis, line.ab.Normalize());

            Line3 GetLine(Line3 l)
            {
                return new Line3(line.Center + q * l.a * len, line.Center + q * l.b * len);
            }

            return lines.Select(GetLine).ToArray();
        }

        public Line3[] CreateFractal(int count)
        {
            return CreateFractal(new[] { new Line3(new Vector3(0, 0, -0.5), new Vector3(0, 0, 0.5)) }, count);
        }

        public Line3[] CreateFractal(Line3[] lines, int count)
        {
            var levelLines = lines;
            for(var i = 0; i<count; i++)
            {
                levelLines = levelLines.SelectMany(l => GetFractalLines(l)).ToArray();
            }

            return levelLines;
        }
    }


}
