namespace Model
{
    public class PoligonInfo
    {
        public Poligon Poligon;
        public Trio[] Trios;
        public bool IsValid;

        public bool IsFilled => Trios != null;

        public PoligonInfo ModifyPoligon(Poligon poligon) => new PoligonInfo
        {
            Poligon = poligon,
            IsValid = this.IsValid,
            Trios = this.Trios
        };
    }
}
