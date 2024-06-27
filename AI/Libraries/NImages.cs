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

    public static IEnumerable<NImageInfo> GetSmileNoiseNetImages((int m, int n) count, (int m, int n) size, string smile, int smileN, int shiftN, double noiseFactor, Vectorizer vectorizer, Random rnd)
    {
        using var bitmap = vectorizer.GetTextBitmap(smile, smileN, "Arial", multX: 1.5);
        var (shiftI, shiftJ) = (11, 12);

        NImageInfo GetImage(int i, int j)
        {
            var img = new NImage(size, new NImageOptions() { MaxValue = 1, BorderType = NImageBorderType.Mirror });
            img.AddBitNoise(rnd, noiseFactor);
            var iSmile = i * (size.m - smileN - 2 * shiftN) / (count.m - 1) - shiftI + shiftN;
            var jSmile = j * (size.n - smileN - 2 * shiftN) / (count.n - 1) - shiftJ + shiftN;
            img.AddBitmap((iSmile, jSmile), bitmap);

            return new NImageInfo { i = i * count.m + j, pos = (iSmile + shiftI, jSmile + shiftJ), img = img };
        }

        for (int i = 0; i < count.m; i++)
            for (int j = 0; j < count.n; j++)
            {
                yield return GetImage(i, j);
            }
    }

    public static IEnumerable<NImageInfo> GetSmileNoiseImages(int count, int m, int n, string smile, int smileN, int shiftN, double noiseFactor, Vectorizer vectorizer, Random rnd)
    {
        using var bitmap = vectorizer.GetTextBitmap(smile, smileN, "Arial", multX: 1.5);
        var (shiftI, shiftJ) = (11, 12);

        NImageInfo GetImage(int k)
        {
            var img = new NImage(m, n);
            img.AddBitNoise(rnd, noiseFactor);
            var i = rnd.Next(m - smileN - 2 * shiftN) - shiftI + shiftN;
            var j = rnd.Next(n - smileN - 2 * shiftN) - shiftJ + shiftN;
            img.AddBitmap((i, j), bitmap);

            return new NImageInfo { i = k, pos = (i + shiftI, j + shiftJ), img = img};
        }

        for (int i = 0; i< count;i++)
        {
            yield return GetImage(i);
        }
    }
}
