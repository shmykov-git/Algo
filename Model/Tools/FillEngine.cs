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
        public Trio[] FillPoligonByTriangles(Poligon poligon)
        {
            List<Trio> res = new();

            var vertices = poligon.Points.Index().ToList();

            Trio ToVTrio(Trio trio) => new Trio(vertices[trio.I], vertices[trio.J], vertices[trio.K]);

            TrioPairInfo ToPairInfo(Trio trio) => new TrioPairInfo
            {
                Line = new Line2(poligon[trio.I], poligon[trio.J]),
                Point = poligon[trio.K]
            };

            bool IsValidT(Trio trio) =>
                trio.SelectPairs()
                .Select(t => ToPairInfo(ToVTrio(t)))
                .All(v => v.Line.Fn(v.Point) < 0);

            var t = new Trio(0, 1, 2);
            while(vertices.Count > 2)
            {
                if (IsValidT(t))
                {
                    res.Add(ToVTrio(t));
                    vertices.RemoveAt(t.J);
                }
                else
                {
                    t.K = (t.K + 1) % vertices.Count;
                }
            }

            return res.ToArray();
        }

        struct TrioPairInfo
        {
            public Line2 Line;
            public Vector2 Point;
        }
    }
}
