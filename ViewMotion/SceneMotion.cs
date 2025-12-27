using Model.Extensions;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ViewMotion.Extensions;
using ViewMotion.Models;
using Color = System.Drawing.Color;
using Line = Model3D.Line3;
using Shape = Model.Shape;
using Vector3 = Model3D.Vector3;

namespace ViewMotion;

partial class SceneMotion
{


    public Task<Motion> Scene()
    {
        //var s = Shapes.Cube;

        //var line = new Line((-3, -2, -6), (7, 5, 5));
        //var lineShape = new Shape() { Points3 = [line.a, line.b], Convexes = [[0, 1]] };

        //var vs = new HashSet<Voxel>();

        //var cube = Shapes.Cube.Mult(9.9);

        //VoxelHelper.AppendShape(vs, cube);

        //var cubes = vs.Select(v => Shapes.PerfectCube.Move(v.ToVector())).ToSingleShape().Normalize();


        //return (cubes.ToMeta() + Shapes.CoodsWithText(3).Mult(2)).ToMotion(10);


        return MaterialActiveWorld();
    }
}