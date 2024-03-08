namespace Model3D.Voxels;

public static class VapeWorldValues
{
    public static VoxelMaterial DefaultMaterial => new VoxelMaterial()
    {
        power = 0.0002,
        powerRadius = 0.2,
        mass = 1,
    };

    public static VapeWorldOptions DefaultVapeWorldOptions => new VapeWorldOptions
    {
        VoxelSize = 0.059
    };


}
