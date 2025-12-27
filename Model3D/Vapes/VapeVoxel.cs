using System;

namespace Model3D.Vapes;

public class VapeVoxel : INet3Item
{
    //<tmp>
    public int i;
    public int gi;
    //</tmp>

    //<action>
    public bool isActive;
    public Vector3 speed;
    //</action>

    public Vape vape;   // voxel owner

    public Vector3 position;
    public Vector3 gposition => position + vape.position;
    public VoxelMaterial material;
    //public Voxel[] links; // 28 dirs ??

    public Func<Vector3> PositionFn => () => position;
}
