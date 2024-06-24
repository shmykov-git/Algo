using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using AI.Images;
using AI.Libraries;
using AI.Model;
using meta.Extensions;
using Model;
using Model.Extensions;

namespace AI.Extensions;

public static class NImageExtensions
{
    public const int alfa = NImage.alfa;
    public const int white = NImage.white;
    public const int black = NImage.black;
    public const int red = NImage.red;

    public static NImage SwitchAlfa(this NImage image)
    {
        image.ModifyEachCij((c, i, j) => c ^ alfa);
        return image;
    }

    public static NImage NormToGray(this NImage image)
    {
        var a = image.pixels.Min();
        var b = image.pixels.Max();
        image.ModifyEachCij((c, i, j) => NImage.FromGray((int)Math.Round(255.0*(c - a)/(b - a))));
        return image;
    }

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

        image.ModifyEachCij((c, i, j) => Avg(ds.Select(d => (i + d.i, j + d.j)).Where(image.IsValid).Select(v => image[v])));

        return image;
    }

    public static NImage AddBitNoise(this NImage image, Random rnd, double noiseFactor = 0.3)
    {
        image.ForEachCij((_, i, j) =>
        {
            if (rnd.NextDouble() < noiseFactor)
                image[i, j] = black;
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
                image[v.i + p.i, v.j + p.j] = black;
            }
        });

        return image;
    }


    public static NImage ApplySobelFilter(this NImage image) =>
        image.ApplyFilter(NValues.SobelMatix, i => i, i => true);

    public static NImage Transform(this NImage image, Func<int, int> transformFn)
    {
        image.ModifyEachCij((c, i, j) => transformFn(c));
        return image;
    }

    public static NImage ApplySumFilter(this NImage image, Func<int, int> transformFn) =>
        image.ApplyFilter(NValues.SumMatrix3, transformFn, i => true);

    public static NImage ApplyFilter(this NImage image, Matrix m, Func<int, int> transformFn, Func<int, bool>? takeFn = null)
    {
        var wFn = takeFn ?? (_ => true);
        var sI = m.M / 2 - 1;
        var sJ = m.N / 2 - 1;

        var iis = (image.m).Range().Where(i => wFn(i)).Select((i, ii) => (ii, i)).ToArray();
        var jjs = (image.n).Range().Where(j => wFn(j)).Select((j, jj) => (jj, j)).ToArray();

        var img = new NImage(iis.Length, jjs.Length);

        double PixelFn(int i, int j, int ii, int jj)
        {
            return m[ii][jj] * transformFn(image.MirrorPixel(i + ii - sI, j + jj - sJ));
        }

        (iis, jjs).ForCross((a, b) =>
        {
            var pixel = (m.M, m.N).SelectRange((ii, jj) => PixelFn(a.i, b.j, ii, jj)).Sum();
            img[(a.ii, b.jj)] = (int)Math.Round(pixel);
        });

        return img;
    }

    public static NImage ApplySumFilter(this NImage image, int n, Func<int, int> transformFn, Func<int, bool>? takeFn = null)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TakeAll(int _) => true;

        var wFn = takeFn ?? TakeAll;
        var s = n / 2 - 1;

        var imIs = (image.m).Range().Where(i => wFn(i)).ToArray();
        var imJs = (image.n).Range().Where(j => wFn(j)).ToArray();

        var resImg = new NImage(imIs.Length, imJs.Length, false);

        for (var i = 0; i < imIs.Length; i++)
            for (var j = 0; j < imJs.Length; j++)
            {
                int sum = 0;

                for (var mI = 0; mI < n; mI++)
                    for (var mJ = 0; mJ < n; mJ++)
                        sum += transformFn(image.MirrorPixel(imIs[i] + mI - s, imJs[j] + mJ - s));

                resImg.ps[i, j] = sum;
            }

        return resImg;
    }
}
