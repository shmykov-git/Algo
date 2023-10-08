using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Tools;

namespace Model3D
{
    public interface INet3Item
    {
        public Func<Vector3> PositionFn { get; }
    }

    /// <summary>
    /// 3D local collisions complexity optimization from O(n^2) to O(n)
    /// </summary>
    // todo: net should support out items
    public class Net3<TNetItem> where TNetItem : INet3Item
    {
        public int nx;
        public int ny;
        public int nz;

        private Vector3 from;
        private Vector3 to;

        private readonly double netSize;
        private List<Item>[][][] netData;
        private List<Item> data;

        public Vector3[] NetField => netData.SelectMany((a, i) =>
            a.SelectMany((b, j) => b.Select((c, k) => netSize * new Vector3(i + 0.5, j + 0.5, k + 0.5) + from))).ToArray();

        public TNetItem[] NetItems => data.Select(item=>item.Value).ToArray();

        public (int i, int j, int k) GetIndex(Vector3 v) => (
            (int) ((v.x - from.x) / netSize),
            (int) ((v.y - from.y) / netSize),
            (int) ((v.z - from.z) / netSize));

        public bool IsGood((int i, int j, int k) v) => 
            v.i >= 0 && v.i < nx && 
            v.j >= 0 && v.j < ny &&
            v.k >= 0 && v.k < nz;

        public Net3(TNetItem[] items, Vector3 from, Vector3 to, double netSize) : this(from, to, netSize)
        {
            AddItems(items);
        }

        public Net3(TNetItem[] items, double netSize) : this(items.Select(kv => kv.PositionFn), netSize)
        {
            AddItems(items);
        }

        private Net3(IEnumerable<Func<Vector3>> posFns, double netSize)
        {
            var positions = posFns.Select(fn => fn()).ToArray();

            this.from = new Vector3(positions.Min(p => p.x), positions.Min(p => p.y), positions.Min(p => p.z));
            this.to = new Vector3(positions.Max(p => p.x), positions.Max(p => p.y), positions.Max(p => p.z));
            this.netSize = netSize;

            InitNet();
        }

        public Net3(Vector3 from, Vector3 to, double netSize)
        {
            this.from = from;
            this.to = to;
            this.netSize = netSize;

            InitNet();
        }

        public void AddItems(TNetItem[] items)
        {
            foreach (var item in items)
                Add(item.PositionFn, item);
        }

        private void InitNet()
        {
            this.nx = (int)((to.x - from.x) / netSize) + 1;
            this.ny = (int)((to.y - from.y) / netSize) + 1;
            this.nz = (int)((to.z - from.z) / netSize) + 1;

            netData = (nx).SelectRange(i => (ny).SelectRange(j => (nz).SelectRange(k => new List<Item>()).ToArray()).ToArray()).ToArray();
            data = new List<Item>();
        }

        private void Add(Func<Vector3> posFn, TNetItem value)
        {
            var pos = posFn();
            var index = GetIndex(pos);

            var item = new Item()
            {
                Value = value,
                Index = index,
                Fn = posFn
            };

            data.Add(item);
            netData[index.i][index.j][index.k].Add(item);
        }

        private static readonly (int i, int j, int k)[] dirs = (3, 3, 3).SelectRange((i, j, k) => (i - 1, j - 1, k - 1))
            .ToArray();

        public IEnumerable<TNetItem> SelectNeighbors(Vector3 item)
        {
            var v0 = GetIndex(item);

            return dirs.Select(v => (i: v0.i + v.i, j: v0.j + v.j, k: v0.k + v.k))
                .Where(IsGood)
                .SelectMany(v => netData[v.i][v.j][v.k].Select(vv => vv.Value));
        }

        public IEnumerable<TNetItem> SelectItemsByRadius(Vector3 position, double radius)
        {
            var r2 = radius * radius;

            return SelectNeighbors(position).Where(n => (n.PositionFn() - position).Length2 < r2);
        }

        public void Update()
        {
            var updates = data.Select(v => (item: v, index: v.Index, newIndex: GetIndex(v.Fn()))).Where(v => v.index != v.newIndex).ToArray();

            foreach (var v in updates)
            {
                var list = netData[v.index.i][v.index.j][v.index.k];
                list.Remove(v.item);
                
                if (IsGood(v.newIndex))
                {
                    v.item.Index = v.newIndex;
                    netData[v.newIndex.i][v.newIndex.j][v.newIndex.k].Add(v.item);
                }
            }
        }

        class Item
        {
            public TNetItem Value;
            public (int i, int j, int k) Index;
            public Func<Vector3> Fn;
        }
    }
}
