using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;

namespace Model3D.Voxels;

/// <summary>
/// Voxel shape
/// </summary>
public class Vape
{
    public VapeWorld World { get; }
    public Vector3 position;
    public Net3<Voxel> net;
    public List<Voxel> activeVoxelRoots = null;
    public Dictionary<(int, int), VoxelEdge> edges = new();
    //public List<VoxelEdge> edges;
    public Dictionary<Voxel, List<VoxelEdge>> edgeDic;


    //public Vape(VapeWorld world, IEnumerable<Voxel> voxels)
    //{
    //    this.World = world;
    //    CreateByVoxels(voxels);
    //}

    public Vape(VapeWorld world, IEnumerable<Voxel> voxels)
    {
        this.World = world;

        CreateByVoxels(voxels.Select(v =>
        {
            v.gi = world.voxelCount++;
            v.i++;
            return v;
        }));
    }

    public Vape(VapeWorld world, VoxelMaterial material, IEnumerable<Vector3> points)
    {
        this.World = world;

        CreateByVoxels(points.Select(p => new Voxel
        {
            gi = world.voxelCount++, // multithreading?
            position = p,
            material = material,
        }));
    }

    private void CreateByVoxels(IEnumerable<Voxel> voxelsEnumerator)
    {
        var voxels = voxelsEnumerator.ToArray();

        if (voxels.Any(v => v.vape != null))
            throw new ArgumentException("Cannot have more then one voxel vape");

        var c = voxels.Select(v => v.position).Center(); // todo: mass center

        voxels.ForEach(v => 
        {
            v.position -= c;
            v.vape = this;
        });

        position = c;
        net = new Net3<Voxel>(voxels, World.Options.VoxelSize, true, new Vector3(2, 2, 2));

        edges = new();
        edgeDic = voxels.ToDictionary(v => v, _ => new List<VoxelEdge>());

        void CreateEdge(Voxel a, Voxel b)
        {
            var key = (a.gi, b.gi).OrderedEdge();
            
            if (edges.TryGetValue(key, out var edge))
                return;

            edge = new VoxelEdge()
            {
                a = a,
                b = b
            };

            edges[key] = edge;
            edgeDic[a].Add(edge);
            edgeDic[b].Add(edge);
        }

        voxels.ForEach(a => net.SelectNeighborsByRadius(a, 1.4 * World.Options.VoxelSize).ForEach(b => CreateEdge(a, b)));

        //edges = voxels.SelectMany(a => net.SelectNeighborsByRadius(a, 1.4 * World.Options.VoxelSize).Select(b => CreateEdge(a, b))).ToList();
        //edgeDic = edges.GroupBy(e => e.a).Concat(edges.GroupBy(e => e.b)).ToDictionary(ge => ge.Key, ge => ge.ToList());

        World.Register(this);
    }

    public void RemoveEdge(VoxelEdge edge)
    {
        edges.Remove((edge.a.gi, edge.b.gi).OrderedEdge());
        edgeDic[edge.a].Remove(edge);
        edgeDic[edge.b].Remove(edge);
    }
}
