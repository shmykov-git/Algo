using Model.Extensions;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model3D.Vapes;

public class VapeWorld
{
    public VapeWorldOptions Options { get; }
    public List<Vape> vapes = new List<Vape>();
    public List<VapeVoxel> voxels = new List<VapeVoxel>();
    private double interactionRadius;
    private double linkRadius;
    public int voxelCount;

    public VapeWorld(VapeWorldOptions options)
    {
        this.Options = options;
        interactionRadius = options.VoxelSize;
        linkRadius = 1.4 * interactionRadius;
    }

    public void Register(Vape vape)
    {
        vapes.Add(vape);
        voxels.AddRange(vape.net.NetItems);
    }

    class ProcVoxel
    {
        public bool isActive;
        public bool visited;
        public VapeVoxel voxel;
        //public Vector3 acc = Vector3.Origin;
        public Vector3 acc = Vector3.Origin;

        public double mass => voxel.material.mass;
    }

    public void Step()
    {
        Options.stepFuncExecCount = 0;

        var procVoxels = new Dictionary<int, ProcVoxel>();
        var stack = new Stack<ProcVoxel>();

        void MaterialCalc(ProcVoxel a, ProcVoxel b, VoxelEdge e)
        {
            Options.stepFuncExecCount++;

            a.isActive = true;

            var direction = b.voxel.position - a.voxel.position;
            var force = VapeWorldRules.MaterialForce(e.materialPower, 0.75, direction.Length / e.materialPowerRadius);
            var impulse = e.mass / a.mass;
            var accLen = force * impulse;
            var acc = direction.ToLenWithCheck(accLen);

            a.acc += acc;
        }

        void InteractionCalc(ProcVoxel a, ProcVoxel b)
        {
            Options.stepFuncExecCount++;

            a.isActive = true;

            var materialPower = 0.5 * (a.voxel.material.power + b.voxel.material.power);
            var materialPowerRadius = 0.5 * (a.voxel.material.powerRadius + b.voxel.material.powerRadius);
            var interactionMass = 0.5 * (a.voxel.material.mass + b.voxel.material.mass);

            var direction = b.voxel.gposition - a.voxel.gposition;
            var force = VapeWorldRules.InteractionForce(materialPower, 0.75, direction.Length / materialPowerRadius);
            var impulse = interactionMass / a.mass;
            var accLen = force * impulse;
            var acc = direction.ToLenWithCheck(accLen);

            a.acc += acc;
        }

        ProcVoxel GetPV(VapeVoxel voxel)
        {
            if (procVoxels.TryGetValue(voxel.gi, out var pv))
                return pv;

            pv = new ProcVoxel() { voxel = voxel, isActive = false };
            procVoxels[voxel.gi] = pv;
            stack.Push(pv);

            return pv;
        }

        vapes.ForEach(v => v.net.Update());

        foreach (var vape in vapes)
        {
            if (vape.activeVoxelRoots != null)
                foreach (var voxel in vape.activeVoxelRoots)
                {
                    GetPV(voxel);
                }
        }

        while (stack.TryPop(out var pA))
        {
            if (pA.visited)
                continue;

            pA.visited = true;
            var vapeA = pA.voxel.vape;

            var links = vapeA.edgeDic[pA.voxel].Select(e => (e, b: e.another(pA.voxel))).ToArray();
            links.ForEach(l => MaterialCalc(pA, GetPV(l.b), l.e));

            foreach (var vapeB in vapes) // todo: Vape world net
            {
                var closeVoxels = vapeB.net.SelectItemsByRadius(pA.voxel.gposition - vapeB.position, Options.VoxelSize).Where(b => pA.voxel != b && !links.Any(l => l.b == b)).ToArray();

                //if (closeVoxels.Length > 0)
                //    Debugger.Break();

                closeVoxels.ForEach(b => InteractionCalc(pA, GetPV(b)));

                //vapeB.edges.Values.ToArray().ForEach(e =>
                //{
                //    if ((e.a.gposition - e.b.gposition).Length2 > e.materialDestroyRadius.Pow2())
                //        vapeB.RemoveEdge(e);
                //});
            }
        }

        foreach (var pv in procVoxels.Values/*.Where(v => v.isActive)*/)
        {
            pv.voxel.speed += pv.acc;
            //pv.voxel.speed *= pv.voxel.material.damping; // todo: split (move, rotate, material)

            if (pv.voxel.speed.Length2 < Options.InactiveSpeed.Pow2())
                pv.isActive = false;
        }

        voxels.ForEach(v => v.position += v.speed);

        vapes.ForEach(v => v.activeVoxelRoots = null);

        foreach (var (vape, activeVoxelRoots) in procVoxels.Values.Where(v => v.isActive).GroupBy(r => r.voxel.vape).Select(gv => (gv.Key, gv.Select(v => v.voxel).ToList())))
        {
            vape.activeVoxelRoots = activeVoxelRoots;
        }

    }


}
