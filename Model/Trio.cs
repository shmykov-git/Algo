using System.Collections.Generic;

namespace Model
{
    public struct Trio
    {
        public int I;
        public int J;
        public int K;

        public Trio(int i, int j, int k)
        {
            I = i;
            J = j;
            K = k;
        }

        public IEnumerable<Trio> SelectPairs()
        {
            yield return new Trio(I, J, K);
            yield return new Trio(J, K, I);
            yield return new Trio(K, I, J);
        }

        public override string ToString() => $"({I}, {J}, {K})";
    }
}
