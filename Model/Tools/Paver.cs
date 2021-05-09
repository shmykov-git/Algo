using Model.Extensions;

namespace Model.Tools
{
    public static class Paver
    {
        public static Shape2 Pave(Polygon polygon, Shape2 carpet, bool inside = true)
        {
            var super = carpet.ToSuperShape();
            super.Cut(polygon, inside);

            return super.ToShape();
        }

        public static Shape2 PaveExact(Polygon polygon, Shape2 carpet, bool inside = true)
        {
            var super = carpet.ToSuperShape();
            super.Cut(polygon, inside);

            var cutPolygon = super.FindPolygon(!inside, polygon[0]);

            if (inside)
            {
                return polygon.PutInside(cutPolygon).Fill().Join(super.ToShape()).Normalize();
            }
            else
            {
                return cutPolygon.PutInside(polygon).Fill(); //.Join(super.ToShape()).Normalize();
            }
        }
    }
}
