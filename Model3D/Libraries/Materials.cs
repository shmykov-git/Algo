using Aspose.ThreeD.Utilities;
using Model;
using System;
using System.Collections.Concurrent;
using System.Drawing;

namespace Model3D.Libraries
{
    public static class Materials
    {
        public static double Precision = 10;

        private static ConcurrentDictionary<Color, Material> materials = new ConcurrentDictionary<Color, Material>();

        private static int CorrectFn(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);
        private static int PrecisionFn(double x) => CorrectFn((int)(Precision * Math.Round((255 * x / Precision))));
        private static Color ColorFn(Vector4 v) => Color.FromArgb(PrecisionFn(v.w), PrecisionFn(v.x), PrecisionFn(v.y), PrecisionFn(v.z));

        public static Material GetByColor(Vector4 v) => materials.GetOrAdd(ColorFn(v), c => new Material { Color = c });
        public static Material GetByColor(Color c) => GetByColor(new Vector4(c));
    }
}
