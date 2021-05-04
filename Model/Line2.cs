using System;

namespace Model
{
    public struct Line2
    {
        public Vector2 A;
        public Vector2 B;

        public Vector2 AB => B - A;
        public Vector2 One => AB.Normed;
        public Vector2 Normal => (AB.Y, -AB.X);
        public Vector2 NOne => Normal.Normed;

        public double Fn(Vector2 x) => (x - A) * Normal;

        public bool IsLeft(Vector2 x) => Fn(x) < 0;

        public double Distance(Vector2 x) => Math.Abs(Fn(x));

        public Line2(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }

        public static implicit operator Line2((Vector2 a, Vector2 b) l)
        {
            return new Line2 (l.a, l.b);
        }
    }
}
