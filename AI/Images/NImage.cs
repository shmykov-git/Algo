using Aspose.ThreeD.Entities;
using System.Drawing.Imaging;
using System.Drawing;
using Model.Extensions;
using Model3D.Tools.Vectorization;

namespace AI.Images;

public class NImage
{
    public const int white = 255 + (255 << 8) + (255 << 16) + (255 << 24);
    public const int black = (255 << 24);


    public int[][] ps;
    public int m => ps.Length;
    public int n => ps[0].Length;

    public NImage(int m, int n)
    {
        ps = (m).Range(_ => (n).Range(_ => white).ToArray()).ToArray();
    }

    public int this[(int i, int j) v] => ps[v.i][v.j];

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
