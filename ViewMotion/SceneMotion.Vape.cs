using Model.Extensions;
using Model3D.Extensions;
using Model3D.Vapes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ViewMotion.Extensions;
using ViewMotion.Models;
using Shape = Model.Shape;
using Vector3 = Model3D.Vector3;

namespace ViewMotion;

partial class SceneMotion
{

    private Shape ToShapeForVape(Vape vape)
    {
        var s = new Shape
        {
            Points3 = vape.net.NetItems.Select(v => v.position + vape.position).ToArray(),
            Convexes = vape.edges.Values.Select(e => (e.a.i, e.b.i).OrderedEdge()).Distinct().Select(e => new[] { e.i, e.j }).ToArray()
        };

        return s;
    }

    public Task<Motion> SceneVape()
    {
        var nMotion = 1000;

        //var pointsA = Shapes.Cube.Perfecto().SplitPlanes(0.1).Points3;
        var n = 5;
        var nB = 3;

        var mult = 1.0 / (n - 1);
        var pointsA = (n, n, n).SelectRange((i, j, k) => mult * (new Vector3(i, j, k) - (n - 1) * new Vector3(0.5, 0.5, 0.5))).ToArray();
        var pointsB = (nB, nB, nB).SelectRange((i, j, k) => mult * (new Vector3(i, j, k) - (nB - 1) * new Vector3(0.5, 0.5, 0.5))).ToArray();
        var voxelSize = 1.0 / (n - 1);

        var world = new VapeWorld(new VapeWorldOptions
        {
            VoxelSize = voxelSize,
            InteractionMult = 1.4,
            InactiveSpeed = 0.00001
        });

        var material = new VoxelMaterial()
        {
            power = 0.0002,
            powerRadius = voxelSize,
            destroyRadius = 2 * voxelSize,
            damping = 0.95,
            mass = 1,
            interaction = new VoxelInteraction
            {
                power = 0.0002,
                powerRadius = 0.9 * voxelSize
            }
        };

        var vapeA = new Vape(world, pointsA.Select(p => new VapeVoxel
        {
            position = p,
            speed = Vector3.Origin,
            material = material
        }))
        {
            position = new Vector3(0, 0, 1)
        };

        var vapeB = new Vape(world, pointsB.Select(p => new VapeVoxel
        {
            position = p,
            speed = new Vector3(0, 0, 0.01),
            material = material
        }))
        {
            position = new Vector3(0, 0, -3)
        };



        IEnumerable<Shape> Animate()
        {
            for (var step = 0; step < nMotion; step++)
            {
                vapeB.activeVoxelRoots = new();
                vapeB.net.NetItems.ForEach(v => vapeB.activeVoxelRoots.Add(v));
                vapeA.activeVoxelRoots = new();
                vapeA.net.NetItems.ForEach(v => vapeA.activeVoxelRoots.Add(v));


                var sw = Stopwatch.StartNew();
                world.Step();
                //Debug.WriteLine($"{step,-2} step: {sw.ElapsedMilliseconds:F0}ms ({vapeA.World.Options.stepFuncExecCount})");

                yield return new[]
                {
                    ToShapeForVape(vapeA),
                    ToShapeForVape(vapeB)
                }.ToSingleShape().Mult(3).ToMeta(multPoint: 5);
                //yield return ToShape(vapeA).Mult(3).ToSpots3(1, Color.Blue, Shapes.Cube.Centered().ToLines().Mult(40 * 3 * voxelSize));
            }
        }

        return Animate().ToMotion();
    }
}
