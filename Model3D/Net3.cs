using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;

namespace Model3D
{
    /// <summary>
    /// 3D local collisions complexity optimization from O(n^2) to O(n)
    /// </summary>
    public class Net3<TNetValue>
    {
        public int nx;
        public int ny;
        public int nz;

        private readonly double fromX;
        private readonly double toX;
        private readonly double fromY;
        private readonly double toY;
        private readonly double fromZ;
        private readonly double toZ;

        private readonly double step;
        private List<Item>[][][] data;

        public int Ix(double x) => (int)((x - fromX) / step);
        public int Jy(double y) => (int)((y - fromY) / step);
        public int Kz(double z) => (int)((z - fromZ) / step);

        public (int i, int j, int k) GetIndex(Vector3 v) => (Ix(v.x), Jy(v.y), Kz(v.z));

        public bool IsGoodI(int i) => i >= 0 && i < nx;
        public bool IsGoodJ(int j) => j >= 0 && j < ny;
        public bool IsGoodK(int k) => k >= 0 && k < nz;

        public bool IsGood((int i, int j, int k) v) => IsGoodI(v.i) && IsGoodJ(v.j) && IsGoodK(v.k);

        public Net3(IEnumerable<(Func<Vector3> key, TNetValue value)> posFns, double step) : this(posFns.Select(kv=>kv.key), step)
        {
            AddRange(posFns);
        }

        public Net3(IEnumerable<Func<Vector3>> posFns, double step)
        {
            var keys = posFns.Select(fn => fn()).ToArray();

            this.fromX = keys.Min(key => key.x);
            this.toX = keys.Max(key => key.x);
            this.fromY = keys.Min(key => key.y);
            this.toY = keys.Max(key => key.y);
            this.fromZ = keys.Min(key => key.z);
            this.toZ = keys.Max(key => key.z);
            this.step = step;

            InitData();
        }

        private Net3(double fromX, double toX, double fromY, double toY, double fromZ, double toZ, double step)
        {
            this.fromX = fromX;
            this.toX = toX;
            this.fromY = fromY;
            this.toY = toY;
            this.fromZ = fromZ;
            this.toZ = toZ;
            this.step = step;

            InitData();
        }

        private void InitData()
        {
            this.nx = (int)((toX - fromX) / step) + 1;
            this.ny = (int)((toY - fromY) / step) + 1;
            this.nz = (int)((toZ - fromZ) / step) + 1;

            data = (nx).SelectRange(i => (ny).SelectRange(j => (nz).SelectRange(k => new List<Item>()).ToArray()).ToArray()).ToArray();
        }

        class Item
        {
            public TNetValue Value;
            public (int i, int j, int k) Index;
            public Func<Vector3> Fn;
        }

        private void Add(Func<Vector3> posFn, TNetValue value)
        {
            var pos = posFn();
            var index = GetIndex(pos);

            var item = new Item()
            {
                Value = value,
                Index = index,
                Fn = posFn
            };

            data[index.i][index.j][index.k].Add(item);
        }

        public void AddRange(IEnumerable<(Func<Vector3> posFns, TNetValue value)> items)
        {
            foreach (var item in items)
                Add(item.posFns, item.value);
        }

        private static readonly (int i, int j, int k)[] dirs = (3, 3, 3).SelectRange((i, j, k) => (i - 1, j - 1, k - 1))
            .ToArray();

        public IEnumerable<TNetValue> SelectNeighbors(Vector3 item)
        {
            var ii = Ix(item.x);
            var jj = Jy(item.y);
            var kk = Kz(item.z);

            return dirs.Select(v => (i: ii + v.i, j: jj + v.j, k: kk + v.k))
                .Where(IsGood)
                .SelectMany(v => data[v.i][v.j][v.k].Select(vv => vv.Value));
        }

        public void Update()
        {
            var updates = data.SelectMany(a => a.SelectMany(b => b.SelectMany(c =>
                c.Select(item => (list:c, item, newIndex: GetIndex(item.Fn()))).Where(v => v.item.Index != v.newIndex)))).ToArray();

            foreach (var update in updates)
            {
                update.list.Remove(update.item);
                
                if (IsGood(update.newIndex))
                {
                    update.item.Index = update.newIndex;
                    data[update.newIndex.i][update.newIndex.j][update.newIndex.k].Add(update.item);
                }
            }
        }

        //public IEnumerable<TNetValue> SelectNeighbors(Vector3 item, int distance)
        //{
        //    var ii = Ix(item.x);
        //    var jj = Jy(item.y);
        //    var kk = Kz(item.z);

        //    return (2 * distance + 1, 2 * distance + 1, 2 * distance + 1)
        //        .SelectRange((i, j, k) => (i: ii + i, j: jj + j, k: kk + k))
        //        .Where(v => IsGoodI(v.i) && IsGoodJ(v.j) && IsGoodK(v.k))
        //        .SelectMany(v => data[v.i][v.j][v.k]);
        //}
    }
}
