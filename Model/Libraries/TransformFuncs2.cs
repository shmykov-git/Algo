using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Libraries
{
    public static class TransformFuncs2
    {
        private static double AngleFn(Vector2 v) => Math.Atan2(v.y, v.x);

        private static Func<Vector2, Vector2> ParametricCircleTransformation(Func2 fn, double radius) => v => radius * fn(AngleFn(v));

        public static Func<Vector2, Vector2> Circle(double radius = 1) => v => radius * v.Normed;

        public static Func<Vector2, Vector2> Heart(double radius = 1) => ParametricCircleTransformation(Funcs2.Heart(), radius);
    }
}
