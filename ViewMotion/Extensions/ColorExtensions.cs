using System.Windows.Media;

namespace ViewMotion.Extensions;

static class ColorExtensions
{
    public static Color ToWColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
}