using AI.Extensions;
using AI.Images;
using Model.Extensions;
using Model3D.Tools.Vectorization;

namespace AI.Libraries;

public static class NImages
{
    public static NImage[] GetNoiseImages(int count, int m, int n, double noiseFactor, Random rnd)
    {
        NImage GetImage()
        {
            var img = new NImage(m, n);
            img.AddBitNoise(rnd, noiseFactor);

            return img;
        }

        return (count).Range(_ => GetImage()).ToArray();
    }

    public static NImage[] GetSmileNoiseImages(int count, int m, int n, int smileN, int shiftN, double noiseFactor, Vectorizer vectorizer, Random rnd)
    {
        using var bitmap = vectorizer.GetTextBitmap("☺", smileN, "Arial", multX: 1.5);
        var (shiftI, shiftJ) = (10, 10);

        NImage GetImage()
        {
            var img = new NImage(m, n);
            img.AddBitNoise(rnd, noiseFactor);
            var i = rnd.Next(m - smileN - 2 * shiftN) - shiftI + shiftN;
            var j = rnd.Next(m - smileN - 2 * shiftN) - shiftJ + shiftN;
            img.AddBitmap((i, j), bitmap);

            return img;
        }

        return (count).Range(_ => GetImage()).ToArray();
    }
}
