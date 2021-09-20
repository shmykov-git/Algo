using Model.Tools;

namespace Model.Libraries
{
    public delegate int[] GraphVisitStrategy(int from, int to, int[] dirs);

    public static class GraphVisitStrateges
    {
        public static GraphVisitStrategy SimpleRandom(int seed)
        {
            var rnd = new Rnd(seed);

            return (from, to, dirs) => rnd.RandomList(dirs);
        }
    }
}
