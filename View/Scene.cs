#define FILL

using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using System;
using System.Linq;

namespace View
{
    class Scene
    {
        private readonly IView view;
        private readonly FillEngine fillEngine;

        public Scene(IView view, FillEngine fillEngine)
        {
            this.view = view;
            this.fillEngine = fillEngine;
        }


        public void Show()
        {
            var poligon = Poligon;

#if FILL
            var (valid, trios) = fillEngine.FillPoligonByTriangles(poligon);
#else
            var (valid, trios) = (true, (Trio[])null);
#endif

            var info = new PoligonInfo
            {
                Poligon = poligon,
                Trios = trios,
                IsValid = valid
            };

            view.DrawPoligon(info);
        }

        private Poligon Poligon => new Poligon
        {
            Points = new Vector2[]
            {
                (4, 0), (6.5, 2), (6,5), (2,7), (0.5,5), (0.5, 3.5), (1.5,2.5), (3,3.5), 
                (3.5, 3.7), (4,2.5), (4.5, 3.5), (3.5, 4.5), (2.5, 4), (1.5, 3.3), (1,4.2), 
                (2.5, 5.5), (5, 4.5), (5.3, 2.2)
            }
        }.ScaleToOne((10, 10));

        private Poligon Spiral(double n, int count) => new Poligon
        {
            Points = new Func2Info 
            { 
                Fn = t => (-Math.Abs(t) * Math.Sin(t), t * Math.Cos(t)),
                From = n * 2 * Math.PI + Math.PI/2,
                To = - n * 2 * Math.PI + Math.PI/2,
                N = count,
            }.GetPoints().Reverse().ToArray()
        }.Mult(0.8).Move((n * 2 * Math.PI, n * 2 * Math.PI)).ScaleToOne((n * 4 * Math.PI, n * 4 * Math.PI));

        private Poligon Poligon4 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 3.5),
                (4.5, 4),
                (6, 7),
                (3, 8),
                (2.5, 5.5),
                (3.5, 4.5),
                (5, 6),
            }
        }.Scale((10, 10), (1, 1));
        private Poligon Poligon3 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8),
                (3, 5),
                (5, 6),
            }
        }.Scale((10, 10), (1, 1));

        private Poligon Poligon2 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8),
                (3, 5),
            }
        }.Scale((10, 10), (1, 1));

        private Poligon Poligon1 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8)
            }
        }.Scale((10, 10), (1, 1));
    }
}
