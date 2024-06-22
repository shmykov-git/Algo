using Aspose.ThreeD.Entities;
using System.Drawing.Imaging;
using System.Drawing;
using Model.Extensions;
using Model3D.Tools.Vectorization;

namespace AI.Images;

public class NImage
{
    public const int alfa = unchecked((int)0xFF000000);
    public const int white = unchecked((int)0xFFFFFFFF);
    public const int black = unchecked((int)0xFF000000);
    public const int red = unchecked((int)0xFF990000);

    public int[][] ps;
    public int m => ps.Length;
    public int n => ps[0].Length;

    public NImage(int m, int n)
    {
        ps = (m).Range(_ => (n).Range(_ => white).ToArray()).ToArray();
    }

    protected NImage(NImage image) 
    {
        ps = image.ps.Select(line => line.ToArray()).ToArray();
    }

    public NImage Clone() => new NImage(this);

    public IEnumerable<int> grayPixels => ps.SelectMany().Select(ToGray);

    public int this[(int i, int j) v] 
    { 
        get => ps[v.i][v.j]; 
        set 
        { 
            if (IsValid(v)) 
                ps[v.i][v.j] = value; 
        }  
    }

    public static (byte a, byte r, byte g, byte b) ToArgb(int c) => 
       ((byte)((0xFF000000 & c) >> 24), 
        (byte)((0xFF0000 & c) >> 16), 
        (byte)((0xFF00 & c) >> 8), 
        (byte)(0xFF & c));

    public static int ToGray(int c)
    {
        var (a, r, g, b) = ToArgb(c);
        return (r + g + b) / 3;
    }

    public static int FromGray(int gray)
    {
        return alfa + (gray << 16) + (gray << 8) + gray;
    }

    public static int ToColor((byte a, byte r, byte g, byte b) argb) => (argb.a << 24) + (argb.r << 16) + (argb.g << 8) + argb.b;
    public static int ToColor((int a, int r, int g, int b) argb) => (argb.a << 24) + (argb.r << 16) + (argb.g << 8) + argb.b;

    public (int a, int r, int g, int b) GetColor((int i, int j) v) => ToArgb(this[v]);
    public int GetGray((int i, int j) v) => ToGray(this[v]);

    public bool IsValid((int i, int j) p)
    {
        return 0 <= p.i && p.i < m &&
               0 <= p.j && p.j < n;
    }

    public void ForEachP(Func<int, int, int, int> func)
    {
        (m, n).Range().ForEach(v => ps[v.i][v.j] = func(ps[v.i][v.j], v.i, v.j));
    }

    public void SaveAsBitmap(string file)
    {
        using var bmp = new Bitmap(n, m);
        ps.ForEach((c, i, j) => bmp.SetPixel(j, i, Color.FromArgb(c)));
        bmp.Save(file);
    }
}
