namespace Model
{
    public struct Line
    {
        public Point A;
        public Point B;

        public Line(Point a, Point b)
        {
            A = a;
            B = b;
        }

        public static implicit operator Line((Point, Point) l)
        {
            return new Line { A = l.Item1, B = l.Item2 };
        }
    }
}
