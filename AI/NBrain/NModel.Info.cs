using AI.Model;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NModel // Info
{
    public (int i, int j)[][] GetGraph() => nns.SkipLast(1).Select(ns => ns.SelectMany(n => n.es.Select(e => (e.a.i, e.b.i))).OrderBy(v => v).ToArray()).ToArray();
    public (int i, int j, double w)[][] GetState() => nns.SkipLast(1).Select(ns => ns.SelectMany(n => n.es.Select(e => (e.a.i, e.b.i, e.w))).OrderBy(v => v).ToArray()).ToArray();

    private Func<N, Vector3> TopologyPositionFn()
    {
        var maxLv = ns.Max(n => n.lv);
        double maxCount = ns.Max(n => nns[n.lv].Count);

        double GetY(N n)
        {
            double count = nns[n.lv].Count;
            var i = ns.Where(nn => nn.lv == n.lv).TakeWhile(nn => nn != n).Count();
            var step = maxCount / (count + 1);
            var oddShift = n.lv % 2 == 0 ? step / Math.Sqrt(31) : 0;

            return maxLv * step * (count - i) - oddShift;
        }

        double GetX(N n)
        {
            return n.lv * maxCount - 0.5 * maxCount;
        }

        return n => new Vector3(GetX(n), GetY(n), 0) / (maxLv * maxCount);
    }

    public Shape GetTopologyWeights(double mult)
    {
        var positionFn = TopologyPositionFn();

        var ps = ns.Select(positionFn).ToArray();
        var n = ps.Length;

        var wightPs = es.Select(e => 0.5 * (ps[e.a.i] + ps[e.b.i]) + new Vector3(0, 0, 0.5 * e.w * mult)).ToArray();
        var convexes = es.SelectMany((e, k) => new int[][] { [e.a.i, k + n], [k + n, e.b.i] }).ToArray();

        return new Shape()
        {
            Points3 = ps.Concat(wightPs).ToArray(),
            Convexes = convexes
        };
    }

    public Shape GetTopology()
    {
        var positionFn = TopologyPositionFn();

        return new Shape()
        {
            Points3 = ns.Select(positionFn).ToArray(),
            Convexes = es.Select(e => new int[] { e.a.i, e.b.i }).ToArray()
        };
    }
}
