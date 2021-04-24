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
            //bool valid = true; Trio[] trios = null;
            var (valid, trios) = fillEngine.FillPoligonByTriangles(Poligon);

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
