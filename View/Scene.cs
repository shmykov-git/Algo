using Model;
using Model.Extensions;

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
            view.DrawPoligon(Poligon);
        }

        private Poligon Poligon => new Poligon
        {
            Points = new Point[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8)
            }
        }.Scale((10, 10), (1, 1));
    }
}
