using System.Numerics;

namespace Model.Extensions
{
    public static class ComplexExtensions
    {
        public static Vector2 ToV2(this Complex c) => new Vector2(c.Real, c.Imaginary);
    }
}
