using AI.Images;
using Model.Extensions;
using System.Drawing;

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
        image.ModifyEachCij((c, i, j) => NImage.FromGray((int)Math.Round(255.0 * (c - a) / (b - a))));
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
                image[i, j] = 1;
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
                image[v.i + p.i, v.j + p.j] = 1;
            }
        });

        return image;
    }


    public static NImage Transform(this NImage image, Func<int, int> transformFn)
    {
        image.ModifyEachCij((c, i, j) => transformFn(c));
        return image;
    }

    public static NImage ApplyAvgFilter(this NImage image, int[][] matrix) =>
        ApplyFilter(image, matrix, matrix.AbsSum());

    public static NImage ApplyFilter(this NImage image, int[][] matrix, int divisor = 1)
    {
        var n = matrix.Length;
        var s = (n - 1) / 2;
        var pixelFn = image.pixelFn;
        var maxValue = (n, n).SelectRange((i, j) => matrix[i][j].Abs()).Sum() / divisor;
        var hasSign = (n, n).Range().Any(v => matrix[v.i][v.j] < 0);

        var resImg = new NImage(image.m, image.n, image.options.With(o => { o.MaxValue *= maxValue; o.HasSign |= hasSign; }));

        for (var i = 0; i < image.m; i++)
            for (var j = 0; j < image.n; j++)
            {
                int sum = 0;

                for (var mI = 0; mI < n; mI++)
                    for (var mJ = 0; mJ < n; mJ++)
                        sum += matrix[mI][mJ] * pixelFn(i + mI - s, j + mJ - s);

                resImg.ps[i, j] = sum / divisor;
            }

        return resImg;
    }

    public static NImage ApplyBorder(this NImage image, NImageBorderType borderType)
    {
        image.options.BorderType = borderType;
        return image;
    }

    public static NImage ApplyMaxPooling(this NImage image, int n) => ApplyMaxPooling(image, (n, 1));

    public static NImage ApplyMaxPooling(this NImage image, (int n, int m) s)
    {
        var resM = image.m * s.m / s.n + ((image.m * s.m) % s.n > 0 ? s.m : 0);
        var resN = image.n * s.m / s.n + ((image.n * s.m) % s.n > 0 ? s.m : 0);
        var resImg = new NImage(resM, resN, image.options);
        var pixelFn = image.pixelFn;

        for (var (sI, ssI) = (0, 0); sI < image.m; (sI, ssI) = (sI + s.n, ssI + 1))
            for (var (sJ, ssJ) = (0, 0); sJ < image.n; (sJ, ssJ) = (sJ + s.n, ssJ + 1))
            {
                var maxJs = (s.n).Range().Select(j => (j, max: (s.n).Range().Max(i => pixelFn(sI + i, sJ + j)))).OrderByDescending(v => v.max).Take(s.m).Select(v => v.j).ToArray();
                var maxIs = (s.n).Range().Select(i => (i, max: (s.n).Range().Max(j => pixelFn(sI + i, sJ + j)))).OrderByDescending(v => v.max).Take(s.m).Select(v => v.i).ToArray();

                for (var i = 0; i < s.m; i++)
                    for (var j = 0; j < s.m; j++)
                    {
                        resImg.ps[ssI + i, ssJ + j] = pixelFn(sI + maxIs[i], sJ + maxJs[j]);
                    }
            }

        return resImg;
    }

    public static NImage ApplyPooling(this NImage image, int n) => image.ApplyPooling((n, 1));

    public static NImage ApplyPooling(this NImage image, (int n, int m) s)
    {
        var resM = image.m * s.m / s.n + ((image.m * s.m) % s.n > 0 ? s.m : 0);
        var resN = image.n * s.m / s.n + ((image.n * s.m) % s.n > 0 ? s.m : 0);
        var resImg = new NImage(resM, resN, image.options);
        var pixelFn = image.pixelFn;

        var ss = (s.m).Range(i => (int)Math.Round(1.0 * i * (s.n - 1) / (s.m - 1))).ToArray();

        for (var (sI, ssI) = (0, 0); sI < image.m; (sI, ssI) = (sI + s.n, ssI + 1))
            for (var (sJ, ssJ) = (0, 0); sJ < image.n; (sJ, ssJ) = (sJ + s.n, ssJ + 1))
            {
                for (var i = 0; i < s.m; i++)
                    for (var j = 0; j < s.m; j++)
                    {
                        resImg.ps[ssI + i, ssJ + j] = pixelFn(sI + ss[i], sJ + ss[j]);
                    }
            }

        return resImg;
    }

    public static NImage ApplyBitFilter(this NImage image)
    {
        image.options.MaxValue = 1;

        for (var i = 0; i < image.m; i++)
            for (var j = 0; j < image.n; j++)
            {
                image.ps[i, j] = image.ps[i, j] > 0 ? 1 : 0;
            }

        return image;
    }

    public static NImage ApplyTopBitFilter(this NImage image, int top)
    {
        image.options.MaxValue = 1;

        for (var i = 0; i < image.m; i++)
            for (var j = 0; j < image.n; j++)
            {
                image.ps[i, j] = image.ps[i, j] >= top ? 1 : 0;
            }

        return image;
    }

    public static NImage ApplyTopFilter(this NImage image, int top)
    {
        image.options.MaxValue -= top - 1;

        for (var i = 0; i < image.m; i++)
            for (var j = 0; j < image.n; j++)
            {
                image.ps[i, j] = image.ps[i, j] >= top ? image.ps[i, j] - top + 1 : 0;
            }

        return image;
    }

    public static NImage ApplySumFilter(this NImage image, int n)
    {
        var pixelFn = image.pixelFn;
        var s = (n - 1) / 2;

        var resImg = new NImage(image.m, image.n, image.options.With(o => o.MaxValue = o.MaxValue * n * n));

        for (var i = 0; i < image.m; i++)
            for (var j = 0; j < image.n; j++)
            {
                int sum = 0;

                if (i == 0 || j == 0)
                {
                    for (var mI = 0; mI < n; mI++)
                        for (var mJ = 0; mJ < n; mJ++)
                            sum += pixelFn(i + mI - s, j + mJ - s);
                }
                else if (i == 0)
                {
                    // b = M + a;
                    var (i00, j00) = (i - s - 1, j - s - 1);
                    var M = (n).SelectRange(mJ => -pixelFn(i00, j00 + mJ) + pixelFn(i00 + n, j00 + mJ)).Sum();
                    sum = M + resImg.ps[i - 1, j - 1];
                }
                else if (j == 0)
                {
                    // c = M + a
                    var (i00, j00) = (i - s - 1, j - s - 1);
                    var M = (n).SelectRange(mI => -pixelFn(i00 + mI, j00) + pixelFn(i00 + mI, j00 + n)).Sum();
                    sum = M + resImg.ps[i - 1, j - 1];
                }
                else
                {
                    // d = M - a + b + c
                    var (i00, j00) = (i - s - 1, j - s - 1);
                    var (a00, a0n) = (pixelFn(i00, j00), -pixelFn(i00, j00 + n));
                    var (an0, ann) = (-pixelFn(i00 + n, j00), pixelFn(i00 + n, j00 + n));
                    var M = a00 + ann + a0n + an0;
                    sum = M - resImg.ps[i - 1, j - 1] + resImg.ps[i - 1, j] + resImg.ps[i, j - 1];
                }
                resImg.ps[i, j] = sum;
            }

        return resImg;
    }
}
