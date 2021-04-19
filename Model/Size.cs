namespace Model
{
    public struct Size
    {
        public static readonly Size One = (1, 1);

        public double Width;
        public double Height;

        public static implicit operator Size((double, double) a)
        {
            return new Size { Width = a.Item1, Height = a.Item2 };
        }
        public static Size operator *(Size a, double k)
        {
            return new Size()
            {
                Width = a.Width * k,
                Height = a.Height * k
            };
        }
    }
}
