using System.Diagnostics;
using System.Drawing;
using AI.Images;
using Model.Extensions;

namespace AI.Extensions;

public static class NImageExtensions
{
    public const int white = NImage.white;
    public const int black = NImage.black;
    public const int red = NImage.red;

    public static NImage Smooth(this NImage image, int d = 3)
    {
        var s = (d - 1) / 2;
        var ds = (d, d).SelectRange((i, j) => (i: i - s, j: j - s)).ToArray();

        int Avg(IEnumerable<int> cs)
        {
            int count = 0;
            (int a, int r, int g, int b) = (0, 0, 0, 0);

            cs.ForEach(v =>
            {
                count++;
                var (aa, rr, gg, bb) = NImage.ToArgb(v);
                a += aa;
                r += rr;
                g += gg;
                b += bb;
            });

            a = (int)Math.Round((decimal)a / count);
            r = (int)Math.Round((decimal)r / count);
            g = (int)Math.Round((decimal)g / count);
            b = (int)Math.Round((decimal)b / count);

            return NImage.ToColor((a, r, g, b));
        }

        image.ForEachP((c, i, j) => Avg(ds.Select(d => (i + d.i, j + d.j)).Where(image.IsValid).Select(v => image[v])));

        return image;
    }

    public static NImage AddBitNoise(this NImage image, Random rnd, double noiseFactor = 0.3)
    {
        image.ps.ForEach((_, i, j) =>
        {
            if (rnd.NextDouble() < noiseFactor)
                image.ps[i][j] = black;
        });

        return image;
    }

    /// <summary>
    /// draw outside border
    /// </summary>
    public static NImage DrawRect(this NImage image, (int i, int j) p, (int i, int j) size, Color c, int thickness = 1)
    {
        var color = c.ToArgb();
        var t = thickness;

        (size.i + t, thickness).Range().ForEach(v =>
        {
            var (i, th) = (v.i - t, v.j + 1);
            image[(p.i + i, p.j - th)] = color;
            image[(p.i + i + t, p.j + th + size.j - 1)] = color;
        });

        (size.j + t, thickness).Range().ForEach(v =>
        {
            var (j, th) = (v.i - t, v.j + 1);
            image[(p.i - th, p.j + j + t)] = color;
            image[(p.i + th + size.i - 1, p.j + j)] = color;
        });

        return image;
    }

    public static NImage AddBitmap(this NImage image, (int i, int j) p, Bitmap bitmap, int colorLevel = 200)
    {
        (bitmap.Height, bitmap.Width).Range().ForEach(v =>
        {
            var c = bitmap.GetPixel(v.j, v.i);
            var isBlack = c.R < colorLevel && c.G < colorLevel && c.B < colorLevel;

            if (isBlack && image.IsValid((v.i + p.i, v.j + p.j)))
            {
                image.ps[v.i + p.i][v.j + p.j] = black;
            }
        });

        return image;
    }
}
