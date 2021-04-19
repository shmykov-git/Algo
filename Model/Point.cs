namespace Model
{

    public struct Point
    {
        public double X;
        public double Y;

        public static implicit operator Point((double, double) a)
        {
            return new Point { X = a.Item1, Y = a.Item2 };
        }

        public static Point operator +(Point a, Size b)
        {
            return new Point
            {
                X = a.X + b.Width,
                Y = a.Y + b.Height
            };
        }
        public static Point operator -(Point a, Size b)
        {
            return new Point
            {
                X = a.X - b.Width,
                Y = a.Y - b.Height
            };
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };
        }

        public static Point operator *(Point a, double k)
        {
            return new Point()
            {
                X = a.X * k,
                Y = a.Y * k
            };
        }

        public static Point operator *(double k, Point a)
        {
            return new Point()
            {
                X = a.X * k,
                Y = a.Y * k
            };
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
