using System;

namespace Model.Bezier;

public class Bz : Bezier
{
    public Bz(Vector2 a, Vector2 b) : base(1, new[] { a, b }, new[] { 1, 1 })
    {
    }

    public Bz(Vector2 a, Vector2 b, Vector2 c) : base(2, new[] { a, b, c }, new[] { 1, 2, 1 })
    {
    }

    public Bz(Vector2 a, Vector2 b, Vector2 c, Vector2 d) : base(3, new[] { a, b, c, d }, new[] { 1, 3, 3, 1 })
    {
    }

    public Bz(Vector2[] ps): base(ps)
    {
    }
}
