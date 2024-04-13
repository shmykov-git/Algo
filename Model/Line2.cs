using System;
using Microsoft.VisualBasic.CompilerServices;

namespace Model
{
    public struct Line2
    {
        public Vector2 A;
        public Vector2 B;

        public Vector2 AB => B - A;
        public double Len => AB.Len;
        public double Len2 => AB.Len2;
        public Vector2 Center => (A + B) / 2;
        public Vector2 One => AB.Normed;
        public Vector2 Normal => (AB.y, -AB.x);
        public Vector2 NOne => Normal.Normed;

        public double Fn(Vector2 x) => (x - A) * Normal;
        public double FnA(Vector2 x) => (x - A) * AB;
        public double FnB(Vector2 x) => (x - B) * AB;
        public double FnAngleB(Vector2 x) => Math.Atan2(Fn(x), FnB(x));
        public double FnAngleA(Vector2 x) => Math.Atan2(Fn(x), FnA(x));

        public bool IsLeft(Vector2 x) => Fn(x) < 0;

        public Func<Vector2, bool> GetIsLeftFn(double epsilon = 0)
        {
            var n = Normal;
            var a = A;
            return x => (x - a) * n < epsilon;
        }

        public double Distance(Vector2 x) => Math.Abs(Fn(x) / AB.Len);

        public double SegmentDistance(Vector2 x)
        {
            if (FnB(x) > 0)
                return (x - B).Len;

            if (FnA(x) < 0)
                return (x - A).Len;

            return Distance(x);
        }

        public Line2(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }

        public override string ToString() => $"{A} - {B}";

        public static implicit operator Line2((Vector2 a, Vector2 b) l)
        {
            return new Line2 (l.a, l.b);
        }
    }
}
