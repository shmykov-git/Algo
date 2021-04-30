namespace Model
{
    public interface IView
    {
        public void DrawPolygon(Shape2 polygon);
        public void DrawDebug(int[] inds);
    }
}
