using Aspose.ThreeD.Utilities;
using Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model3D.Libraries
{
    public static class Materials
    {
        private static ConcurrentDictionary<Color, Material> materials = new ConcurrentDictionary<Color, Material>();

        private static int PrecisionFn(double x) => (int)(255 * x);
        private static Color ColorFn(Vector4 v) => Color.FromArgb(PrecisionFn(v.w), PrecisionFn(v.x), PrecisionFn(v.y), PrecisionFn(v.z));

        public static Material GetByColor(Vector4 v) => materials.GetOrAdd(ColorFn(v), c => new Material { Color = c });
    }
}
