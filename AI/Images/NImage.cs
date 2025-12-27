using AI.Extensions;
using AI.Libraries;
using Mapster;
using Model.Extensions;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace AI.Images;

public class NImage
{
    public const int alfa = unchecked((int)0xFF000000);
    public const int white = unchecked((int)0xFFFFFFFF);
    public const int black = unchecked((int)0xFF000000);
    public const int red = unchecked((int)0xFF990000);

    public NImageOptions options;
    public int[,] ps;

    public int m => ps.GetLength(0);
    public int n => ps.GetLength(1);

    public NImage((int m, int n) size, NImageOptions? options = null, int color = 0) : this(size.m, size.n, options, color)
    {
    }

    public NImage(int[][] values)
    {
        this.options = new() { MaxValue = values.AbsSum() };
        var m = values.Length;
        var n = values[0].Length;
        ps = new int[m, n];

        for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
            {
                ps[i, j] = values[i][j];
            }
    }

    public NImage(int m, int n, NImageOptions? options = null, int color = 0)
    {
        this.options = options?.Adapt<NImageOptions>() ?? new();
        ps = new int[m, n];

        if (color != 0)
        {
            for (var i = 0; i < m; i++)
                for (var j = 0; j < n; j++)
                {
                    ps[i, j] = color;
                }
        }
    }

    protected NImage(NImage image, Func<int, int>? transformFn)
    {
        var fn = transformFn ?? (i => i);
        options = image.options.Adapt<NImageOptions>();
        ps = new int[image.m, image.n];

        for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
            {
                ps[i, j] = fn(image.ps[i, j]);
            }
    }

    public double[] boxedPixels => pixels.Select(p => options.HasSign
        ? NValues.Boxed(p + options.MaxValue, options.MaxValue + options.MaxValue, options.BoxM)
        : NValues.Boxed(p, options.MaxValue, options.BoxM)).ToArray();

    public NImage Clone(Func<int, int>? transformFn = null) => new NImage(this, transformFn);

    public IEnumerable<int> pixels
    {
        get
        {
            for (var i = 0; i < m; i++)
                for (var j = 0; j < n; j++)
                {
                    yield return ps[i, j];
                }
        }
    }

    public int this[int i, int j]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ps[i, j];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (IsValid((i, j)))
                ps[i, j] = value;
        }
    }

    public int this[(int i, int j) v]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ps[v.i, v.j];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (IsValid(v))
                ps[v.i, v.j] = value;
        }
    }

    public Func<int, int, int> pixelFn => options.BorderType switch
    {
        NImageBorderType.Mirror => MirrorPixel,
        NImageBorderType.Padding => PaddingPixel,
        _ => throw new NotImplementedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int MirrorPixel(int i, int j)
    {
        if (i < 0)
            i = -i;
        else if (i >= m)
            i = m + m - 2 - i;

        if (j < 0)
            j = -j;
        else if (j >= n)
            j = n + n - 2 - j;

        return ps[i, j];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PaddingPixel(int i, int j) => 0 <= i && i < m && 0 <= j && j < n ? ps[i, j] : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte a, byte r, byte g, byte b) ToArgb(int c) =>
       ((byte)((0xFF000000 & c) >> 24),
        (byte)((0xFF0000 & c) >> 16),
        (byte)((0xFF00 & c) >> 8),
        (byte)(0xFF & c));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToGray(int c)
    {
        var (a, r, g, b) = ToArgb(c);
        return (r + g + b) / 3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToBit(int c)
    {
        return c == white ? 0 : 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromBit(int c)
    {
        return c == 1 ? black : white;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsIs(int c)
    {
        return c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromGray(int gray)
    {
        return alfa + (gray << 16) + (gray << 8) + gray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToColor((byte a, byte r, byte g, byte b) argb) => (argb.a << 24) + (argb.r << 16) + (argb.g << 8) + argb.b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToColor((int a, int r, int g, int b) argb) => (argb.a << 24) + (argb.r << 16) + (argb.g << 8) + argb.b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int a, int r, int g, int b) GetColor((int i, int j) v) => ToArgb(this[v]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetGray((int i, int j) v) => ToGray(this[v]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid((int i, int j) p)
    {
        return 0 <= p.i && p.i < m &&
               0 <= p.j && p.j < n;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ModifyEachCij(Func<int, int, int, int> getFn)
    {
        for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
                ps[i, j] = getFn(ps[i, j], i, j);
    }

    public void ForEachCij(Action<int, int, int> action)
    {
        for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
                action(ps[i, j], i, j);
    }

    public void SaveAsBitmap(string file)
    {
        using var bmp = new Bitmap(n, m);
        ForEachCij((c, i, j) => bmp.SetPixel(j, i, Color.FromArgb(c)));
        bmp.Save(file);
    }

    public NImage DebugShow()
    {
        var min = pixels.Min().Abs();
        var max = pixels.Max().Abs();
        var padLeft = Math.Max(min.ToString().Length, max.ToString().Length);

        var s = new StringBuilder();

        for (var i = 0; i < m; i++)
        {
            s.AppendLine();

            for (var j = 0; j < n; j++)
                s.Append($" {ps[i, j].ToString().PadLeft(padLeft)}");
        }

        Debug.WriteLine(s.ToString());

        return this;
    }
}
