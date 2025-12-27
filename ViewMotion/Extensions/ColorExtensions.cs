using ColorPicker.Models;
using System.Windows.Media;

namespace ViewMotion.Extensions;

static class ColorExtensions
{
    public static Color ToWColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);

    public static ColorState ToState(this Color c)
    {
        var s = new ColorState();
        s.SetARGB(c.ScA, c.ScR, c.ScG, c.ScB);

        return s;
    }

    public static System.Drawing.Color ToDColor(this Color c)
    {
        return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
    }

    public static Color FromState(this ColorState s)
    {
        return Color.FromScRgb((float)s.A, (float)s.RGB_R, (float)s.RGB_G, (float)s.RGB_B);
    }
}