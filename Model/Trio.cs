using System.Collections.Generic;

namespace Model
{
    public struct Trio
    {
        public int i;
        public int j;
        public int k;

        public Trio(int i, int j, int k)
        {
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public bool IsStart => i == 0;

        public IEnumerable<Trio> SelectPairs()
        {
            yield return new Trio(i, j, k);
            yield return new Trio(j, k, i);
            yield return new Trio(k, i, j);
        }

        public override string ToString() => $"({i}, {j}, {k})";

        public int[] ToArray() => new[] { i, j, k };

        public static Trio Start = new Trio(0, 1, 2);
    }
}
