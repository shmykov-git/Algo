using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Tools
{
    public class Rnd
    {
        private readonly Random rnd;

        public Rnd(int seed = 0)
        {
            rnd = new Random(seed);
        }

        public T[] RandomList<T>(T[] list) => RandomIndices(list.Length).Select(i => list[i]).ToArray();

        public int[] RandomIndices(int n)
        {
            return (n).SelectRange(i => (i, rnd: rnd.NextDouble())).OrderBy(v => v.rnd).Select(v => v.i).ToArray();
        }

        public T[] RandomProbList<T>(T[] list, double[] probabilities, int count = 2) => RandomIndices(probabilities, count).Select(i => list[i]).ToArray();

        public int[] RandomIndices(double[] probabilities, int count = 2)
        {
            var res = (probabilities.Length).SelectRange(i => i).ToArray();

            var probs = probabilities.ToList();
            var mult = 1.0;

            for (var i = 0; i < count; i++)
            {
                var k = GetIndex(probs, mult * rnd.NextDouble());

                Reversal(res, res.Length - 1 - i, k);
                mult -= probs[k];
                probs.RemoveAt(k);
            }

            return res;
        }

        private void Reversal(int[] values, int from, int to)
        {
            var tmp = values[from];
            values[from] = values[to];
            values[to] = tmp;
        }

        private int GetIndex(List<double> probabilities, double value)
        {
            var sum = 0.0;

            for (var i = 0; i < probabilities.Count; i++)
            {
                sum += probabilities[i];

                if (value < sum)
                    return i;
            }

            throw new ApplicationException($"Incorrect probability list or value");
        }
    }
}
