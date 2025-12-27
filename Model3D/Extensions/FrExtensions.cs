using Meta;
using Model;
using Model.Extensions;
using Model.Fourier;
using Model3D.Tools.Vectorization;
using System.Drawing;

namespace Model3D.Extensions
{
    public static class FrExtensions
    {
        private static readonly Vectorizer vectorizer = DI.Get<Vectorizer>();

        public static Shape SearchShape(this Fr[] fShape, int n, (int iFrom, int iCount) ii, (int jFrom, int jCount) jj)
        {
            var searchShape = (ii, jj).SelectRange((i, j) =>
                (fShape.ModifyTwoLasts((a, b) =>
                 {
                     a.n = i;
                     b.n = j;
                 }).ToShape(n, 0.01).ApplyColor(Color.Blue)
                 + vectorizer.GetTextObsolet($"{i} {j}").Perfecto(0.3).MoveY(-0.7).MoveZ(0.005).ToLines(1, Color.Red)
                ).MoveX(2 * j).MoveY(2 * i)).ToSingleShape();

            return searchShape;
        }
    }
}