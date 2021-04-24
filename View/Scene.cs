#define FILL

using Model;
using Model.Extensions;
using Model.Tools;

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
            var trios = fillEngine.FillPoligonByTriangles(Poligon);
#else
            var (valid, trios) = (true, (Trio[])null);
#endif

            var info = new PoligonInfo
            {
                Poligon = poligon,
                Trios = trios,
                IsValid = true
            };

            view.DrawPoligon(info);
        }

        private Poligon Poligon => new Poligon
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
