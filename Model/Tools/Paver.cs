using Model.Extensions;

namespace Model.Tools
{
    public static class Paver
    {
        public static Shape2 Pave(Polygon poligon, Shape2 carpet)
        {
            var super = carpet.ToSuperShape();
            super.Cut(poligon);
            var shape = super.ToShape();

            return shape;
        }
    }
}
