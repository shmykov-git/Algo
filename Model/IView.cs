namespace Model
{
    public interface IView
    {
        public void DrawPoligon(PoligonInfo poligon);
        public void DrawDebug(int[] inds);
    }
}
