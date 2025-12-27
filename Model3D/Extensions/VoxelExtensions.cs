using System;

namespace Model3D.Extensions;

public static class VoxelExtensions
{
    public static Voxel ToVoxel(this Vector3 v) => new Voxel()
    {
        i = (int)Math.Floor(v.x),
        j = (int)Math.Floor(v.y),
        k = (int)Math.Floor(v.z)
    };

    public static Voxel ToSignVoxel(this Vector3 v) => new Voxel()
    {
        i = Math.Sign(v.x),
        j = Math.Sign(v.y),
        k = Math.Sign(v.z)
    };

    public static Vector3 ToVector(this Voxel v)
    {
        return new Vector3(v.i, v.j, v.k);
    }
}
