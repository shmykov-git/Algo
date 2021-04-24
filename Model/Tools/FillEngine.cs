using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Tools
{
    public class FillEngine
    {
        private const int MaxCircles = 10;

        public (bool, Trio[]) FillPoligonByTriangles(Poligon poligon)
        {
            List<Trio> triangles = new();

            var vertices = poligon.Points.Index().ToList();

            Trio ToVTrio(Trio trio) => new Trio(vertices[trio.I], vertices[trio.J], vertices[trio.K]);

            int CorrectInd(int i) => (i + vertices.Count) % vertices.Count;
            int NextInd(int i) => CorrectInd(i + 1);
            int PrevInd(int i) => CorrectInd(i - 1);
            Trio NextTrio(Trio trio) => new Trio(trio.J, trio.K, NextInd(trio.K));

            IEnumerable<int> SelectSiblings(Trio trio) 
            {
                yield return NextInd(trio.K);
                yield return PrevInd(trio.I);
            }

            TrioPairInfo ToPairInfo(Trio trio) => new TrioPairInfo
            {
                Line = new Line2(poligon[trio.I], poligon[trio.J]),
                Point = poligon[trio.K]
            };

            TrioInfo GetTrioInfo(Trio trio) => new TrioInfo
            {
                Info = trio.SelectPairs()
                .Select(t => ToPairInfo(ToVTrio(t))).ToArray()
            };

            bool IsValid(Trio trio)
            {
                var info = GetTrioInfo(trio);

                if (!info.IsLeftTriangle)
                    return false;

                if (SelectSiblings(trio).Any(l => info.IsInsidePoint(poligon[vertices[l]])))
                    return false;

                return true;
            }

            var circleCount = 0;
            var t = new Trio(0, 1, 2);

            while (vertices.Count > 2)
            {
                if (IsValid(t))
                {
                    triangles.Add(ToVTrio(t));
                    vertices.RemoveAt(t.J);
                    t.K = CorrectInd(t.K);
                }
                else
                {
                    t = NextTrio(t);

                    if (t.K == 0 && ++circleCount == MaxCircles)
                        break;
                }
            }

            return (circleCount < MaxCircles, triangles.ToArray());
        }

        struct TrioInfo
        {
            public TrioPairInfo[] Info;

            public bool IsInsidePoint(Vector2 point) => Info.All(v => v.CheckLeftPoint(point));
            public bool IsLeftTriangle => Info.All(v => v.IsLeftPoint);
        };

        struct TrioPairInfo
        {
            public Line2 Line;
            public Vector2 Point;

            public bool CheckLeftPoint(Vector2 point) => Line.Fn(point) < 0;
            public bool IsLeftPoint => CheckLeftPoint(Point);
        }
    }
}
