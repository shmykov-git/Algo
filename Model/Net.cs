using Model.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    /// <summary>
    /// 2D local collisions complexity optimization from O(n^2) to O(n)
    /// </summary>
    public class Net<TNetKey, TNetValue> where TNetKey : INetKey
    {
        public int nx;
        public int ny;
        private readonly double fromX;
        private readonly double toX;
        private readonly double fromY;
        private readonly double toY;
        private readonly double step;
        private List<TNetValue>[][] data;

        public int Ix(double x) => (int)((x - fromX) / step);
        public int Jy(double y) => (int)((y - fromY) / step);
        //public int Good(int i, int max) => i < 0 ? 0 : (i > max ? max : i);
        //public int GIx(double x) => Good(Ix(x), nx-1);
        //public int GJy(double y) => Good(Jy(y), ny-1);
        public bool IsGoodI(int i) => i >= 0 && i < nx;
        public bool IsGoodJ(int j) => j >= 0 && j < ny;

        public Net(IEnumerable<(TNetKey key, TNetValue value)> keyValues, double step) : this(keyValues.Select(kv=>kv.key), step)
        {
            AddRange(keyValues);
        }

        public Net(IEnumerable<TNetKey> keys, double step)
        {
            this.fromX = keys.Min(key => key.x);
            this.toX = keys.Max(key => key.x);
            this.fromY = keys.Min(key => key.y);
            this.toY = keys.Max(key => key.y);
            this.step = step;

            InitData();
        }

        public Net(double fromX, double toX, double fromY, double toY, double step)
        {
            this.fromX = fromX;
            this.toX = toX;
            this.fromY = fromY;
            this.toY = toY;
            this.step = step;

            InitData();
        }

        private void InitData()
        {
            this.ny = (int)((toY - fromY) / step) + 1;
            this.nx = (int)((toX - fromX) / step) + 1;
            data = (nx).SelectRange(i => (ny).SelectRange(j => new List<TNetValue>()).ToArray()).ToArray();
        }

        public void Add(TNetKey key, TNetValue value)
        {
            data[Ix(key.x)][Jy(key.y)].Add(value);
        }

        public void AddRange(IEnumerable<(TNetKey key, TNetValue value)> items)
        {
            foreach (var item in items)
                Add(item.key, item.value);
        }

        private static readonly (int i, int j)[] dirs = { (-1, -1), (0, -1), (1, -1), (-1, 0), (0, 0), (1, 0), (-1, 1), (0, 1), (1, 1) };

        public IEnumerable<TNetValue> SelectNeighbors(TNetKey item)
        {
            var ii = Ix(item.x);
            var jj = Jy(item.y);

            return dirs.Select(v => (i: ii + v.i, j: jj + v.j))
                .Where(v => IsGoodI(v.i) && IsGoodJ(v.j))
                .SelectMany(v => data[v.i][v.j]);
        }
    }
}
