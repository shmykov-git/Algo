﻿using Model.Extensions;
using System;

namespace Model.Libraries
{
    public delegate Vector2 Func2(double v);

    public static class Funcs2
    {
        public static Func2 Circle() => t => (Math.Sin(t), Math.Cos(t));

        public static Func2 Heart()
        {
            Func2 heartFn = t =>
                (
                    16 * Math.Sin(t).Pow3(),
                    13 * Math.Cos(t) - 5 * Math.Cos(2 * t) - 2 * Math.Cos(3 * t) - Math.Cos(4 * t)
                );

            return v => heartFn(v).Scale((1.0 / 64, 1.0 / 48)) + new Vector2(0, 0.1);
        }
    }
}