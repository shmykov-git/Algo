﻿using Model.Extensions;
using System;

namespace Model3D.Libraries
{
    public delegate double Func3Z(double x, double y);

    public static class Funcs3Z
    {
        public static Func3Z Hyperboloid = (x, y) => x * x - y * y;
        public static Func3Z Paraboloid = (x, y) => x * x + y * y;
        public static Func3Z Waves = (x, y) => Math.Sin((x * x + y * y).Sqrt() *40)/50;
    }
}