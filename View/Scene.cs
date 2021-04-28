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
            fillEngine.OnDebug = view.DrawDebug;
        }

        public void Show()
        {
            //var poligon = Sinus(3, 50);
            //var poligon = Spiral(3, 60);
            //var poligon = Poligon3;
            //var poligon = Poligon5;
            //var poligon = Elipse(1, 0.3, 30);
            //var poligon = Elipse(0.4, 1, 10);
            //var poligon = Square.PutInside(Spiral(3, 60));
            //var poligon = Square.PutInside(Square.MultOne(0.9));
            //var poligon = Poligons.Square.PutInside(Poligons.Sinus(3, 100));
            //var poligon = Poligons.Elipse(1, 1, 50).PutInside(Poligons.Spiral(15, 1000).Mult(1.23));
            //var poligon = Poligons.Square(1).PutInside(Poligons.Elipse(1, 1, 51).Mult(0.99)); // todo: check!
            var poligon = Poligons.Heart(1, 1, 50)
                .PutInside(Poligons.Spiral(10, 500).Mult(0.3).Move((0.13,0.21)))
                .PutInside(Poligons.Spiral(10, 500).Mult(0.3).Move((-0.13, 0.21)));

#if FILL
            var (valid, convexes, trios) = fillEngine.FillPoligonByTriangles(poligon);
#else
            var (valid, convexes, trios) = (true, (int[][])null, (Trio[])null);
#endif

            var info = new PoligonInfo
            {
                Poligon = poligon,
                Convexes = convexes,
                Trios = trios,
                IsValid = valid
            };

            view.DrawPoligon(info);
        }
    }
}
