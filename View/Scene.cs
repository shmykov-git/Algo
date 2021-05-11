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

        public Scene(IView view)
        {
            this.view = view;
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
            Shape2 shape;
            bool isValid;
            try
            {
                shape = polygon.Fill();
                isValid = true;
            }
            catch (PolygonFillException e)
            {
                shape = new Shape2
                {
                    Points = polygon.Points,
                    Convexes = e.IntersectConvexes,
                };
                isValid = false;
            }            
#else
            bool isFilled = true;
            Shape2 shape = poligon.ToShape2();
#endif



            view.DrawPolygon(isValid, shape);
        }
    }
}
