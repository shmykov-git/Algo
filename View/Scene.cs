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
            //var polygon = Sinus(3, 50);
            //var polygon = Spiral(3, 60);
            //var polygon = Polygon3;
            //var polygon = Polygon5;
            //var polygon = Elipse(1, 0.3, 30);
            //var polygon = Elipse(0.4, 1, 10);
            //var polygon = Square.PutInside(Spiral(3, 60));
            //var polygon = Square.PutInside(Square.MultOne(0.9));
            //var polygon = Polygons.Square.PutInside(Polygons.Sinus(3, 100));
            //var polygon = Polygons.Elipse(1, 1, 50).PutInside(Polygons.Spiral(15, 1000).Mult(1.23));
            //var polygon = Polygons.Square(1).PutInside(Polygons.Elipse(1, 1, 51).Mult(0.99)); // todo: check!
            //var polygon = Polygons.Heart(1, 1, 50)
            //    .PutInside(Polygons.Spiral(10, 500).Mult(0.3).Move((0.13,0.21)))
            //    .PutInside(Polygons.Spiral(10, 500).Mult(0.3).Move((-0.13, 0.21)));
            var polygon = Polygons.Spiral(15, 1000);

#if FILL
            var (valid, convexes) = fillEngine.FindConvexes(polygon);
#else
            var (valid, convexes) = (true, (int[][])null, (Trio[])null);
#endif

            var info = new Shape2
            {
                Polygon = polygon,
                Convexes = convexes,
                IsValid = valid
            };

            view.DrawPolygon(info);
        }
    }
}
